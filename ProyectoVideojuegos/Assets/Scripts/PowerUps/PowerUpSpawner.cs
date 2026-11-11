using System.Collections.Generic;
using UnityEngine;

public class PowerUpSpawner : MonoBehaviour
{
    [Header("Configuración del Spawner")]
    [SerializeField] private PowerUpFactory factory;
    [SerializeField] private float radioDeGeneracion = 5f;
    [SerializeField] private float tiempoEntreSpawns = 15f;

    [Header("Poderes Permitidos en esta zona")]
    [SerializeField] private List<PowerUpType> poderesPermitidos;

    // Diccionario para llevar la cuenta de cuántos de cada tipo hay en el suelo
    private Dictionary<PowerUpType, List<PowerUp>> poderesActivos = new Dictionary<PowerUpType, List<PowerUp>>();
    private float temporizador;

    void Awake()
    {
        // Inicializamos el diccionario con todos los tipos posibles para evitar errores
        foreach (PowerUpType tipo in System.Enum.GetValues(typeof(PowerUpType)))
        {
            poderesActivos[tipo] = new List<PowerUp>();
        }
    }

    void Update()
    {
        temporizador += Time.deltaTime;

        if (temporizador >= tiempoEntreSpawns)
        {
            GenerarPoderAleatorio();
            temporizador = 0f; // Reinicia el cronómetro
        }
    }

    private void GenerarPoderAleatorio()
    {
        if (poderesPermitidos.Count == 0) return;

        // Limpia la memoria de poderes que ya fueron destruidos o recogidos por un jugador
        LimpiarListas();

        // Filtramos solo los poderes permitidos que aún no llegan al límite de 2 en el suelo
        List<PowerUpType> opcionesValidas = new List<PowerUpType>();
        foreach (PowerUpType tipo in poderesPermitidos)
        {
            if (poderesActivos[tipo].Count < 2)
            {
                opcionesValidas.Add(tipo);
            }
        }

        // Si todos los tipos llegaron a 2, no spawneamos nada hasta que alguien agarre uno
        if (opcionesValidas.Count == 0) return;

        // Elige uno al azar de las opciones que todavía tienen espacio
        PowerUpType tipoElegido = opcionesValidas[Random.Range(0, opcionesValidas.Count)];

        // Calcula posición
        Vector2 circulo = Random.insideUnitCircle * radioDeGeneracion;
        Vector3 posicionSpawn = transform.position + new Vector3(circulo.x, 0f, circulo.y);

        // Instancia a través de la fábrica
        PowerUp nuevoPoder = factory.CrearPowerUp(tipoElegido, posicionSpawn);

        if (nuevoPoder != null)
        {
            poderesActivos[tipoElegido].Add(nuevoPoder);
        }
    }

    private void LimpiarListas()
    {
        // Si el objeto es null (explotó) o si !PuedeSerRecogido() (ya lo agarraron), libera el espacio
        foreach (var lista in poderesActivos.Values)
        {
            lista.RemoveAll(poder => poder == null || !poder.PuedeSerRecogido());
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radioDeGeneracion);
    }
}