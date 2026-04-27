using UnityEngine;

[RequireComponent(typeof(CharacterController))]
/// <summary>
/// Movimiento en primera persona con WASD + rotacion por mouse + animacion.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Camara")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 100f;

    [Header("Animacion")]
    [SerializeField] private Animator playerAnimator;

    private CharacterController controller;
    private float verticalVelocity;
    private float xRotation;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleRotation();
        HandleMovement();
    }

    private void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Rotar camara verticalmente
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);
        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Rotar personaje horizontalmente
        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 move = (transform.right * horizontal + transform.forward * vertical).normalized;

        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = move * moveSpeed;
        velocity.y = verticalVelocity;

        controller.Move(velocity * Time.deltaTime);

        // Animacion
        bool isMoving = move.magnitude > 0.1f;
        playerAnimator?.SetBool("IsWalking", isMoving);
    }
}