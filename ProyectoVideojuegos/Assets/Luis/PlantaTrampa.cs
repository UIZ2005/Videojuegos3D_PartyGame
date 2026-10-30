
using System.Collections;
using UnityEngine;

public class PlantaTrampa : MonoBehaviour
{
    [Header("Detección")]
    public float tiempoCaptura = 3f;
    public float cooldown = 2f;
    private AudioSource audioplanta;

    [Header("Punto de agarre")]
    public Transform puntoAgarre;

    [Header("Animación")]
    public Animator animator;
    public string parametroAtrapando = "catch";

    private bool atrapando = false;
    private bool disponible = true;

    private movePlayer jugadorAtrapado;
    private Coroutine rutinaCaptura;

    private Transform padreOriginal;
    private Vector3 escalaOriginal;

    public void Start()
    {
        audioplanta = GetComponent<AudioSource>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!disponible || atrapando)
            return;

        movePlayer jugador = other.GetComponentInParent<movePlayer>();

        if (jugador == null)
            return;

        CapturarJugador(jugador);
    }

    private void CapturarJugador(movePlayer jugador)
    {
        audioplanta.Play();
        atrapando = true;
        disponible = false;

        jugadorAtrapado = jugador;

        // Guardar jerarquía y escala originales.
        Transform jugadorTransform = jugador.transform;

        padreOriginal = jugadorTransform.parent;
        escalaOriginal = jugadorTransform.lossyScale;

        // Inmovilizar al jugador.
        jugador.activarquieto();

        // Convertir al jugador en hijo del punto de agarre.
        jugadorTransform.SetParent(puntoAgarre, true);

        // Colocarlo en la posición del punto de agarre.
        jugadorTransform.position = puntoAgarre.position;
        jugadorTransform.rotation = puntoAgarre.rotation;

        // Recuperar su escala mundial original.
        jugadorTransform.localScale = Vector3.one;

        // Ajustar escala local para conservar la escala mundial.
        Vector3 escalaPadre = jugadorTransform.parent.lossyScale;

        jugadorTransform.localScale = new Vector3(
            escalaOriginal.x / escalaPadre.x,
            escalaOriginal.y / escalaPadre.y,
            escalaOriginal.z / escalaPadre.z
        );

        // Activar animación.
        if (animator != null)
        {
            animator.SetBool(parametroAtrapando, true);
        }

        rutinaCaptura = StartCoroutine(LiberarJugador());
    }

    private IEnumerator LiberarJugador()
    {
        yield return new WaitForSeconds(tiempoCaptura);

        if (jugadorAtrapado != null)
        {
            Transform jugadorTransform = jugadorAtrapado.transform;

            // Recuperar su padre original.
            jugadorTransform.SetParent(padreOriginal, true);

            // Restaurar su escala original.
            jugadorTransform.localScale = escalaOriginal;

            // Liberar al jugador.
            jugadorAtrapado.desactivarquieto();
        }

        jugadorAtrapado = null;
        padreOriginal = null;

        // Abrir la planta.
        if (animator != null)
        {
            animator.SetBool(parametroAtrapando, false);
        }

        atrapando = false;

        audioplanta.Stop();
        yield return new WaitForSeconds(cooldown);

        disponible = true;
        rutinaCaptura = null;
    }

    private void OnDisable()
    {
        if (jugadorAtrapado != null)
        {
            Transform jugadorTransform = jugadorAtrapado.transform;

            jugadorTransform.SetParent(padreOriginal, true);
            jugadorTransform.localScale = escalaOriginal;

            jugadorAtrapado.desactivarquieto();
        }

        if (rutinaCaptura != null)
        {
            StopCoroutine(rutinaCaptura);
            rutinaCaptura = null;
        }

        jugadorAtrapado = null;
        padreOriginal = null;

        atrapando = false;
        disponible = true;
    }
}
