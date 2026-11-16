// FantasyAR - Mejora la imagen de la cámara: modo de máxima calidad y enfoque automático continuo.
using UnityEngine;
using Vuforia;

public class ConfigurarCamaraAR : MonoBehaviour
{
    void Start()
    {
        VuforiaApplication.Instance.OnVuforiaStarted += Configurar;
        if (VuforiaApplication.Instance.IsRunning) Configurar();
    }

    void OnDestroy()
    {
        if (VuforiaApplication.Instance != null) VuforiaApplication.Instance.OnVuforiaStarted -= Configurar;
    }

    void Configurar()
    {
        var cam = VuforiaBehaviour.Instance.CameraDevice;
        bool calidad = cam.SetCameraMode(CameraMode.MODE_OPTIMIZE_QUALITY);
        bool foco = cam.SetFocusMode(FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
        Debug.Log($"[FantasyAR] Cámara: modo calidad={calidad}, enfoque continuo={foco}");
    }
}
