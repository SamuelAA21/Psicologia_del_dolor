using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Yarn.Unity;

public class PauseMenuController : MonoBehaviour
{
    private const string MenuSceneName = "Interfaz";
    private const string BootstrapSceneName = "Bootstrap";

    [SerializeField] private Canvas pauseCanvas;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button musicDownButton;
    [SerializeField] private Button musicUpButton;
    [SerializeField] private Text musicVolumeText;
    [SerializeField] private DialogueRunner dialogueRunner;

    private bool paused;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
        EnsureForScene(SceneManager.GetActiveScene());
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureForScene(scene);
    }

    private static void EnsureForScene(Scene scene)
    {
        if (!IsGameplayScene(scene.name))
        {
            return;
        }

        if (FindAnyObjectByType<PauseMenuController>(FindObjectsInactive.Include) != null)
        {
            return;
        }

        GameObject controllerObject = new GameObject("PauseMenuController");
        controllerObject.AddComponent<PauseMenuController>();
    }

    private static bool IsGameplayScene(string sceneName)
    {
        return !sceneName.Equals(MenuSceneName, System.StringComparison.OrdinalIgnoreCase)
            && !sceneName.Equals(BootstrapSceneName, System.StringComparison.OrdinalIgnoreCase);
    }

    private void Awake()
    {
        EnsureUi();
        SetPaused(false);
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame && !IsDialogueRunning())
        {
            SetPaused(!paused);
        }

        if (paused)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            RefreshMusicVolumeText();
        }
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }

    public void Resume()
    {
        GameAudioManager.PlayUiClick();
        SetPaused(false);
    }

    public void ReturnToMainMenu()
    {
        GameAudioManager.PlayUiClick();
        Time.timeScale = 1f;
        paused = false;
        SceneLoader.LoadSceneSafe(MenuSceneName);
    }

    public void QuitGame()
    {
        GameAudioManager.PlayUiClick();
        Time.timeScale = 1f;
        Application.Quit();
    }

    public void LowerMusicVolume()
    {
        GameAudioManager.PlayUiClick();
        GameAudioManager.AdjustMusicVolume(-0.1f);
        RefreshMusicVolumeText();
    }

    public void RaiseMusicVolume()
    {
        GameAudioManager.PlayUiClick();
        GameAudioManager.AdjustMusicVolume(0.1f);
        RefreshMusicVolumeText();
    }

    private void SetPaused(bool value)
    {
        paused = value;
        Time.timeScale = paused ? 0f : 1f;

        if (pauseCanvas != null)
        {
            pauseCanvas.gameObject.SetActive(paused);
        }

        if (paused)
        {
            EnsureEventSystem();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        if (FindAnyObjectByType<PlayerController>(FindObjectsInactive.Exclude) != null)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void EnsureUi()
    {
        if (pauseCanvas != null)
        {
            return;
        }

        GameObject canvasObject = new GameObject("PauseMenuCanvas");
        pauseCanvas = canvasObject.AddComponent<Canvas>();
        pauseCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        pauseCanvas.sortingOrder = 900;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject overlay = new GameObject("Overlay");
        overlay.transform.SetParent(canvasObject.transform, false);
        Image overlayImage = overlay.AddComponent<Image>();
        overlayImage.color = new Color(0f, 0f, 0f, 0.58f);

        RectTransform overlayRect = overlay.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(canvasObject.transform, false);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.02f, 0.05f, 0.06f, 0.92f);

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(520f, 510f);

        CreateText(panel.transform, "Title", "PAUSA", 44, FontStyle.Bold, new Vector2(0f, 185f), new Vector2(440f, 70f));
        CreateText(panel.transform, "Hint", "Esc para volver al juego", 18, FontStyle.Normal, new Vector2(0f, 140f), new Vector2(440f, 34f));

        resumeButton = CreateButton(panel.transform, "Resume", "REANUDAR", new Vector2(0f, 75f));
        musicVolumeText = CreateText(panel.transform, "MusicVolume", string.Empty, 20, FontStyle.Bold, new Vector2(0f, 15f), new Vector2(300f, 34f));
        musicDownButton = CreateButton(panel.transform, "MusicDown", "MUSICA -", new Vector2(-105f, -40f), new Vector2(180f, 48f));
        musicUpButton = CreateButton(panel.transform, "MusicUp", "MUSICA +", new Vector2(105f, -40f), new Vector2(180f, 48f));
        mainMenuButton = CreateButton(panel.transform, "MainMenu", "MENU INICIAL", new Vector2(0f, -115f));
        quitButton = CreateButton(panel.transform, "Quit", "SALIR", new Vector2(0f, -190f));

        resumeButton.onClick.AddListener(Resume);
        musicDownButton.onClick.AddListener(LowerMusicVolume);
        musicUpButton.onClick.AddListener(RaiseMusicVolume);
        mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        quitButton.onClick.AddListener(QuitGame);
        RefreshMusicVolumeText();
    }

    private bool IsDialogueRunning()
    {
        if (dialogueRunner == null)
        {
            dialogueRunner = FindAnyObjectByType<DialogueRunner>(FindObjectsInactive.Include);
        }

        return dialogueRunner != null && dialogueRunner.IsDialogueRunning;
    }

    private static Button CreateButton(Transform parent, string objectName, string label, Vector2 position)
    {
        return CreateButton(parent, objectName, label, position, new Vector2(300f, 54f));
    }

    private static Button CreateButton(Transform parent, string objectName, string label, Vector2 position, Vector2 size)
    {
        GameObject buttonObject = new GameObject(objectName);
        buttonObject.transform.SetParent(parent, false);

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.04f, 0.55f, 0.68f, 0.96f);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        CreateText(buttonObject.transform, "Label", label, 22, FontStyle.Bold, Vector2.zero, size);
        return button;
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
        text.raycastTarget = false;

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        return text;
    }

    private static void EnsureEventSystem()
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            eventSystem = FindAnyObjectByType<EventSystem>(FindObjectsInactive.Include);
        }

        if (eventSystem == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystem = eventSystemObject.AddComponent<EventSystem>();
        }

        eventSystem.gameObject.SetActive(true);

        if (eventSystem.GetComponent<BaseInputModule>() == null)
        {
            eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }
    }

    private void RefreshMusicVolumeText()
    {
        if (musicVolumeText == null)
        {
            return;
        }

        musicVolumeText.text = $"MUSICA {Mathf.RoundToInt(GameAudioManager.MusicVolume * 100f)}%";
    }
}
