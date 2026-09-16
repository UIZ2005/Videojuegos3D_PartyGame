using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
public class movePlayer : MonoBehaviour
{
  
    private Vector2 moveInput;
    public float speed = 5f;
    public float jumpForce = 5f;
    public float rotationSpeed = 150f;
    private Rigidbody rb;
    public GameObject[] players;

    // para el salto
    public Transform puntoPiso;      
    public float groundCheckRadius = 0.2f;
    public LayerMask piso;      // layer del piso
    public bool estaPiso;


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

    private void OnLook(InputValue context)
    {

    }:
    // Update is called once per frame
    void Update()
    {
        // Verifica si est� tocando el suelo
        estaPiso = Physics.CheckSphere(puntoPiso.position, groundCheckRadius, piso);
        // Movimiento hacia adelante / atr�s
        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y);
        transform.position += movement * speed * Time.deltaTime;
    }
}