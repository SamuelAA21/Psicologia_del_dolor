using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class JetpackRuntimeControls : MonoBehaviour
{
    [SerializeField] private string returnSceneName = GameSceneNames.MainGame;

    private Text pauseLabel;
    private bool paused;
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
        if (!GameSceneNames.IsBreathingMiniGame(scene.name) || FindAnyObjectByType<JetpackRuntimeControls>() != null)
        {
            return;
        }

        GameObject controlsObject = new GameObject(nameof(JetpackRuntimeControls));
        controlsObject.AddComponent<JetpackRuntimeControls>();
    }

    private void Awake()
    {
        BuildUi();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }

    public void TogglePause()
    {
        SetPaused(!paused);
    }

    public void RestartMiniGame()
    {
        SetPaused(false);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitMiniGame()
    {
        SetPaused(false);
        Time.timeScale = 1f;
        Chapter1ProgressState.ReportBreathingFailed();
        SceneLoader.LoadSceneSafe(returnSceneName);
    }

    private void SetPaused(bool value)
    {
        paused = value;
        Time.timeScale = paused ? 0f : 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (pauseLabel != null)
        {
            pauseLabel.text = paused ? "REANUDAR" : "PAUSA";
        }
    }

    private void BuildUi()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1150;

        RuntimeUiUtility.EnsureCanvasScaler(gameObject, new Vector2(1920f, 1080f));
        RuntimeUiUtility.EnsureGraphicRaycaster(gameObject);
        RuntimeUiUtility.EnsureEventSystem();

        Font font = RuntimeUiUtility.DefaultFont;

        Image panel = CreateImage("MiniGameControlsPanel", transform, new Color(0.02f, 0.05f, 0.07f, 0.72f));
        RectTransform panelRect = panel.rectTransform;
        panelRect.anchorMin = new Vector2(1f, 1f);
        panelRect.anchorMax = new Vector2(1f, 1f);
        panelRect.pivot = new Vector2(1f, 1f);
        panelRect.anchoredPosition = new Vector2(-24f, -24f);
        panelRect.sizeDelta = new Vector2(440f, 58f);

        Button pauseButton = CreateButton(panelRect, "PauseButton", "PAUSA", new Vector2(-292f, 0f), font, out pauseLabel);
        Button restartButton = CreateButton(panelRect, "RestartButton", "REINICIAR", new Vector2(-148f, 0f), font, out _);
        Button exitButton = CreateButton(panelRect, "ExitButton", "SALIR", new Vector2(-32f, 0f), font, out _);

        pauseButton.onClick.AddListener(TogglePause);
        restartButton.onClick.AddListener(RestartMiniGame);
        exitButton.onClick.AddListener(ExitMiniGame);
    }

    private static Button CreateButton(Transform parent, string objectName, string label, Vector2 position, Font font, out Text labelText)
    {
        Image image = CreateImage(objectName, parent, new Color(0.05f, 0.58f, 0.68f, 0.92f));
        RectTransform rect = image.rectTransform;
        rect.anchorMin = new Vector2(1f, 0.5f);
        rect.anchorMax = new Vector2(1f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(label == "REINICIAR" ? 132f : 104f, 38f);

        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;

        labelText = CreateText("Label", rect, font, 16, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
        RuntimeUiUtility.Stretch(labelText.rectTransform);
        labelText.text = label;
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

    private static Text CreateText(string objectName, Transform parent, Font font, int fontSize, FontStyle style, TextAnchor anchor, Color color)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(parent, false);

        Text text = textObject.AddComponent<Text>();
        text.font = font;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = anchor;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        return text;
    }
}
