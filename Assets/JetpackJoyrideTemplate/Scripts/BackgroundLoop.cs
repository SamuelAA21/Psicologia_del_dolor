using UnityEngine;

public class BackgroundLoop : MonoBehaviour
{
    public float speed = 2f;
    public float width = 20f;

    void Update()
    {
        transform.position += Vector3.left * speed * Time.deltaTime;

        if (transform.position.x < -width)
        {
            transform.position += new Vector3(width * 2f, 0, 0);
        }
    }
}