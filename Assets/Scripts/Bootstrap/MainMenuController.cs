using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    private const string MenuSceneName = "Interfaz";

    [Header("Escenas")]
    [SerializeField] private string gameSceneName = "SampleScene";

    [Header("Botones")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button exitButton;

    [Header("Textos")]
    [SerializeField] private Text titleText;
    [SerializeField] private Text subtitleText;
    [SerializeField] private Text statusText;

    private bool isLoading;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallOnInitialScene()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
        EnsureMenuController(SceneManager.GetActiveScene());
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureMenuController(scene);
    }

    private static void EnsureMenuController(Scene scene)
    {
        if (!scene.name.Equals(MenuSceneName, System.StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (FindAnyObjectByType<MainMenuController>(FindObjectsInactive.Include) != null)
        {
            return;
        }

        GameObject controllerObject = new GameObject("MainMenuController");
        controllerObject.AddComponent<MainMenuController>();
    }

    private void Awake()
    {
        WireMenu();
    }

    private void OnDestroy()
    {
        if (playButton != null)
        {
            playButton.onClick.RemoveListener(StartGame);
        }

        if (exitButton != null)
        {
            exitButton.onClick.RemoveListener(QuitGame);
        }
    }

    public void StartGame()
    {
        if (isLoading)
        {
            return;
        }

        isLoading = true;
        SetStatus("Cargando...");
        SetButtonsInteractable(false);
        SceneLoader.LoadSceneSafe(gameSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void WireMenu()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>(FindObjectsInactive.Include);
        if (canvas == null)
        {
            canvas = CreateCanvas();
        }

        ConfigureCanvas(canvas);

        playButton = playButton != null ? playButton : FindButton("Play", "Jugar");
        if (playButton == null)
        {
            playButton = CreateMenuButton(canvas.transform, "Play", "JUGAR", new Vector2(0f, -245f), new Vector2(260f, 64f));
        }

        exitButton = exitButton != null ? exitButton : FindButton("Exit", "Salir");
        if (exitButton == null)
        {
            exitButton = CreateMenuButton(canvas.transform, "Exit", "SALIR", new Vector2(0f, -330f), new Vector2(220f, 52f));
        }

        titleText = titleText != null ? titleText : CreateTitle(canvas.transform);
        subtitleText = subtitleText != null ? subtitleText : CreateSubtitle(canvas.transform);
        statusText = statusText != null ? statusText : CreateStatusText(canvas.transform);

        EnsureButtonLabel(playButton, "JUGAR", 28);
        EnsureButtonLabel(exitButton, "SALIR", 20);

        playButton.onClick.RemoveListener(StartGame);
        playButton.onClick.AddListener(StartGame);
        exitButton.onClick.RemoveListener(QuitGame);
        exitButton.onClick.AddListener(QuitGame);

        SetStatus(string.Empty);
        SetButtonsInteractable(true);
    }

    private static Button FindButton(params string[] names)
    {
        foreach (Button button in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            foreach (string buttonName in names)
            {
                if (button.name.Equals(buttonName, System.StringComparison.OrdinalIgnoreCase))
                {
                    return button;
                }
            }
        }

        return null;
    }

    private static Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject("MainMenuCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private static void ConfigureCanvas(Canvas canvas)
    {
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;
        canvas.transform.localScale = Vector3.one;

        RectTransform rect = canvas.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
        }

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        if (canvas.GetComponent<GraphicRaycaster>() == null)
        {
            canvas.gameObject.AddComponent<GraphicRaycaster>();
        }
    }

    private static Button CreateMenuButton(Transform parent, string objectName, string label, Vector2 position, Vector2 size)
    {
        GameObject buttonObject = new GameObject(objectName);
        buttonObject.transform.SetParent(parent, false);

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.05f, 0.62f, 0.78f, 0.94f);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        EnsureButtonLabel(button, label, 24);
        return button;
    }

    private static void EnsureButtonLabel(Button button, string label, int fontSize)
    {
        if (button == null)
        {
            return;
        }

        Text text = button.GetComponentInChildren<Text>(true);
        if (text == null)
        {
            GameObject textObject = new GameObject("Label");
            textObject.transform.SetParent(button.transform, false);
            text = textObject.AddComponent<Text>();

            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        text.text = label;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.raycastTarget = false;
    }

    private static Text CreateTitle(Transform parent)
    {
        Text text = CreateText(parent, "Title", "PSICOLOGIA DEL DOLOR", 54, FontStyle.Bold, new Vector2(0f, 225f), new Vector2(900f, 80f));
        text.color = new Color(0.95f, 1f, 1f, 1f);
        return text;
    }

    private static Text CreateSubtitle(Transform parent)
    {
        return CreateText(parent, "Subtitle", "Explora, decide y respira para avanzar.", 24, FontStyle.Normal, new Vector2(0f, 165f), new Vector2(760f, 42f));
    }

    private static Text CreateStatusText(Transform parent)
    {
        Text text = CreateText(parent, "Status", string.Empty, 20, FontStyle.Bold, new Vector2(0f, -395f), new Vector2(480f, 36f));
        text.color = new Color(0.65f, 0.95f, 1f, 1f);
        return text;
    }

    private static Text CreateText(Transform parent, string objectName, string value, int fontSize, FontStyle style, Vector2 position, Vector2 size)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(parent, false);

        Text text = textObject.AddComponent<Text>();
        text.text = value;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        return text;
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }

    private void SetButtonsInteractable(bool interactable)
    {
        if (playButton != null)
        {
            playButton.interactable = interactable;
        }

        if (exitButton != null)
        {
            exitButton.interactable = interactable;
        }
    }
}
