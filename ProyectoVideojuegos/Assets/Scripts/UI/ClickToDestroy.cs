using UnityEngine;
using UnityEngine.InputSystem;

public class ClickToDestroy : MonoBehaviour
{
    Camera cam;

    void Start() => cam = Camera.main;

    void Update()
    {
        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            Ray ray = cam.ScreenPointToRay(Pointer.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                var walker = hit.collider.GetComponentInParent<AnimInicioPlayersd>();
                if (walker != null)
                    Destroy(walker.gameObject);
            }
        }
    }
}