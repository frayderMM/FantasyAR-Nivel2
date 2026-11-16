// FantasyAR - Botón virtual sobre un Image Target de Vuforia.
// Define una zona rectangular del dibujo (en coordenadas locales del Image Target, en metros).
// Cuando el usuario tapa esa zona con el dedo, DetectorBotonesVirtuales dispara OnPresionado.
using UnityEngine;
using UnityEngine.Events;

public class BotonVirtualAR : MonoBehaviour
{
    [Tooltip("Nombre del botón (Spell, Idle o Jump)")]
    public string nombreBoton = "Spell";

    [Tooltip("Tamaño de la zona sensible del botón sobre el dibujo (ancho, alto) en metros")]
    public Vector2 tamano = new Vector2(0.04f, 0.04f);

    public UnityEvent<BotonVirtualAR> OnPresionado = new UnityEvent<BotonVirtualAR>();
    public UnityEvent<BotonVirtualAR> OnLiberado = new UnityEvent<BotonVirtualAR>();

    [Header("Estado (solo lectura)")]
    public bool presionado;
    [Range(-1f, 1f)] public float similitud = 1f;   // 1 = zona visible igual al dibujo, bajo = zona tapada

    [HideInInspector] public int framesTapado;
    [HideInInspector] public int framesVisible;

    // Centro del botón en el plano del Image Target (X = derecha, Z = arriba del dibujo)
    public Vector2 CentroLocal => new Vector2(transform.localPosition.x, transform.localPosition.z);

    void OnDrawGizmos()
    {
        Gizmos.color = presionado ? Color.green : Color.yellow;
        Gizmos.matrix = transform.parent ? transform.parent.localToWorldMatrix : Matrix4x4.identity;
        Gizmos.DrawWireCube(transform.localPosition, new Vector3(tamano.x, 0.001f, tamano.y));
    }
}
