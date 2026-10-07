using UnityEngine;

public class PowerUpFactory : MonoBehaviour
{
    [SerializeField] private PowerUp bombaPrefab;
    [SerializeField] private PowerUp ganchoPrefab; 
    [SerializeField] private PowerUp impulsoPrefab;
    [SerializeField] private PowerUp coloresPrefab;
    [SerializeField] private PowerUp invisibilidadPrefab;

    public PowerUp CrearPowerUp(PowerUpType tipo, Vector3 posicion)
    {
        PowerUp prefabSeleccionado = null;

        switch (tipo)
        {
            case PowerUpType.Bomba:
                prefabSeleccionado = bombaPrefab;
                break;
            case PowerUpType.Gancho:
                prefabSeleccionado = ganchoPrefab;
                break;
            case PowerUpType.colores:
                prefabSeleccionado = coloresPrefab;
                break;
            case PowerUpType.Impulso:
                prefabSeleccionado = impulsoPrefab;
                break;
            case PowerUpType.invisibilidad:
                prefabSeleccionado = invisibilidadPrefab;
                break;


        }

        if (prefabSeleccionado == null)
        {
            Debug.Log($"El poder [{tipo}] no ha sido creado/asignado todavía. Omitiendo generación.");
            return null;
        }

        return Instantiate(prefabSeleccionado, posicion, Quaternion.identity);
    }
}