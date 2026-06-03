using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private Camera targetCamera;
    [SerializeField, Min(0.05f)] private float spawnInterval = 0.35f;
    [SerializeField] private bool useCameraBounds = true;
    [SerializeField] private bool spawnFromCameraRightEdge = true;
    [SerializeField] private float spawnXPosition = 10f;
    [SerializeField, Min(0f)] private float horizontalSpawnPadding = 0.5f;
    [SerializeField] private float obstacleLifetime = 10f;
    [SerializeField] private Vector2 fallbackSpawnYRange = new Vector2(-2f, 2f);
    [SerializeField] private Vector2 playAreaYRange = new Vector2(-2.5f, 4f);
    [SerializeField, Min(0.25f)] private float corridorHeight = 2.5f;
    [SerializeField, Min(0f)] private float obstacleVerticalSpacing = 0f;
    [SerializeField, Min(0f)] private float corridorPadding = 0.25f;
    [SerializeField] private BreathingController breathingController;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private bool compensateObstacleTravelTime = true;
    [SerializeField, Min(0f)] private float timingPadding = 0.1f;

    private float spawnTimer;

    private void Awake()
    {
        ResolveBreathingController();

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        ResolvePlayerTransform();
    }

    private void Update()
    {
        if (!CanSpawn())
        {
            return;
        }

        spawnTimer += Time.deltaTime;

        if (spawnTimer < spawnInterval)
        {
            return;
        }

        spawnTimer = 0f;
        Spawn();
    }

    private void Spawn()
    {
        if (obstaclePrefab == null)
        {
            Debug.LogWarning($"{nameof(ObstacleSpawner)} on {name} has no obstacle prefab assigned.");
            return;
        }

        Vector2 sortedPlayArea = GetPlayableYRange();
        float centerY = GetCorridorCenterY();
        float halfCorridor = corridorHeight * 0.5f;
        float corridorBottomEdge = Mathf.Clamp(
            centerY - halfCorridor - corridorPadding,
            sortedPlayArea.x,
            sortedPlayArea.y);
        float corridorTopEdge = Mathf.Clamp(
            centerY + halfCorridor + corridorPadding,
            sortedPlayArea.x,
            sortedPlayArea.y);

        SpawnObstacleBand(sortedPlayArea.x, corridorBottomEdge);
        SpawnObstacleBand(corridorTopEdge, sortedPlayArea.y);
    }

    private float GetCorridorCenterY()
    {
        if (breathingController != null)
        {
            return breathingController.GetTherapeuticCenterY(GetTherapeuticLookAheadSeconds());
        }

        Vector2 fallbackRange = GetSortedRange(fallbackSpawnYRange);
        return (fallbackRange.x + fallbackRange.y) * 0.5f;
    }

    private void SpawnObstacleBand(float startY, float endY)
    {
        if (endY <= startY)
        {
            return;
        }

        float obstacleHeight = GetObstacleHeight();
        float spacing = obstacleVerticalSpacing > 0f ? obstacleVerticalSpacing : obstacleHeight;
        float halfHeight = obstacleHeight * 0.5f;
        float firstCenterY = startY + halfHeight;
        float lastCenterY = endY - halfHeight;

        if (lastCenterY < firstCenterY)
        {
            SpawnObstacleAt((startY + endY) * 0.5f);
            return;
        }

        int obstacleCount = Mathf.Max(0, Mathf.CeilToInt((lastCenterY - firstCenterY) / spacing));

        for (int i = 0; i <= obstacleCount; i++)
        {
            float y = Mathf.Min(firstCenterY + (i * spacing), lastCenterY);
            SpawnObstacleAt(y);
        }
    }

    private void SpawnObstacleAt(float y)
    {
        GameObject obstacle = Instantiate(
            obstaclePrefab,
            new Vector3(GetSpawnXPosition(), y, 0f),
            Quaternion.identity);

        Destroy(obstacle, obstacleLifetime);
    }

    private static Vector2 GetSortedRange(Vector2 range)
    {
        return new Vector2(
            Mathf.Min(range.x, range.y),
            Mathf.Max(range.x, range.y));
    }

    private float GetObstacleHeight()
    {
        if (obstaclePrefab == null)
        {
            return 1f;
        }

        BoxCollider2D boxCollider = obstaclePrefab.GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            return Mathf.Abs(boxCollider.size.y * obstaclePrefab.transform.localScale.y);
        }

        SpriteRenderer spriteRenderer = obstaclePrefab.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            return Mathf.Abs(spriteRenderer.sprite.bounds.size.y * obstaclePrefab.transform.localScale.y);
        }

        return 1f;
    }

    private Vector2 GetPlayableYRange()
    {
        if (useCameraBounds && targetCamera != null && targetCamera.orthographic)
        {
            float halfHeight = targetCamera.orthographicSize;
            float centerY = targetCamera.transform.position.y;
            return new Vector2(centerY - halfHeight, centerY + halfHeight);
        }

        return GetSortedRange(playAreaYRange);
    }

    private float GetSpawnXPosition()
    {
        if (spawnFromCameraRightEdge && targetCamera != null && targetCamera.orthographic)
        {
            float halfWidth = targetCamera.orthographicSize * targetCamera.aspect;
            return targetCamera.transform.position.x + halfWidth + horizontalSpawnPadding;
        }

        return spawnXPosition;
    }

    private bool CanSpawn()
    {
        ResolveBreathingController();

        if (GameManager.Instance != null && !GameManager.Instance.CanPlay)
        {
            return false;
        }

        if (breathingController != null && !breathingController.IsRunning)
        {
            return false;
        }

        return obstaclePrefab != null;
    }

    private void ResolveBreathingController()
    {
        if (breathingController != null)
        {
            return;
        }

        MiniGameFlowController flowController = FindAnyObjectByType<MiniGameFlowController>();
        if (flowController != null && flowController.Controller != null)
        {
            breathingController = flowController.Controller;
            return;
        }

        breathingController = FindAnyObjectByType<BreathingController>();
    }

    private void ResolvePlayerTransform()
    {
        if (playerTransform != null)
        {
            return;
        }

        PlayerJetpack player = FindAnyObjectByType<PlayerJetpack>();
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    private float GetTherapeuticLookAheadSeconds()
    {
        if (!compensateObstacleTravelTime)
        {
            return 0f;
        }

        ResolvePlayerTransform();

        if (playerTransform == null)
        {
            return timingPadding;
        }

        float speed = GetObstacleSpeed();
        if (speed <= 0.01f)
        {
            return timingPadding;
        }

        float distanceToPlayer = Mathf.Abs(GetSpawnXPosition() - playerTransform.position.x);
        return (distanceToPlayer / speed) + timingPadding;
    }

    private float GetObstacleSpeed()
    {
        if (obstaclePrefab == null)
        {
            return 0f;
        }

        MoveLeft moveLeft = obstaclePrefab.GetComponent<MoveLeft>();
        return moveLeft != null ? moveLeft.Speed : 5f;
    }
}
