using System.Collections;
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
    public float dashForce = 12f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 1f;

    private bool haciendoDash = false;
    private bool dashDisponible = true;

    [Header("Cooldown Ataque")]
    public float attackCooldown = 3f;
    private bool ataqueDisponible = true;


    [Header("burbuja")]
    public GameObject burbujaPrefab;
    public Transform puntoAtaque;


    [Header("Mira")]
    public float rotationSpeed = 15f;
    public Camera camara;
    public bool apuntar=false;

    [Header("Objetivos")]
    public GameObject canvaObj;

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
    private GroundPound groundPoundActivo;

    [Header("attach diamante")]
    public Transform diamante;
    private PowerUp poderSostenido;
    private bool quieto=false;
    public bool condiamante = false;

    [Header("Skins")]
    public GameObject[] players;
    public int numplayer;


    public void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Evita que las colisiones inclinen al jugador.
        rb.constraints |= RigidbodyConstraints.FreezeRotationX
                        | RigidbodyConstraints.FreezeRotationZ;

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnMira(InputValue context)
    {
        if (!context.isPressed) return;

        if (apuntar)
        {
            apuntar = false;
        }
        else
        {
            apuntar = true;
        }
        Debug.Log(apuntar);
    }

    //ATAQUES ESPECIALES SEGUN EL ESPIRITU ELEGIDO

    private void OnAttack(InputValue context)
    {
        if (!context.isPressed) return;

        if (!ataqueDisponible) return;

        if (quieto)
            return;

        if (haciendoGroundPound)
            return;

        if (haciendoDash)
            return;

        switch (numplayer)
        {
            case 1:
                //agua
                GameObject ataque = Instantiate(
                    burbujaPrefab,
                    puntoAtaque.position,
                    puntoAtaque.rotation
                );

                BubbleAttack burbuja = ataque.GetComponent<BubbleAttack>();
                burbuja.Inicializar(numplayer);
                break;
            case 2:
                //fuego
                break;
            case 3:
                //aire
                break;
            case 4:
                //rayo
                break;
        }
        StartCoroutine(CooldownAttack());
    }

    IEnumerator CooldownAttack()
    {
        ataqueDisponible = false;

        yield return new WaitForSeconds(attackCooldown);

        ataqueDisponible = true;
    }
    private void OnMove(InputValue context)
    {
        if (quieto) return;
        moveInput = context.Get<Vector2>();
    }

    private void OnSprint(InputValue context)
    {
        if (!context.isPressed) return;

        if (!dashDisponible) return;
        if (haciendoDash) return;
        if (quieto) return;
        if (haciendoGroundPound) return;

        StartCoroutine(Dash());
    }

    // La acción Look debe estar vinculada al mouse.
    private void OnLook(InputValue context)
    {
        lookInput = context.Get<Vector2>();
    }

    private void OnJump(InputValue context)
    {
        if (quieto) return;
        if (estaPiso)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void OnInteract(InputValue context)
    {
        if (quieto) return;
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

    public void IniciarGroundPound(
    float velocidadCaida,
    GroundPound groundPound
    )
    {
        if (estaPiso)
            return;

        haciendoGroundPound = true;
        groundPoundActivo = groundPound;

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

        if (groundPoundActivo != null)
        {
            groundPoundActivo.EjecutarImpacto(
                puntoPiso.position
            );

            groundPoundActivo = null;
        }

        Debug.Log("Ground Pound: impacto contra el suelo");
    }


    private void FixedUpdate()
    {
        if (quieto) return;
        if (haciendoDash)
            return;

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
        if (apuntar)
        {
            RotarJugadorConCamara();
        }else if (movimiento.sqrMagnitude > 0.01f)

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

    private void RotarJugadorConCamara()
    {
        if (camara == null)
            return;

        Vector3 direccionCamara = camara.transform.forward;

        // Ignoramos la inclinación vertical de la cámara.
        direccionCamara.y = 0f;

        if (direccionCamara.sqrMagnitude < 0.01f)
            return;

        direccionCamara.Normalize();

        Quaternion rotacionObjetivo =
            Quaternion.LookRotation(direccionCamara, Vector3.up);

        Quaternion nuevaRotacion = Quaternion.Slerp(
            rb.rotation,
            rotacionObjetivo,
            rotationSpeed * Time.fixedDeltaTime
        );

        rb.MoveRotation(nuevaRotacion);
    }


    IEnumerator Dash()
    {
        dashDisponible = false;
        haciendoDash = true;

        // Dirección del dash basada en el movimiento del jugador
        Vector3 direccionDash;

        // Dirección relativa a la cámara
        Vector3 direccionAdelante = camara.transform.forward;
        Vector3 direccionDerecha = camara.transform.right;

        direccionAdelante.y = 0f;
        direccionDerecha.y = 0f;

        direccionAdelante.Normalize();
        direccionDerecha.Normalize();

        direccionDash =
            direccionDerecha * moveInput.x +
            direccionAdelante * moveInput.y;

        // Si no hay input, dash hacia donde mira el jugador
        if (direccionDash.sqrMagnitude < 0.01f)
        {
            direccionDash = transform.forward;
        }

        direccionDash.Normalize();

        // Mantener la velocidad vertical actual
        Vector3 velocidadDash = direccionDash * dashForce;
        velocidadDash.y = rb.linearVelocity.y;

        rb.linearVelocity = velocidadDash;

        // Rotar hacia la dirección del dash
        Quaternion rotacionObjetivo =
            Quaternion.LookRotation(direccionDash, Vector3.up);

        rb.MoveRotation(rotacionObjetivo);

        yield return new WaitForSeconds(dashDuration);

        haciendoDash = false;

        yield return new WaitForSeconds(dashCooldown);

        dashDisponible = true;
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

    public void skinplayer(int jugador)
    {
        numplayer = jugador;
        players[jugador - 1].SetActive(true);
    }


}
