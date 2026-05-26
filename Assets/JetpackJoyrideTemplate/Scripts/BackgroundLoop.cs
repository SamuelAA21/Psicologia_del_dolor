using UnityEngine;

public class BackgroundLoop : MonoBehaviour
{
    [SerializeField] private float speed = 2f;
    [SerializeField] private float width = 20f;

    private Camera targetCamera;
    private float wrapWidth;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        targetCamera = Camera.main;
        wrapWidth = width > 0f ? width : GetSpriteWidth();
    }

    private void Update()
    {
        transform.position += Vector3.left * speed * Time.deltaTime;

        if (IsPastLeftBound())
        {
            MoveToRightOfLastBackground();
        }
    }

    private bool IsPastLeftBound()
    {
        if (targetCamera != null && targetCamera.orthographic)
        {
            float leftBound = targetCamera.transform.position.x - (targetCamera.orthographicSize * targetCamera.aspect);
            return transform.position.x + (wrapWidth * 0.5f) < leftBound;
        }

        return transform.position.x < -wrapWidth;
    }

    private void MoveToRightOfLastBackground()
    {
        BackgroundLoop[] loops = transform.parent != null
            ? transform.parent.GetComponentsInChildren<BackgroundLoop>()
            : FindObjectsByType<BackgroundLoop>(FindObjectsInactive.Exclude);

        float rightMostX = transform.position.x;

        foreach (BackgroundLoop loop in loops)
        {
            if (loop == null || loop == this)
            {
                continue;
            }

            rightMostX = Mathf.Max(rightMostX, loop.transform.position.x);
        }

        Vector3 position = transform.position;
        position.x = rightMostX + wrapWidth;
        transform.position = position;
    }

    private float GetSpriteWidth()
    {
        if (spriteRenderer == null)
        {
            return 20f;
        }

        return spriteRenderer.bounds.size.x;
    }
}
