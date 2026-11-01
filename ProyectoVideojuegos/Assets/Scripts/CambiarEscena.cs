using UnityEngine;
using UnityEngine.SceneManagement;

public class CambiarEscena : MonoBehaviour
{
    public string nombreDeLaEscena;

    public void CargarNivel()
    {
        SceneManager.LoadScene(nombreDeLaEscena); 
    }
}