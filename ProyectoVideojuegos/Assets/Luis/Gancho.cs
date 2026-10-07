using System.Collections;
using UnityEngine;

public class Gancho : PowerUp
{
    [Header("Gancho")]
    [SerializeField] private float alcance = 20f;
    [SerializeField] private float velocidadAtraccion = 15f;
    [SerializeField] private LayerMask capasEnganchables;

    [Header("Visual")]
    [SerializeField] private LineRenderer liana;
    [SerializeField] float escalaPequena = 0.5f;

    private Vector3 escalaOriginal;

    private Rigidbody rb;
    private Collider col;

    private Transform jugador;
    private movePlayer movimientoJugador;

    private bool estaSostenido = false;
    private bool yaUsado = false;

    private Diamante diamante;

    private void Awake()
    {
        escalaOriginal = transform.localScale;
        diamante =FindAnyObjectByType<Diamante>();
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        if (liana != null)
        {
            liana.positionCount = 2;
            liana.enabled = false;
        }
    }

    // =========================================================
    // ¿SE PUEDE RECOGER?
    // =========================================================

    public override bool PuedeSerRecogido()
    {
        return !estaSostenido && !yaUsado;
    }

    // =========================================================
    // RECOGER
    // =========================================================

    public override void SerRecogido(Transform puntoAgarre)
    {
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
        transform.localScale = escalaOriginal * escalaPequena;
    }

    // =========================================================
    // USAR
    // =========================================================

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

        // El gancho trabaja horizontalmente.
        direccionUso = movimientoJugador.camara.transform.forward;

        if (direccionUso.sqrMagnitude < 0.01f)
        {
            DestruirGancho();
            return;
        }

        direccionUso.Normalize();

        // Buscar una pared/objeto enganchable.
        RaycastHit hit;

        if (Physics.Raycast(
            jugador.position,
            direccionUso,
            out hit,
            alcance,
            capasEnganchables
        ))
        {
            if (diamante != null) { 
            if (hit.collider.CompareTag("Player") && hit.collider.gameObject.GetComponent<movePlayer>().condiamante)
                diamante.soltardiamante();
            }

            StartCoroutine(
                AtraerJugador(hit.point)
            );
        }
        else
        {
            Debug.Log("El gancho no encontró ningún objetivo.");

            DestruirGancho();
        }
    }

    // =========================================================
    // ATRAER JUGADOR
    // =========================================================

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

        // Mostrar liana.
        if (liana != null)
        {
            liana.enabled = true;

            liana.SetPosition(
                0,
                jugador.position
            );

            liana.SetPosition(
                1,
                puntoObjetivo
            );
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
                (puntoObjetivo -
                 jugador.position).normalized;

            // Mover directamente al jugador.
            jugador.position +=
                direccion *
                velocidadAtraccion *
                Time.deltaTime;

            // Mantener la liana conectada.
            if (liana != null)
            {
                liana.SetPosition(
                    0,
                    jugador.position
                );

                liana.SetPosition(
                    1,
                    puntoObjetivo
                );
            }

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
    // DESTRUIR
    // =========================================================

    private void DestruirGancho()
    {
        if (liana != null)
        {
            liana.enabled = false;
        }

        Destroy(gameObject);
    }

    // =========================================================
    // DEBUG
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawRay(
            transform.position,
            transform.forward * alcance
        );
    }
}