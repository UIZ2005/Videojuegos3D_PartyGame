using UnityEngine;

public class BubbleAttack : MonoBehaviour
{
    [Header("Movimiento")]
    public float speed = 12f;
    public float tiempoVida = 5f;

    [Header("Stun")]
    public float duracionStun = 2f;
    private Animator burbujaanim;

    [Header("Jugador dentro de la burbuja")]
    public float velocidadRotacion = 180f;

    private int jugadorOrigen;

    private movePlayer jugadorAtrapado;
    private Transform jugadorTransform;

    private bool tieneJugador = false;

    private Vector3 escalaOriginalJugador;

    public void Inicializar(int jugador)
    {
        jugadorOrigen = jugador;
    }

    private void Start()
    {
        burbujaanim = GetComponent<Animator>();
        Destroy(gameObject, tiempoVida);
    }

    private void Update()
    {
        // Si todavía no atrapamos a nadie,
        // la burbuja continúa avanzando.
        if (!tieneJugador)
        {
            transform.position += transform.forward * speed * Time.deltaTime;
        }
        else
        {
            // La burbuja gira mientras el jugador está atrapado.
            transform.Rotate(
                Vector3.up,
                velocidadRotacion * Time.deltaTime,
                Space.Self
            );
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (tieneJugador)
            return;

        movePlayer jugador = other.GetComponentInParent<movePlayer>();

        if (jugador == null)
            return;

        // Evitar que la burbuja afecte al jugador que la lanzó.
        if (jugador.numplayer == jugadorOrigen)
            return;

        AtraparJugador(jugador);
    }

    private void AtraparJugador(movePlayer jugador)
    {
        // Aplicar stun.
        StunSystem stun = jugador.GetComponent<StunSystem>();

        if (stun.estaStuneado) return;

        burbujaanim.SetBool("touch", true);
        tieneJugador = true;
        jugadorAtrapado = jugador;
        jugadorTransform = jugador.transform;

        // Guardamos la escala original del jugador.
        escalaOriginalJugador = jugadorTransform.lossyScale;

       

        if (stun != null)
        {
            stun.AplicarStun(duracionStun);
        }

        // Colocar al jugador en el centro de la burbuja.
        jugadorTransform.position = transform.position;

        // Hacer al jugador hijo de la burbuja.
        jugadorTransform.SetParent(transform, true);

        // Corregir la escala para que el jugador
        // NO crezca cuando la burbuja crezca.
        MantenerEscalaJugador();

        // Después de la duración del stun,
        // liberar al jugador.
        Invoke(nameof(LiberarJugador), duracionStun);
    }

    private void MantenerEscalaJugador()
    {
        Vector3 escalaBurbuja = transform.lossyScale;

        if (escalaBurbuja.x == 0 ||
            escalaBurbuja.y == 0 ||
            escalaBurbuja.z == 0)
        {
            return;
        }

        jugadorTransform.localScale = new Vector3(
            escalaOriginalJugador.x / escalaBurbuja.x,
            escalaOriginalJugador.y / escalaBurbuja.y,
            escalaOriginalJugador.z / escalaBurbuja.z
        );
    }

    private void LateUpdate()
    {
        if (!tieneJugador || jugadorTransform == null)
            return;

        // Como el Animator modifica la escala de la burbuja,
        // actualizamos la escala local del jugador después
        // de que el Animator haya aplicado sus cambios.
        MantenerEscalaJugador();
    }

    private void LiberarJugador()
    {
        if (jugadorTransform == null)
        {
            Destroy(gameObject);
            return;
        }

        // Guardamos la posición actual antes de separarlo.
        Vector3 posicion = jugadorTransform.position;

        // Sacar al jugador de la burbuja.
        jugadorTransform.SetParent(null, true);

        // Restaurar su escala original.
        jugadorTransform.localScale = escalaOriginalJugador;

        // Mantener su posición.
        jugadorTransform.position = posicion;

        jugadorAtrapado = null;
        jugadorTransform = null;
        tieneJugador = false;

        Destroy(gameObject);
    }
}