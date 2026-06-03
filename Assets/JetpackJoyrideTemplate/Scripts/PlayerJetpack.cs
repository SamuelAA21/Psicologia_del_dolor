using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerJetpack : MonoBehaviour
{
    [SerializeField] private float thrustVelocity = 8f;
    [SerializeField] private float descentVelocity = -8f;
    [SerializeField] private float maxFallSpeed = -6f;
    [SerializeField] private bool useControlledGravity = true;
    [SerializeField] private float controlledGravityScale = 0f;
    [SerializeField] private InputActionReference thrustAction;
    [SerializeField] private Key fallbackKeyboardKey = Key.Space;

    private Rigidbody2D rb;
    private bool isThrusting;
    private float originalGravityScale;

    public bool IsThrusting => isThrusting;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError($"{nameof(PlayerJetpack)} requires a {nameof(Rigidbody2D)} on {name}.");
            return;
        }

        originalGravityScale = rb.gravityScale;

        if (useControlledGravity)
        {
            rb.gravityScale = controlledGravityScale;
        }
    }

    private void OnEnable()
    {
        thrustAction?.action?.Enable();
    }

    private void OnDisable()
    {
        thrustAction?.action?.Disable();

        if (rb != null && useControlledGravity)
        {
            rb.gravityScale = originalGravityScale;
        }
    }

    private void Update()
    {
        isThrusting = ReadThrustInput();
    }

    private void FixedUpdate()
    {
        if (rb == null || (GameManager.Instance != null && !GameManager.Instance.CanPlay))
        {
            return;
        }

        float targetVerticalVelocity = isThrusting ? thrustVelocity : descentVelocity;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, targetVerticalVelocity);

        ClampFallSpeed();
    }

    private bool ReadThrustInput()
    {
        if (thrustAction != null && thrustAction.action != null)
        {
            return thrustAction.action.IsPressed();
        }

        return Keyboard.current != null && Keyboard.current[fallbackKeyboardKey].isPressed;
    }

    private void ClampFallSpeed()
    {
        if (rb.linearVelocity.y >= maxFallSpeed)
        {
            return;
        }

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, maxFallSpeed);
    }
}
