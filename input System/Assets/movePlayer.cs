using UnityEngine;
using UnityEngine.InputSystem;

public class movePlayer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private NIS inputAccion;

    private Vector2 moveInput;

    public float speed= 5f;

    public float jumpForce = 5f;

    private Rigidbody rb;

    public void Awake()
    {
        inputAccion = new NIS();
        rb = GetComponent<Rigidbody>();
    }
    private void OnEnable()
    {
        inputAccion.Player.Enable();

        inputAccion.Player.Move.performed += OnMove;
        inputAccion.Player.Move.canceled += OnMove;

        inputAccion.Player.Jump.performed += OnJump;
    }
    private void OnDisable()
    {

        inputAccion.Player.Move.performed -= OnMove;
        inputAccion.Player.Move.canceled -= OnMove;

        inputAccion.Player.Jump.performed -= OnJump;

        inputAccion.Player.Disable();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    private void OnJump(InputAction.CallbackContext context)
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y);
        transform.Translate(movement * speed * Time.deltaTime);
    }
}
