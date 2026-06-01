using UnityEngine;
using UnityEngine.SceneManagement;

public class BreathingTargetZoneVisualizer : MonoBehaviour
{
    private const string TargetSceneName = "FirstMiniGame";

    [SerializeField] private BreathingController breathingController;
    [SerializeField] private BreathingTherapyGuide therapyGuide;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float horizontalPadding = 0.35f;
    [SerializeField] private int sortingOrder = -20;

    private SpriteRenderer bandRenderer;
    private static bool sceneHooked;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        InstallForScene(SceneManager.GetActiveScene());

        if (sceneHooked)
        {
            return;
        }

        SceneManager.sceneLoaded += (scene, mode) => InstallForScene(scene);
        sceneHooked = true;
    }

    private static void InstallForScene(Scene scene)
    {
        if (scene.name != TargetSceneName || FindAnyObjectByType<BreathingTargetZoneVisualizer>() != null)
        {
            return;
        }

        GameObject visualizerObject = new GameObject(nameof(BreathingTargetZoneVisualizer));
        visualizerObject.AddComponent<BreathingTargetZoneVisualizer>();
    }

    private void Awake()
    {
        ResolveReferences();
        CreateBand();
    }

    private void LateUpdate()
    {
        ResolveReferences();

        if (breathingController == null || targetCamera == null || bandRenderer == null)
        {
            return;
        }

        if (!breathingController.TryGetCurrentPhaseSettings(out BreathingPhaseSettings settings))
        {
            bandRenderer.enabled = false;
            return;
        }

        bandRenderer.enabled = breathingController.IsRunning;

        Vector2 targetRange = settings.GetSortedPlayerTargetRange();
        float height = Mathf.Max(0.1f, targetRange.y - targetRange.x);
        float centerY = (targetRange.x + targetRange.y) * 0.5f;
        float width = GetCameraWorldWidth() + horizontalPadding;

        transform.position = new Vector3(targetCamera.transform.position.x, centerY, 0f);
        transform.localScale = new Vector3(width, height, 1f);

        bool inZone = therapyGuide != null && therapyGuide.IsPlayerInTargetZone;
        bandRenderer.color = inZone
            ? new Color(0.24f, 1f, 0.45f, 0.2f)
            : new Color(0.24f, 0.82f, 1f, 0.16f);
    }

    private void ResolveReferences()
    {
        if (breathingController == null)
        {
            MiniGameFlowController flowController = FindAnyObjectByType<MiniGameFlowController>();
            breathingController = flowController != null && flowController.Controller != null
                ? flowController.Controller
                : FindAnyObjectByType<BreathingController>();
        }

        if (therapyGuide == null)
        {
            therapyGuide = FindAnyObjectByType<BreathingTherapyGuide>();
        }

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    private void CreateBand()
    {
        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();

        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        bandRenderer = gameObject.AddComponent<SpriteRenderer>();
        bandRenderer.sprite = sprite;
        bandRenderer.sortingOrder = sortingOrder;
        bandRenderer.color = new Color(0.24f, 0.82f, 1f, 0.16f);
    }

    private float GetCameraWorldWidth()
    {
        if (targetCamera != null && targetCamera.orthographic)
        {
            return targetCamera.orthographicSize * 2f * targetCamera.aspect;
        }

        return 18f;
    }
}
