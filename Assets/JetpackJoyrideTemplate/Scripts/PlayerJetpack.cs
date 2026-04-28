using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerJetpack : MonoBehaviour
{
    [SerializeField] private float thrustVelocity = 8f;
    [SerializeField] private float maxFallSpeed = -6f;
    [SerializeField] private InputActionReference thrustAction;
    [SerializeField] private Key fallbackKeyboardKey = Key.Space;

    private Rigidbody2D rb;
    private bool isThrusting;
    public bool IsThrusting => isThrusting;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError($"{nameof(PlayerJetpack)} requires a {nameof(Rigidbody2D)} on {name}.");
        }
    }

    private void OnEnable()
    {
        thrustAction?.action?.Enable();
    }

    private void OnDisable()
    {
        thrustAction?.action?.Disable();
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

        if (isThrusting)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, thrustVelocity);
        }

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
