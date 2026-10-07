using UnityEngine;

public class PowerUpSpawner : MonoBehaviour
{
    [SerializeField] private PowerUpFactory factory;
    [SerializeField] private float radioDeGeneracion = 5f;
    [SerializeField] private float tiempoEntreSpawns = 3f;

    private float temporizador;

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
        // Elige un número al azar entre 0, 1 y 2 (Bomba, Gancho, Impulso)
        int indiceAleatorio = Random.Range(0, 5);
        PowerUpType tipoAleatorio = (PowerUpType)indiceAleatorio;

        // Calcula una posición al azar dentro de un círculo plano (ejes X y Z para 3D)
        Vector2 circulo = Random.insideUnitCircle * radioDeGeneracion;
        Vector3 posicionSpawn = transform.position + new Vector3(circulo.x, 0f, circulo.y);

        // Le pide a la fábrica que lo cree
        factory.CrearPowerUp(tipoAleatorio, posicionSpawn);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radioDeGeneracion);
    }
}