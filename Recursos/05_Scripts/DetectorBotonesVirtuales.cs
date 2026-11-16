// FantasyAR - Detector de botones virtuales por oclusión (equivalente a los Virtual Buttons de Vuforia,
// que fueron retirados en Vuforia Engine 11).
//
// Funcionamiento:
//  1. Se pide a Vuforia la imagen de la cámara en escala de grises (CameraDevice.GetCameraImage).
//  2. Para cada botón se proyecta una rejilla de puntos de la zona del dibujo sobre la imagen de la cámara,
//     usando la pose del Image Target y los parámetros de la lente (GetCameraIntrinsics).
//  3. Se compara (correlación normalizada) lo que ve la cámara con el dibujo original.
//  4. Si la zona deja de parecerse al dibujo (un dedo la tapa) mientras los otros botones siguen visibles,
//     el botón se considera PRESIONADO y se dispara su evento.
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class DetectorBotonesVirtuales : MonoBehaviour
{
    [Header("Referencias")]
    public ImageTargetBehaviour imageTarget;
    [Tooltip("El mismo dibujo subido a Vuforia (con Read/Write activado)")]
    public Texture2D dibujoReferencia;
    public Camera camaraAR;

    [Header("Ajustes de detección")]
    [Tooltip("Por debajo de este valor la zona se considera tapada")]
    public float umbralPresionado = 0.3f;
    [Tooltip("Por encima de este valor la zona se considera libre otra vez")]
    public float umbralLiberado = 0.5f;
    [Tooltip("Al menos otro botón debe verse con esta similitud (evita falsos positivos por desenfoque)")]
    public float umbralOtrosVisibles = 0.45f;
    public int lecturasParaConfirmar = 3;
    public float intervaloLectura = 0.05f;
    public int puntosPorLado = 8;

    [Header("Depuración")]
    public bool mostrarDepuracion = false;

    BotonVirtualAR[] botones;
    float[] referencia;
    bool formatoActivo;
    float ultimaLectura;
    string estadoCamara = "esperando Vuforia";
    int lecturas;

    // Calibración automática: cómo se relaciona la cámara de Unity con la imagen del sensor
    // (8 candidatos = 4 rotaciones x espejo). Depende del dispositivo y de la orientación de pantalla.
    int orientacion = -1;
    readonly float[] sumaCalibracion = new float[8];
    int lecturasCalibracion;
    float ultimaCalibracion;

    void Start()
    {
        if (imageTarget == null) imageTarget = GetComponentInParent<ImageTargetBehaviour>();
        if (camaraAR == null) camaraAR = Camera.main;
        botones = imageTarget.GetComponentsInChildren<BotonVirtualAR>(true);
        PrepararReferencia();
        VuforiaApplication.Instance.OnVuforiaStarted += ActivarFormato;
        if (VuforiaApplication.Instance.IsRunning) ActivarFormato();
    }

    void OnDestroy()
    {
        if (VuforiaApplication.Instance != null) VuforiaApplication.Instance.OnVuforiaStarted -= ActivarFormato;
    }

    void ActivarFormato()
    {
        formatoActivo = VuforiaBehaviour.Instance.CameraDevice.SetFrameFormat(PixelFormat.GRAYSCALE, true);
        estadoCamara = formatoActivo ? "imagen GRAYSCALE activa" : "no se pudo activar GRAYSCALE";
    }

    bool Rastreado =>
        imageTarget != null && (imageTarget.TargetStatus.Status == Status.TRACKED
                                || imageTarget.TargetStatus.Status == Status.EXTENDED_TRACKED);

    IEnumerable<Vector3> PuntosLocales(BotonVirtualAR b)
    {
        var c = b.CentroLocal;
        for (int j = 0; j < puntosPorLado; j++)
            for (int i = 0; i < puntosPorLado; i++)
            {
                float fx = (i + 0.5f) / puntosPorLado - 0.5f;
                float fz = (j + 0.5f) / puntosPorLado - 0.5f;
                yield return new Vector3(c.x + fx * b.tamano.x, 0f, c.y + fz * b.tamano.y);
            }
    }

    void PrepararReferencia()
    {
        var size = imageTarget.GetSize();
        int n = puntosPorLado * puntosPorLado;
        referencia = new float[botones.Length * n];
        for (int b = 0; b < botones.Length; b++)
        {
            int k = 0;
            foreach (var p in PuntosLocales(botones[b]))
                referencia[b * n + k++] = dibujoReferencia.GetPixelBilinear(p.x / size.x + 0.5f, p.z / size.y + 0.5f).grayscale;
        }
    }

    void Update()
    {
#if ENABLE_INPUT_SYSTEM
        // Doble toque en la pantalla (o tecla D en la PC): muestra/oculta el panel de diagnóstico
        var ts = UnityEngine.InputSystem.Touchscreen.current;
        if (ts != null && ts.primaryTouch.press.wasPressedThisFrame && ts.primaryTouch.tapCount.ReadValue() >= 2)
            mostrarDepuracion = !mostrarDepuracion;
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb != null && kb.dKey.wasPressedThisFrame) mostrarDepuracion = !mostrarDepuracion;
#endif
        if (!formatoActivo || !Rastreado || Time.unscaledTime - ultimaLectura < intervaloLectura) return;
        ultimaLectura = Time.unscaledTime;
        Leer();
    }

    void Leer()
    {
        var cam = VuforiaBehaviour.Instance.CameraDevice;
        var img = cam.GetCameraImage(PixelFormat.GRAYSCALE);
        if (Image.IsNullOrEmpty(img)) { estadoCamara = "sin imagen de cámara"; return; }
        var ci = cam.GetCameraIntrinsics();
        var intr = new Vector4(ci.FocalLength.x, ci.FocalLength.y, ci.PrincipalPoint.x, ci.PrincipalPoint.y);
        var px = img.Pixels;
        if (px == null || px.Length == 0) { estadoCamara = "imagen vacía"; return; }
        int w = img.Width, h = img.Height, stride = img.Stride;
        float sx = w / ci.Size.x, sy = h / ci.Size.y;
        estadoCamara = $"cámara {w}x{h}";
        lecturas++;

        int n = puntosPorLado * puntosPorLado;
        // Puntos en coordenadas de la cámara de Unity
        var pc = new Vector3[botones.Length * n];
        for (int b = 0; b < botones.Length; b++)
        {
            int k = 0;
            foreach (var p in PuntosLocales(botones[b]))
                pc[b * n + k++] = camaraAR.transform.InverseTransformPoint(imageTarget.transform.TransformPoint(p));
        }

        if (orientacion < 0)
        {
            for (int cand = 0; cand < 8; cand++)
            {
                var vivo = Muestrear(pc, cand, px, w, h, stride, intr, sx, sy);
                float s = 0f;
                for (int b = 0; b < botones.Length; b++) s += Correlacion(vivo, referencia, b * n, n);
                sumaCalibracion[cand] += s / botones.Length;
            }
            if (++lecturasCalibracion >= 8)
            {
                int mejor = 0;
                for (int c = 1; c < 8; c++) if (sumaCalibracion[c] > sumaCalibracion[mejor]) mejor = c;
                ultimaCalibracion = sumaCalibracion[mejor] / lecturasCalibracion;
                if (ultimaCalibracion > 0.3f) orientacion = mejor;
                lecturasCalibracion = 0; System.Array.Clear(sumaCalibracion, 0, 8);
                if (mostrarDepuracion) Debug.Log($"[BotonesVirtuales] calibración {ultimaCalibracion:0.00} -> orientación {orientacion}");
            }
            GuardarDepuracion(px, w, h, stride, pc, Mathf.Max(0, orientacion), intr, sx, sy);
            return;
        }

        var actual = Muestrear(pc, orientacion, px, w, h, stride, intr, sx, sy);
        for (int b = 0; b < botones.Length; b++)
            botones[b].similitud = Correlacion(actual, referencia, b * n, n);
        GuardarDepuracion(px, w, h, stride, pc, orientacion, intr, sx, sy);

        for (int b = 0; b < botones.Length; b++)
        {
            var bt = botones[b];
            bool otrosVisibles = false;
            for (int o = 0; o < botones.Length; o++)
                if (o != b && botones[o].similitud > umbralOtrosVisibles) { otrosVisibles = true; break; }

            if (!bt.presionado)
            {
                bt.framesTapado = (bt.similitud < umbralPresionado && otrosVisibles) ? bt.framesTapado + 1 : 0;
                if (bt.framesTapado >= lecturasParaConfirmar)
                {
                    bt.presionado = true; bt.framesVisible = 0;
                    Debug.Log($"[BotonesVirtuales] PRESIONADO {bt.nombreBoton}");
                    bt.OnPresionado.Invoke(bt);
                }
            }
            else
            {
                bt.framesVisible = bt.similitud > umbralLiberado ? bt.framesVisible + 1 : 0;
                if (bt.framesVisible >= 2)
                {
                    bt.presionado = false; bt.framesTapado = 0;
                    bt.OnLiberado.Invoke(bt);
                }
            }
        }
    }

    // Proyección pinhole: punto de cámara -> píxel del sensor, con la orientación candidata
    static bool Proyectar(Vector3 p, int cand, Vector4 intr, float sx, float sy, out float u, out float v)
    {
        u = v = 0f;
        if (p.z <= 0.0001f) return false;
        float x = p.x, y = p.y;
        for (int r = 0; r < (cand & 3); r++) { float t = x; x = -y; y = t; }   // rotaciones de 90°
        if ((cand & 4) != 0) x = -x;                                          // espejo
        u = (intr.x * x / p.z + intr.z) * sx;
        v = (intr.w - intr.y * y / p.z) * sy;
        return true;
    }

    float[] Muestrear(Vector3[] pc, int cand, byte[] px, int w, int h, int stride, Vector4 intr, float sx, float sy)
    {
        var r = new float[pc.Length];
        for (int i = 0; i < pc.Length; i++)
        {
            if (!Proyectar(pc[i], cand, intr, sx, sy, out var u, out var v) || u < 0 || v < 0 || u >= w - 1 || v >= h - 1) { r[i] = float.NaN; continue; }
            int iu = (int)u, iv = (int)v;
            // pequeño promedio 2x2 para suavizar ruido
            r[i] = (px[iv * stride + iu] + px[iv * stride + iu + 1] + px[(iv + 1) * stride + iu] + px[(iv + 1) * stride + iu + 1]) / (4f * 255f);
        }
        return r;
    }

    // Correlación cruzada normalizada (insensible a cambios de brillo y contraste)
    static float Correlacion(float[] a, float[] r, int inicio, int n)
    {
        double sa = 0, sr = 0; int m = 0;
        for (int i = inicio; i < inicio + n; i++) { if (float.IsNaN(a[i])) continue; sa += a[i]; sr += r[i]; m++; }
        if (m < n / 2) return 0f;
        double ma = sa / m, mr = sr / m, num = 0, da = 0, dr = 0;
        for (int i = inicio; i < inicio + n; i++)
        {
            if (float.IsNaN(a[i])) continue;
            double x = a[i] - ma, y = r[i] - mr;
            num += x * y; da += x * x; dr += y * y;
        }
        if (da < 1e-6 || dr < 1e-6) return 0f;   // zona uniforme (p. ej. un dedo) -> sin parecido
        return (float)(num / System.Math.Sqrt(da * dr));
    }

    float ultimoLog;
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    void GuardarDepuracion(byte[] px, int w, int h, int stride, Vector3[] pc, int cand, Vector4 intr, float sx, float sy)
    {
#if UNITY_EDITOR
        if (Time.unscaledTime - ultimoLog < 2f) return;
        ultimoLog = Time.unscaledTime;
        Debug.Log("[BotonesVirtuales] " + Estado().Replace("\n", " | "));
        var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        var cols = new Color32[w * h];
        for (int y = 0; y < h; y++) for (int x = 0; x < w; x++) { byte g = px[y * stride + x]; cols[(h - 1 - y) * w + x] = new Color32(g, g, g, 255); }
        tex.SetPixels32(cols);
        int n = puntosPorLado * puntosPorLado;
        for (int i = 0; i < pc.Length; i++)
            if (Proyectar(pc[i], cand, intr, sx, sy, out var u, out var v) && u >= 0 && v >= 0 && u < w && v < h)
                tex.SetPixel((int)u, h - 1 - (int)v, (i / n) == 0 ? Color.red : (i / n) == 1 ? Color.green : Color.cyan);
        System.IO.File.WriteAllBytes(System.IO.Path.Combine(Application.dataPath, "../Temp/bv_debug.png"), tex.EncodeToPNG());
        Destroy(tex);
#endif
    }

    public string Estado()
    {
        var s = $"Target: {(imageTarget ? imageTarget.TargetStatus.Status.ToString() : "-")}  {estadoCamara}  lecturas: {lecturas}\n";
        s += orientacion < 0 ? $"Calibrando... (última={ultimaCalibracion:0.00})\n" : $"Orientación: {orientacion} (calibración {ultimaCalibracion:0.00})\n";
        if (botones != null)
            foreach (var b in botones) s += $"{b.nombreBoton}: {b.similitud:0.00} {(b.presionado ? "PRESIONADO" : "")}\n";
        return s;
    }

    GUIStyle estilo;
    void OnGUI()
    {
        if (!mostrarDepuracion) return;
        if (estilo == null) estilo = new GUIStyle(GUI.skin.box) { fontSize = Mathf.Max(14, Screen.height / 45), alignment = TextAnchor.UpperLeft };
        GUI.Box(new Rect(10, 10, Screen.width - 20, estilo.fontSize * 8), Estado(), estilo);
    }
}
