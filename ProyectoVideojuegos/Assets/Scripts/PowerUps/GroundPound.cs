using UnityEngine;

public class GroundPound : PowerUp
{
    [Header("Ground Pound")]
    [SerializeField] private float velocidadCaida = 20f;

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

        jugador.IniciarGroundPound(velocidadCaida);

        gameObject.SetActive(false);
    }
}