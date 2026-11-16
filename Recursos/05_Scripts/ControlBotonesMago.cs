// FantasyAR - Control de animaciones del Mago con 3 botones virtuales.
//   Botón 1 - Spell : activa la animación de hechizo
//   Botón 2 - Idle  : activa la animación de reposo
//   Botón 3 - Jump  : activa la animación de salto
// Cada botón activa ÚNICAMENTE su animación (las demás se cancelan).
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class ControlBotonesMago : MonoBehaviour
{
    public Animator animatorMago;
    public BotonVirtualAR botonSpell;
    public BotonVirtualAR botonIdle;
    public BotonVirtualAR botonJump;

    [Tooltip("Efecto visual que se reproduce al lanzar el hechizo (opcional)")]
    public ParticleSystem efectoHechizo;

    [Tooltip("Rayo que sale de la mano del Mago cada vez que termina la animación de hechizo")]
    public ParticleSystem efectoRayo;

    int cicloSpell = -1;

    void LateUpdate()
    {
        // Detecta el final de cada ciclo de la animación Spell y lanza el rayo
        var estado = animatorMago.GetCurrentAnimatorStateInfo(0);
        if (estado.IsName("Spell") && !animatorMago.IsInTransition(0))
        {
            int ciclo = Mathf.FloorToInt(estado.normalizedTime);
            if (cicloSpell >= 0 && ciclo > cicloSpell && efectoRayo != null) efectoRayo.Play(true);
            cicloSpell = ciclo;
        }
        else cicloSpell = -1;
    }

    static readonly string[] Triggers = { "Spell", "Idle", "Jump" };

    void OnEnable()
    {
        botonSpell.OnPresionado.AddListener(_ => Activar("Spell"));
        botonIdle.OnPresionado.AddListener(_ => Activar("Idle"));
        botonJump.OnPresionado.AddListener(_ => Activar("Jump"));
    }

    void OnDisable()
    {
        botonSpell.OnPresionado.RemoveAllListeners();
        botonIdle.OnPresionado.RemoveAllListeners();
        botonJump.OnPresionado.RemoveAllListeners();
    }

    public void Activar(string animacion)
    {
        foreach (var t in Triggers) animatorMago.ResetTrigger(t);
        animatorMago.SetTrigger(animacion);
        if (animacion == "Spell" && efectoHechizo != null) efectoHechizo.Play(true);
        Debug.Log($"[FantasyAR] Botón virtual {animacion} -> animación {animacion} del Mago");
    }

#if ENABLE_INPUT_SYSTEM
    // Atajos para probar en el Editor sin cámara: teclas 1, 2 y 3
    void Update()
    {
        var k = Keyboard.current;
        if (k == null) return;
        if (k.digit1Key.wasPressedThisFrame) Activar("Spell");
        if (k.digit2Key.wasPressedThisFrame) Activar("Idle");
        if (k.digit3Key.wasPressedThisFrame) Activar("Jump");
    }
#endif
}
