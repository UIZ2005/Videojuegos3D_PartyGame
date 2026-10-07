using System.Collections;
using UnityEngine;

public class GroundPound : PowerUp
{
    [Header("Ground Pound")]
    [SerializeField] private float velocidadCaida = 20f;

    [Header("Onda de choque")]
    [SerializeField] private float radioImpacto = 5f;
    [SerializeField] private float fuerzaEmpuje = 10f;
    [SerializeField] private float fuerzaEmpujeVertical = 5f;
    [SerializeField] private float tiempoAturdimiento = 2f;
    [SerializeField] private LayerMask capasImpacto;

    private bool estaSostenido = false;
    private bool yaUsado = false;

    private movePlayer jugador;

    public override bool PuedeSerRecogido()
    {
        return !estaSostenido && !yaUsado;
    }

    public override void SerRecogido(Transform puntoAgarre)
    {
        estaSostenido = true;

        transform.position = puntoAgarre.position;
        transform.SetParent(puntoAgarre);

        jugador = puntoAgarre.GetComponentInParent<movePlayer>();
    }

    public override void Usar(Vector3 direccionUso)
    {
        if (yaUsado || jugador == null)
            return;

        estaSostenido = false;
        yaUsado = true;

        transform.SetParent(null);

        jugador.IniciarGroundPound(
        velocidadCaida,
        this);

    }

    public void EjecutarImpacto(Vector3 posicionImpacto)
    {
        Collider[] objetosAfectados = Physics.OverlapSphere(
            posicionImpacto,
            radioImpacto,
            capasImpacto
        );

        foreach (Collider objeto in objetosAfectados)
        {
            movePlayer jugadorAfectado =
                objeto.GetComponentInParent<movePlayer>();

            if (jugadorAfectado == null)
                continue;

            if (jugadorAfectado == jugador)
                continue;

            AplicarEfectosJugador(
                jugadorAfectado,
                posicionImpacto
            );
        }
    }

    private void AplicarEfectosJugador(movePlayer jugadorAfectado,Vector3 posicionImpacto)
    {
        Rigidbody rbJugador =
            jugadorAfectado.GetComponent<Rigidbody>();

        if (rbJugador != null)
        {
            Vector3 direccionEmpuje =
                (jugadorAfectado.transform.position - posicionImpacto).normalized;

            direccionEmpuje.y = 0f;

            Vector3 fuerza =
                (direccionEmpuje * fuerzaEmpuje) +
                (Vector3.up * fuerzaEmpujeVertical);

            rbJugador.AddForce(
                fuerza,
                ForceMode.Impulse
            );
        }

        // Aturdir
        jugadorAfectado.activarquieto();

        StartCoroutine(
            QuitarAturdimiento(jugadorAfectado)
        );

        // SOLTAR DIAMANTE
        if (jugadorAfectado.condiamante)
        {
            Diamante diamante =
                FindAnyObjectByType<Diamante>();

            if (diamante != null)
            {
                diamante.soltardiamante();
            }
        }
    }

    private IEnumerator QuitarAturdimiento(movePlayer jugadorAfectado)
    {
        yield return new WaitForSeconds(tiempoAturdimiento);

        if (jugadorAfectado != null)
        {
            jugadorAfectado.desactivarquieto();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radioImpacto);
    }
}