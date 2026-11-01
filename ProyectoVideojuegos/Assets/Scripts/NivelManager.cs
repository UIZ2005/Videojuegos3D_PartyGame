using UnityEngine;
using UnityEngine.InputSystem;

public class NivelManager : MonoBehaviour
{
    public Transform[] SpawnPoints;

    void Start()
    {
        int cantidadJugadores = PlayerSelectionManager.SelectedPlayerPrefabs.Count;

        for (int i = 0; i < cantidadJugadores; i++)
        {
            GameObject prefabJugador = PlayerSelectionManager.SelectedPlayerPrefabs[i];
            InputDevice dispositivo = PlayerSelectionManager.SelectedPlayerDevices[i];
            
            PlayerInput inputJugador = PlayerInput.Instantiate(prefabJugador, pairWithDevice: dispositivo);
            GameObject nuevoJugador = inputJugador.gameObject;
            
            nuevoJugador.transform.position = SpawnPoints[i].position;
            nuevoJugador.transform.rotation = SpawnPoints[i].rotation;

            Camera camara = nuevoJugador.GetComponentInChildren<Camera>();
            if (camara != null)
            {
                ConfigurarPantallaDividida(camara, i, cantidadJugadores);
            }
        }
    }

    private void ConfigurarPantallaDividida(Camera cam, int indiceJugador, int totalJugadores)
    {
        if (totalJugadores == 1)
        {
            cam.rect = new Rect(0f, 0f, 1f, 1f);
        }
        else if (totalJugadores == 2)
        {
            if (indiceJugador == 0) cam.rect = new Rect(0f, 0f, 0.5f, 1f);
            else cam.rect = new Rect(0.5f, 0f, 0.5f, 1f);
        }
        else
        {
            if (indiceJugador == 0) cam.rect = new Rect(0f, 0.5f, 0.5f, 0.5f);
            else if (indiceJugador == 1) cam.rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
            else if (indiceJugador == 2) cam.rect = new Rect(0f, 0f, 0.5f, 0.5f);
            else if (indiceJugador == 3) cam.rect = new Rect(0.5f, 0f, 0.5f, 0.5f);
        }
    }
}