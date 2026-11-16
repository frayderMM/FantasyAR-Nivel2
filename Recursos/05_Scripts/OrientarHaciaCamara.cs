// FantasyAR - Mantiene al personaje de pie y de frente a la cámara.
// Si el dibujo se mira de frente (pantalla, pared), el personaje se "levanta" sobre el dibujo
// y se ve en perspectiva. Si el dibujo está sobre la mesa y se mira inclinado, se para sobre él.
using UnityEngine;

public class OrientarHaciaCamara : MonoBehaviour
{
    [Tooltip("Image Target sobre el que está el personaje")]
    public Transform imageTarget;
    public Camera camara;

    [Tooltip("Inclinación máxima en grados (90 = de pie dentro del plano del dibujo)")]
    [Range(0f, 90f)] public float inclinacionMaxima = 90f;
    [Tooltip("Velocidad con la que se ajusta la inclinación")]
    public float suavizado = 6f;

    float angulo = -1f;

    void LateUpdate()
    {
        if (camara == null) camara = Camera.main;
        if (camara == null || imageTarget == null) return;

        // Dirección "arriba" de la cámara expresada en el sistema del dibujo (Y = normal, Z = parte superior del dibujo)
        var arriba = imageTarget.InverseTransformDirection(camara.transform.up);
        float objetivo = Mathf.Clamp(Mathf.Atan2(arriba.z, arriba.y) * Mathf.Rad2Deg, 0f, inclinacionMaxima);

        angulo = angulo < 0f ? objetivo : Mathf.Lerp(angulo, objetivo, 1f - Mathf.Exp(-suavizado * Time.deltaTime));
        transform.localRotation = Quaternion.Euler(angulo, 0f, 0f);
    }
}
