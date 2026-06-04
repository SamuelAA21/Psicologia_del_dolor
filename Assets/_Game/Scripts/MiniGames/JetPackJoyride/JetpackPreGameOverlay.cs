using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class JetpackPreGameOverlay : MonoBehaviour
{
    [SerializeField] private BreathingController breathingController;
    [SerializeField] private string prefabResourcePath = "UI/JetpackPreGameOverlay";

    private Canvas overlayCanvas;
    private Button playButton;
    private bool started;
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
        if (!GameSceneNames.IsBreathingMiniGame(scene.name) || FindAnyObjectByType<JetpackPreGameOverlay>() != null)
        {
            return;
        }

        GameObject overlayObject = new GameObject(nameof(JetpackPreGameOverlay));
        overlayObject.AddComponent<JetpackPreGameOverlay>();
    }

    private void Awake()
    {
        ResolveReferences();
        BuildUi();
        Show();
    }

    private void Start()
    {
        PrepareMiniGame();
    }

    private void Show()
    {
        started = false;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (overlayCanvas != null)
        {
            overlayCanvas.gameObject.SetActive(true);
        }

        RuntimeUiUtility.EnsureEventSystem();
    }

    private void Play()
    {
        if (started)
        {
            return;
        }

        started = true;
        GameAudioManager.PlayUiClick();
        PrepareMiniGame();

        if (breathingController != null)
        {
            breathingController.StartCycle();
        }

        if (overlayCanvas != null)
        {
            overlayCanvas.gameObject.SetActive(false);
        }

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void PrepareMiniGame()
    {
        ResolveReferences();

        if (breathingController == null)
        {
            return;
        }

        breathingController.StopCycle();
        breathingController.ResetCycle();
    }

    private void ResolveReferences()
    {
        if (breathingController != null)
        {
            return;
        }

        breathingController = MiniGameRuntimeUtility.ResolveBreathingController();
    }

    private void BuildUi()
    {
        if (TryBuildFromPrefab())
        {
            return;
        }

        overlayCanvas = gameObject.AddComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.sortingOrder = 1200;

        RuntimeUiUtility.EnsureCanvasScaler(gameObject, new Vector2(1920f, 1080f));

        RuntimeUiUtility.EnsureGraphicRaycaster(gameObject);

        Font font = RuntimeUiUtility.DefaultFont;

        Image dimmer = CreateImage("SoftBlurOverlay", transform, new Color(0.64f, 0.83f, 0.9f, 0.38f));
        RuntimeUiUtility.Stretch(dimmer.rectTransform);

        Image shade = CreateImage("DepthShade", transform, new Color(0f, 0.05f, 0.08f, 0.42f));
        RuntimeUiUtility.Stretch(shade.rectTransform);

        Image panel = CreateImage("InstructionPanel", transform, new Color(0.02f, 0.05f, 0.07f, 0.82f));
        RectTransform panelRect = panel.rectTransform;
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(620f, 390f);

        Text title = CreateText("Title", panelRect, font, 34, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
        RectTransform titleRect = title.rectTransform;
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -34f);
        titleRect.sizeDelta = new Vector2(540f, 48f);
        title.text = "RESPIRACION GUIADA";

        Text body = CreateText("Body", panelRect, font, 21, FontStyle.Normal, TextAnchor.UpperLeft, new Color(0.9f, 0.98f, 1f, 1f));
        RectTransform bodyRect = body.rectTransform;
        bodyRect.anchorMin = new Vector2(0.5f, 1f);
        bodyRect.anchorMax = new Vector2(0.5f, 1f);
        bodyRect.pivot = new Vector2(0.5f, 1f);
        bodyRect.anchoredPosition = new Vector2(0f, -104f);
        bodyRect.sizeDelta = new Vector2(500f, 155f);
        body.text =
            "Inhala: manten Espacio para subir.\n" +
            "Sosten: pulsa y suelta suave para mantener altura.\n" +
            "Exhala: suelta Espacio para bajar.\n\n" +
            "Sigue la franja y respira con ritmo, no con prisa.";

        playButton = CreateButton(panelRect, font);
        playButton.onClick.AddListener(Play);
    }

    private bool TryBuildFromPrefab()
    {
        if (string.IsNullOrWhiteSpace(prefabResourcePath))
        {
            return false;
        }

        GameObject prefab = Resources.Load<GameObject>(prefabResourcePath);
        if (prefab == null)
        {
            return false;
        }

        GameObject instance = Instantiate(prefab, transform, false);
        instance.name = prefab.name;

        overlayCanvas = instance.GetComponentInChildren<Canvas>(true);
        playButton = RuntimeUiUtility.FindChildComponent<Button>(instance.transform, "PlayButton");

        if (overlayCanvas == null || playButton == null)
        {
            Destroy(instance);
            overlayCanvas = null;
            playButton = null;
            return false;
        }

        playButton.onClick.RemoveListener(Play);
        playButton.onClick.AddListener(Play);
        return true;
    }

    private static Button CreateButton(Transform parent, Font font)
    {
        Image image = CreateImage("PlayButton", parent, new Color(0.05f, 0.62f, 0.72f, 0.96f));
        RectTransform rect = image.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, 42f);
        rect.sizeDelta = new Vector2(250f, 62f);

        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;

        Text label = CreateText("Label", rect, font, 25, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
        RuntimeUiUtility.Stretch(label.rectTransform);
        label.text = "PLAY";

        return button;
    }

    private static Image CreateImage(string objectName, Transform parent, Color color)
    {
        GameObject imageObject = new GameObject(objectName);
        imageObject.transform.SetParent(parent, false);
        Image image = imageObject.AddComponent<Image>();
        image.color = color;
        return image;
    }

    private static Text CreateText(string objectName, Transform parent, Font font, int size, FontStyle style, TextAnchor anchor, Color color)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(parent, false);
        Text text = textObject.AddComponent<Text>();
        text.font = font;
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = anchor;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        return text;
    }

}
