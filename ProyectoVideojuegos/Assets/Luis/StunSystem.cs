using UnityEngine;

public class StunSystem : MonoBehaviour
{
    [Header("Stun")]
    public bool estaStuneado = false;

    private float tiempoStun = 0f;

    private movePlayer jugador;

    private void Awake()
    {
        jugador = GetComponent<movePlayer>();
    }

    public void AplicarStun(float duracion)
    {
        estaStuneado = true;
        tiempoStun = duracion;

        jugador.activarquieto();
    }

    private void Update()
    {
        if (!estaStuneado)
            return;

        tiempoStun -= Time.deltaTime;

        if (tiempoStun <= 0f)
        {
            TerminarStun();
        }
    }

    private void TerminarStun()
    {
        estaStuneado = false;
        tiempoStun = 0f;

        jugador.desactivarquieto();
    }
}