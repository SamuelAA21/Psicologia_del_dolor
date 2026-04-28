using UnityEngine;

public class MoveLeft : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float destroyWhenPastX = -12f;

    private void Update()
    {
        transform.position += Vector3.left * speed * Time.deltaTime;

        if (transform.position.x <= destroyWhenPastX)
        {
            Destroy(gameObject);
        }
    }
}
