using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class movePlayer : MonoBehaviour
{
    private Vector2 moveInput;
    private Vector2 lookInput;

    [Header("Movimiento")]
    public float speed = 5f;
    public float jumpForce = 5f;

    [Header("Mira")]
    public float rotationSpeed = 15f;
    public Camera camara;

    private Rigidbody rb;


    [Header("Salto")]
    public Transform puntoPiso;
    public float groundCheckRadius = 0.2f;
    public LayerMask piso;
    public bool estaPiso;

    [Header("Sistema de poderes")]
    public Transform puntoAgarrePoder;
    public float radioInteraccion = 2f;
    public LayerMask capaPoderes;
    public float fuerzaUsoHaciaAdelante = 10f;
    public float fuerzaUsoHaciaArriba = 5f;

    [Header("Ground Pound")]
    private bool haciendoGroundPound = false;

    [Header("attach diamante")]
    public Transform diamante;
    private PowerUp poderSostenido;
    private bool quieto=false;
    public bool condiamante = false;
    

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

            
            if (!poderSostenido.SigueEnManoTrasUsar())
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

    public void IniciarGroundPound(float velocidadCaida)
    {
        if (estaPiso)
            return;

        haciendoGroundPound = true;

        moveInput = Vector2.zero;

        Vector3 velocidad = rb.linearVelocity;
        velocidad.x = 0f;
        velocidad.z = 0f;
        velocidad.y = -velocidadCaida;

        rb.linearVelocity = velocidad;
    }

    private void Update()
    {
        bool estabaEnPiso = estaPiso;

        estaPiso = Physics.CheckSphere(
            puntoPiso.position,
            groundCheckRadius,
            piso
        );

        if (haciendoGroundPound && estaPiso && !estabaEnPiso)
        {
            TerminarGroundPound();
        }
    }

    private void TerminarGroundPound()
    {
        haciendoGroundPound = false;

        Vector3 velocidad = rb.linearVelocity;
        velocidad.y = 0f;
        rb.linearVelocity = velocidad;

        moveInput = Vector2.zero;

        Debug.Log("Ground Pound: impacto contra el suelo");
    }


    private void FixedUpdate()
    {
        if (quieto) return;
        if (camara == null) return;

        if (haciendoGroundPound)
        {
            Vector3 velocidadGroundPound = rb.linearVelocity;

            velocidadGroundPound.x = 0f;
            velocidadGroundPound.z = 0f;

            rb.linearVelocity = velocidadGroundPound;

            return;
        }

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

    public void activarquieto()
    {
        quieto = true;
        moveInput = Vector2.zero;

        rb.linearVelocity = Vector3.zero;
    }
    public void desactivarquieto()
    {
        quieto = false;
        moveInput = Vector2.zero;
    }
}
