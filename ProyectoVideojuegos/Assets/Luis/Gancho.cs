using System.Collections;
using UnityEngine;

public class Gancho : PowerUp
{
    [Header("Gancho")]
    [SerializeField] private float alcance = 20f;
    [SerializeField] private float velocidadAtraccion = 15f;
    [SerializeField] private LayerMask capasEnganchables;

    [Header("Visual")]
    [SerializeField] private Transform modeloLiana;
    [SerializeField] private float longitudModeloLiana = 1f;
    [SerializeField] float escalaPequena = 0.5f;

    private audiomanager Audiomanager;

    private Vector3 escalaOriginal;
    private Vector3 escalaOriginalLiana;

    private Rigidbody rb;
    private Collider col;

    private Transform jugador;
    private movePlayer movimientoJugador;

    private bool estaSostenido = false;
    private bool yaUsado = false;

    private Diamante diamante;

    private void Awake()
    {
        Audiomanager = FindAnyObjectByType<audiomanager>();

        escalaOriginal = transform.localScale;

        diamante = FindAnyObjectByType<Diamante>();

        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        if (modeloLiana != null)
        {
            escalaOriginalLiana = modeloLiana.localScale;
            modeloLiana.gameObject.SetActive(false);
        }
    }

    public override bool PuedeSerRecogido()
    {
        return !estaSostenido && !yaUsado;
    }

    public override void SerRecogido(Transform puntoAgarre)
    {
        if (Audiomanager != null)
            Audiomanager.seleccionAudio(0);

        estaSostenido = true;

        jugador = puntoAgarre.root;

        movimientoJugador =
            jugador.GetComponent<movePlayer>();

        rb.isKinematic = true;
        col.isTrigger = true;

        transform.SetParent(puntoAgarre);

        transform.position =
            puntoAgarre.position;

        transform.localRotation =
            Quaternion.identity;

        transform.localScale =
            escalaOriginal * escalaPequena;
    }


    public override void Usar(Vector3 direccionUso)
    {
        if (yaUsado)
            return;

        yaUsado = true;
        estaSostenido = false;

        // Separar el objeto del jugador.
        transform.SetParent(null);

        rb.isKinematic = true;
        col.isTrigger = true;

        direccionUso =
            movimientoJugador.camara.transform.forward;

        if (direccionUso.sqrMagnitude < 0.01f)
        {
            DestruirGancho();
            return;
        }

        direccionUso.Normalize();

        RaycastHit hit;

        if (Physics.Raycast(
            jugador.position,
            direccionUso,
            out hit,
            alcance,
            capasEnganchables
        ))
        {
            if (diamante != null)
            {
                if (hit.collider.CompareTag("Player") &&
                    hit.collider.gameObject
                        .GetComponent<movePlayer>()
                        .condiamante)
                {
                    diamante.soltardiamante();
                }
            }

            StartCoroutine(
                AtraerJugador(hit.point)
            );
        }
        else
        {
            Debug.Log(
                "El gancho no encontró ningún objetivo."
            );

            DestruirGancho();
        }
    }

    private IEnumerator AtraerJugador(
        Vector3 puntoObjetivo
    )
    {
        if (jugador == null)
        {
            DestruirGancho();
            yield break;
        }

        if (movimientoJugador != null)
        {
            movimientoJugador.activarquieto();
        }

        // Activar modelo de la liana.
        if (modeloLiana != null)
        {
            modeloLiana.gameObject.SetActive(true);
        }

        while (true)
        {
            float distancia =
                Vector3.Distance(
                    jugador.position,
                    puntoObjetivo
                );

            if (distancia <= 1.5f)
                break;

            Vector3 direccion =
                (
                    puntoObjetivo -
                    jugador.position
                ).normalized;

            // Mover directamente al jugador.
            jugador.position +=
                direccion *
                velocidadAtraccion *
                Time.deltaTime;

            ActualizarLiana(
                jugador.position,
                puntoObjetivo
            );

            yield return null;
        }

        // Detener cualquier velocidad residual.
        Rigidbody jugadorRb =
            jugador.GetComponent<Rigidbody>();

        if (jugadorRb != null)
        {
            jugadorRb.linearVelocity =
                Vector3.zero;
        }

        // Devolver control al jugador.
        if (movimientoJugador != null)
        {
            movimientoJugador.desactivarquieto();
        }

        DestruirGancho();
    }

    // =========================================================
    // ACTUALIZAR LIANA
    // =========================================================

    private void ActualizarLiana(
        Vector3 puntoInicio,
        Vector3 puntoFinal
    )
    {
        if (modeloLiana == null)
            return;

        Vector3 direccion =
            puntoFinal - puntoInicio;

        float distancia =
            direccion.magnitude;

        if (distancia <= 0.01f)
            return;

        // La base de la liana sigue al jugador.
        modeloLiana.position =
            puntoInicio;

        // El eje Y de la liana apunta hacia el objetivo.
        modeloLiana.rotation =
            Quaternion.FromToRotation(
                Vector3.up,
                direccion.normalized
            );

        // Escalar únicamente en Y.
        Vector3 escala =
            escalaOriginalLiana;

        escala.y =
            escalaOriginalLiana.y *
            (distancia / longitudModeloLiana);

        modeloLiana.localScale =
            escala;
    }


    private void DestruirGancho()
    {
        if (modeloLiana != null)
        {
            modeloLiana.gameObject.SetActive(false);
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawRay(
            transform.position,
            transform.forward * alcance
        );
    }
}