using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
/// <summary>
/// Movimiento en primera persona con New Input System + gravedad + animacion.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Camara")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 0.1f;

    [Header("Animacion")]
    [SerializeField] private Animator playerAnimator;

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private float verticalVelocity;
    private float xRotation;
    private bool controlEnabled = true;

    public bool ControlEnabled => controlEnabled;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Llamado automaticamente por PlayerInput component
    public void OnMove(InputValue value)
    {
        moveInput = controlEnabled ? value.Get<Vector2>() : Vector2.zero;
    }

    public void OnLook(InputValue value)
    {
        lookInput = controlEnabled ? value.Get<Vector2>() : Vector2.zero;
    }

    private void Update()
    {
        if (!controlEnabled)
        {
            playerAnimator?.SetBool("IsWalking", false);
            return;
        }

        HandleRotation();
        HandleMovement();
    }

    public void SetControlEnabled(bool enabled)
    {
        controlEnabled = enabled;

        if (enabled)
        {
            return;
        }

        moveInput = Vector2.zero;
        lookInput = Vector2.zero;
        playerAnimator?.SetBool("IsWalking", false);
    }

    private void HandleRotation()
    {
        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleMovement()
    {
        Vector3 move = (transform.right * moveInput.x + transform.forward * moveInput.y).normalized;

        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = move * moveSpeed;
        velocity.y = verticalVelocity;

        controller.Move(velocity * Time.deltaTime);

        bool isMoving = move.magnitude > 0.1f;
        playerAnimator?.SetBool("IsWalking", isMoving);
    }
}
