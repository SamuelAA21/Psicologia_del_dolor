using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    [SerializeField] private string obstacleTag = "Obstacle";

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryHandleCollision(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryHandleCollision(other.gameObject);
    }

    private void TryHandleCollision(GameObject other)
    {
        if (!other.CompareTag(obstacleTag) || GameManager.Instance == null)
        {
            return;
        }

        GameManager.Instance.GameOver();
    }
}
