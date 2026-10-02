using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
public class movePlayer : MonoBehaviour
{
    private Vector2 moveInput;
    public float speed = 5f;
    public float jumpForce = 5f;
    public float rotationSpeed = 15f; 
    private Rigidbody rb;

    public Transform puntoPiso;      
    public float groundCheckRadius = 0.2f;
    public LayerMask piso;      
    public bool estaPiso;

    // Sistema de Poderes
    public Transform puntoAgarrePoder; 
    public float radioInteraccion = 2f; 
    public LayerMask capaPoderes;
    public float fuerzaUsoHaciaAdelante = 10f;
    public float fuerzaUsoHaciaArriba = 5f; 
    
    private PowerUp poderSostenido; 

    public void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnMove(InputValue context)
    {
        moveInput = context.Get<Vector2>();
    }

    private void OnJump(InputValue context)
    {
        if (estaPiso)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void OnInteract(InputValue context)
    {
        if (!context.isPressed) return;

        if (poderSostenido != null)
        {
            // Calcula el vector de dirección/fuerza
            Vector3 direccionUso = (transform.forward * fuerzaUsoHaciaAdelante) + (Vector3.up * fuerzaUsoHaciaArriba);
            
            // Llama al método abstracto
            poderSostenido.Usar(direccionUso); 
            poderSostenido = null; 
        }
        else
        {
            Collider[] objetosCercanos = Physics.OverlapSphere(transform.position, radioInteraccion, capaPoderes);
            
            foreach (var objeto in objetosCercanos)
            {
                // Busca la clase base PowerUp, detectará cualquier script que herede de ella
                if (objeto.TryGetComponent(out PowerUp poderEncontrado))
                {
                    if (poderEncontrado.PuedeSerRecogido())
                    {
                        poderSostenido = poderEncontrado;
                        poderSostenido.SerRecogido(puntoAgarrePoder);
                        break; 
                    }
                }
            }
        }
    }

    void Update()
    {
        estaPiso = Physics.CheckSphere(puntoPiso.position, groundCheckRadius, piso);
        
        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y);
        transform.position += movement * speed * Time.deltaTime;

        if (movement != Vector3.zero)
        {
            Quaternion rotacionObjetivo = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionObjetivo, rotationSpeed * Time.deltaTime);
        }
    }
}