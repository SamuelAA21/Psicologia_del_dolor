using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class JetpackCompletionOverlay : MonoBehaviour
{
    private const string TargetSceneName = "FirstMiniGame";

    [SerializeField] private string prefabResourcePath = "UI/JetpackCompletionOverlay";

    private Canvas completionCanvas;
    private Button continueButton;
    private Text summaryText;
    private Text rewardsText;
    private Action continueAction;
    private bool continuing;
    private static bool sceneHooked;

    public static JetpackCompletionOverlay Instance { get; private set; }

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
        if (scene.name != TargetSceneName || FindAnyObjectByType<JetpackCompletionOverlay>() != null)
        {
            return;
        }

        GameObject overlayObject = new GameObject(nameof(JetpackCompletionOverlay));
        overlayObject.AddComponent<JetpackCompletionOverlay>();
    }

    private void Awake()
    {
        Instance = this;
        BuildUi();
        Hide();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void ShowCompletion(Action onContinue)
    {
        continueAction = onContinue;
        continuing = false;

        if (summaryText != null)
        {
            summaryText.text =
                "Completaste los 4 ciclos de respiracion.\n\n" +
                "La practica no busca velocidad: busca notar el ritmo, subir con la inhalacion, sostener con calma y bajar con la exhalacion.";
        }

        if (rewardsText != null)
        {
            rewardsText.text =
                "Objetos y avances conseguidos:\n" +
                "- Respiracion consciente completada\n" +
                "- Progreso del compromiso registrado\n" +
                "- Camino de regreso a la Brujula desbloqueado";
        }

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        EnsureEventSystem();

        if (completionCanvas != null)
        {
            completionCanvas.gameObject.SetActive(true);
        }
    }

    private void Continue()
    {
        if (continuing)
        {
            return;
        }

        continuing = true;
        GameAudioManager.PlayUiClick();
        Time.timeScale = 1f;

        if (completionCanvas != null)
        {
            completionCanvas.gameObject.SetActive(false);
        }

        continueAction?.Invoke();
    }

    private void Hide()
    {
        if (completionCanvas != null)
        {
            completionCanvas.gameObject.SetActive(false);
        }
    }

    private void BuildUi()
    {
        if (TryBuildFromPrefab())
        {
            return;
        }

        completionCanvas = gameObject.AddComponent<Canvas>();
        completionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        completionCanvas.sortingOrder = 1210;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        gameObject.AddComponent<GraphicRaycaster>();

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        Image shade = CreateImage("CompletionShade", transform, new Color(0f, 0.05f, 0.08f, 0.62f));
        Stretch(shade.rectTransform);

        Image panel = CreateImage("CompletionPanel", transform, new Color(0.02f, 0.05f, 0.07f, 0.9f));
        RectTransform panelRect = panel.rectTransform;
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(720f, 470f);

        Text title = CreateText("Title", panelRect, font, 34, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
        title.text = "PRACTICA COMPLETADA";
        SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -34f), new Vector2(620f, 48f));

        summaryText = CreateText("Summary", panelRect, font, 20, FontStyle.Normal, TextAnchor.UpperLeft, new Color(0.92f, 0.98f, 1f));
        SetRect(summaryText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -100f), new Vector2(600f, 135f));

        rewardsText = CreateText("Rewards", panelRect, font, 19, FontStyle.Bold, TextAnchor.UpperLeft, new Color(1f, 0.86f, 0.52f));
        SetRect(rewardsText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -245f), new Vector2(600f, 100f));

        Image buttonImage = CreateImage("ContinueButton", panelRect, new Color(0.05f, 0.62f, 0.72f, 0.96f));
        SetRect(buttonImage.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 38f), new Vector2(270f, 58f));
        continueButton = buttonImage.gameObject.AddComponent<Button>();
        continueButton.targetGraphic = buttonImage;
        continueButton.onClick.AddListener(Continue);

        Text label = CreateText("Label", buttonImage.transform, font, 23, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
        label.text = "CONTINUAR";
        Stretch(label.rectTransform);
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

        completionCanvas = instance.GetComponentInChildren<Canvas>(true);
        continueButton = FindChildComponent<Button>(instance.transform, "ContinueButton");
        summaryText = FindChildComponent<Text>(instance.transform, "Summary");
        rewardsText = FindChildComponent<Text>(instance.transform, "Rewards");

        if (completionCanvas == null || continueButton == null || summaryText == null || rewardsText == null)
        {
            Destroy(instance);
            completionCanvas = null;
            continueButton = null;
            summaryText = null;
            rewardsText = null;
            return false;
        }

        continueButton.onClick.RemoveListener(Continue);
        continueButton.onClick.AddListener(Continue);
        return true;
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

    private static T FindChildComponent<T>(Transform root, string objectName) where T : Component
    {
        foreach (T component in root.GetComponentsInChildren<T>(true))
        {
            if (component.name == objectName)
            {
                return component;
            }
        }

        return null;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 position, Vector2 size)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
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
}
