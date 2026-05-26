using UnityEngine;
using UnityEngine.InputSystem;

public class Player2D : MonoBehaviour
{
    public float jumpForce = 8f;
    public float maxFallSpeed = -6f;
    public Key thrustKey = Key.Space;

    private Rigidbody2D rb;

    private void Awake()
    {
        if (TryGetComponent(out PlayerJetpack jetpack) && jetpack.enabled)
        {
            enabled = false;
        }
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (rb == null)
        {
            return;
        }

        if (Keyboard.current != null && Keyboard.current[thrustKey].isPressed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // Limitar caida.
        if (rb.linearVelocity.y < maxFallSpeed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, maxFallSpeed);
        }
    }
}
