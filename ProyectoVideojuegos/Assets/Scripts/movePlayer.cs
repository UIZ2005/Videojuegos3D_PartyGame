using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class movePlayer : MonoBehaviour
{
    private Vector2 moveInput;
    private Vector2 lookInput;

    public float speed = 5f;
    public float jumpForce = 5f;
    public float rotationSpeed = 15f;
    public Camera camara;

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

        // Evita que las colisiones inclinen al jugador.
        rb.constraints |= RigidbodyConstraints.FreezeRotationX
                        | RigidbodyConstraints.FreezeRotationZ;
    }

    private void OnMove(InputValue context)
    {
        moveInput = context.Get<Vector2>();
    }

    // La acción Look debe estar vinculada al mouse.
    private void OnLook(InputValue context)
    {
        lookInput = context.Get<Vector2>();
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
            Vector3 direccionUso =
                (transform.forward * fuerzaUsoHaciaAdelante) +
                (Vector3.up * fuerzaUsoHaciaArriba);

            poderSostenido.Usar(direccionUso);
            poderSostenido = null;
        }
        else
        {
            Collider[] objetosCercanos = Physics.OverlapSphere(
                transform.position,
                radioInteraccion,
                capaPoderes
            );

            foreach (var objeto in objetosCercanos)
            {
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

    private void Update()
    {
        estaPiso = Physics.CheckSphere(
            puntoPiso.position,
            groundCheckRadius,
            piso
        );
    }


    private void FixedUpdate()
    {
        if (camara == null) return;

        // Dirección horizontal de la cámara.
        Vector3 direccionAdelante = camara.transform.forward;
        Vector3 direccionDerecha = camara.transform.right;

        direccionAdelante.y = 0f;
        direccionDerecha.y = 0f;

        direccionAdelante.Normalize();
        direccionDerecha.Normalize();

        // Movimiento relativo a la cámara.
        Vector3 movimiento =
            direccionDerecha * moveInput.x +
            direccionAdelante * moveInput.y;

        movimiento = Vector3.ClampMagnitude(movimiento, 1f);

        // Mantener la velocidad horizontal del jugador.
        Vector3 velocidad = rb.linearVelocity;
        velocidad.x = movimiento.x * speed;
        velocidad.z = movimiento.z * speed;

        rb.linearVelocity = velocidad;

        // Rotar al jugador hacia donde se mueve.
        if (movimiento.sqrMagnitude > 0.01f)
        {
            Quaternion rotacionObjetivo =
                Quaternion.LookRotation(movimiento, Vector3.up);

            Quaternion nuevaRotacion = Quaternion.Slerp(
                rb.rotation,
                rotacionObjetivo,
                rotationSpeed * Time.fixedDeltaTime
            );

            rb.MoveRotation(nuevaRotacion);
        }
    }
}
