using UnityEngine;
using UnityEngine.InputSystem;

public class prueba : MonoBehaviour
{
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
        {
            var rueda = GetComponentInChildren<RuedaColoresUI>(true);
            rueda.Abrir(c => Debug.Log("Elegido: " + c), () => Debug.Log("Cancelado"));
        }
    }
}
