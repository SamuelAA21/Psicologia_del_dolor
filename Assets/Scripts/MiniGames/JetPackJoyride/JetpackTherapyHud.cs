using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class JetpackTherapyHud : MonoBehaviour
{
    private const string TargetSceneName = "FirstMiniGame";

    [SerializeField] private BreathingController breathingController;
    [SerializeField] private BreathingTherapyGuide therapyGuide;

    private Text phaseText;
    private Text instructionText;
    private Text cycleText;
    private Text scoreText;
    private Text statusText;
    private Text livesText;
    private Image phaseFill;
    private Image statusDot;

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
        if (scene.name != TargetSceneName || FindAnyObjectByType<JetpackTherapyHud>() != null)
        {
            return;
        }

        GameObject hudObject = new GameObject(nameof(JetpackTherapyHud));
        hudObject.AddComponent<JetpackTherapyHud>();
    }

    private void Awake()
    {
        ResolveReferences();
        BuildUi();
    }

    private void Update()
    {
        ResolveReferences();

        if (breathingController == null)
        {
            return;
        }

        BreathingPhaseSettings settings = breathingController.CurrentPhaseSettings;
        phaseText.text = GetPhaseLabel(settings.phase);
        instructionText.text = therapyGuide != null && !string.IsNullOrWhiteSpace(therapyGuide.CurrentInstruction)
            ? therapyGuide.CurrentInstruction
            : settings.guidanceText;

        int totalCycles = Mathf.Max(1, breathingController.SessionCycleCount);
        int shownCycle = breathingController.IsRunning
            ? Mathf.Clamp(breathingController.CompletedCycles + 1, 1, totalCycles)
            : Mathf.Clamp(breathingController.CompletedCycles, 0, totalCycles);
        cycleText.text = $"Ciclo {shownCycle}/{totalCycles}";

        float score = therapyGuide != null ? therapyGuide.RegulationScore : 0f;
        scoreText.text = $"{Mathf.RoundToInt(score * 100f)}% regulacion";

        if (GameManager.Instance != null)
        {
            livesText.text = $"Intentos {GameManager.Instance.RemainingMistakes}/{GameManager.Instance.MaxMistakes}";
        }

        bool inZone = therapyGuide != null && therapyGuide.IsPlayerInTargetZone;
        statusText.text = inZone ? "En zona" : "Ajusta altura";
        statusText.color = inZone ? new Color(0.64f, 1f, 0.74f) : new Color(1f, 0.86f, 0.48f);
        statusDot.color = inZone ? new Color(0.18f, 0.95f, 0.42f) : new Color(1f, 0.65f, 0.18f);
        phaseFill.fillAmount = breathingController.CurrentPhaseProgress;
        phaseFill.color = GetPhaseColor(settings.phase);
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
    }

    private void BuildUi()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        gameObject.AddComponent<GraphicRaycaster>();

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        Image panel = CreateImage("Panel", transform, new Color(0.03f, 0.05f, 0.07f, 0.76f));
        RectTransform panelRect = panel.rectTransform;
        panelRect.anchorMin = new Vector2(0.5f, 1f);
        panelRect.anchorMax = new Vector2(0.5f, 1f);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.anchoredPosition = new Vector2(0f, -18f);
        panelRect.sizeDelta = new Vector2(620f, 132f);

        phaseText = CreateText("Phase", panelRect, font, 28, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white);
        RectTransform phaseRect = phaseText.rectTransform;
        phaseRect.anchorMin = new Vector2(0f, 1f);
        phaseRect.anchorMax = new Vector2(0f, 1f);
        phaseRect.pivot = new Vector2(0f, 1f);
        phaseRect.anchoredPosition = new Vector2(24f, -16f);
        phaseRect.sizeDelta = new Vector2(280f, 36f);

        instructionText = CreateText("Instruction", panelRect, font, 19, FontStyle.Normal, TextAnchor.MiddleLeft, new Color(0.93f, 0.97f, 1f));
        RectTransform instructionRect = instructionText.rectTransform;
        instructionRect.anchorMin = new Vector2(0f, 1f);
        instructionRect.anchorMax = new Vector2(1f, 1f);
        instructionRect.pivot = new Vector2(0.5f, 1f);
        instructionRect.anchoredPosition = new Vector2(0f, -52f);
        instructionRect.sizeDelta = new Vector2(-48f, 30f);

        Image barBack = CreateImage("PhaseProgressBack", panelRect, new Color(1f, 1f, 1f, 0.18f));
        RectTransform barBackRect = barBack.rectTransform;
        barBackRect.anchorMin = new Vector2(0f, 0f);
        barBackRect.anchorMax = new Vector2(1f, 0f);
        barBackRect.pivot = new Vector2(0.5f, 0f);
        barBackRect.anchoredPosition = new Vector2(0f, 18f);
        barBackRect.sizeDelta = new Vector2(-48f, 12f);

        phaseFill = CreateImage("PhaseProgressFill", barBackRect, new Color(0.38f, 0.84f, 1f, 0.95f));
        phaseFill.type = Image.Type.Filled;
        phaseFill.fillMethod = Image.FillMethod.Horizontal;
        phaseFill.fillOrigin = 0;
        RectTransform fillRect = phaseFill.rectTransform;
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        cycleText = CreateText("Cycle", panelRect, font, 16, FontStyle.Bold, TextAnchor.MiddleRight, Color.white);
        RectTransform cycleRect = cycleText.rectTransform;
        cycleRect.anchorMin = new Vector2(1f, 1f);
        cycleRect.anchorMax = new Vector2(1f, 1f);
        cycleRect.pivot = new Vector2(1f, 1f);
        cycleRect.anchoredPosition = new Vector2(-24f, -18f);
        cycleRect.sizeDelta = new Vector2(160f, 24f);

        scoreText = CreateText("Score", panelRect, font, 15, FontStyle.Normal, TextAnchor.MiddleRight, new Color(0.78f, 0.9f, 1f));
        RectTransform scoreRect = scoreText.rectTransform;
        scoreRect.anchorMin = new Vector2(1f, 1f);
        scoreRect.anchorMax = new Vector2(1f, 1f);
        scoreRect.pivot = new Vector2(1f, 1f);
        scoreRect.anchoredPosition = new Vector2(-24f, -42f);
        scoreRect.sizeDelta = new Vector2(170f, 22f);

        livesText = CreateText("Lives", panelRect, font, 15, FontStyle.Bold, TextAnchor.MiddleRight, new Color(1f, 0.74f, 0.58f));
        RectTransform livesRect = livesText.rectTransform;
        livesRect.anchorMin = new Vector2(1f, 1f);
        livesRect.anchorMax = new Vector2(1f, 1f);
        livesRect.pivot = new Vector2(1f, 1f);
        livesRect.anchoredPosition = new Vector2(-24f, -66f);
        livesRect.sizeDelta = new Vector2(170f, 22f);

        statusDot = CreateImage("StatusDot", panelRect, Color.white);
        RectTransform dotRect = statusDot.rectTransform;
        dotRect.anchorMin = new Vector2(1f, 0f);
        dotRect.anchorMax = new Vector2(1f, 0f);
        dotRect.pivot = new Vector2(1f, 0f);
        dotRect.anchoredPosition = new Vector2(-130f, 43f);
        dotRect.sizeDelta = new Vector2(12f, 12f);

        statusText = CreateText("Status", panelRect, font, 15, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white);
        RectTransform statusRect = statusText.rectTransform;
        statusRect.anchorMin = new Vector2(1f, 0f);
        statusRect.anchorMax = new Vector2(1f, 0f);
        statusRect.pivot = new Vector2(1f, 0f);
        statusRect.anchoredPosition = new Vector2(-24f, 36f);
        statusRect.sizeDelta = new Vector2(96f, 24f);
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

    private static string GetPhaseLabel(BreathPhase phase)
    {
        switch (phase)
        {
            case BreathPhase.Inhale:
                return "INHALA";
            case BreathPhase.Hold:
                return "SOSTEN";
            case BreathPhase.Exhale:
                return "EXHALA";
            default:
                return phase.ToString().ToUpperInvariant();
        }
    }

    private static Color GetPhaseColor(BreathPhase phase)
    {
        switch (phase)
        {
            case BreathPhase.Inhale:
                return new Color(0.36f, 0.82f, 1f, 0.95f);
            case BreathPhase.Hold:
                return new Color(1f, 0.86f, 0.36f, 0.95f);
            case BreathPhase.Exhale:
                return new Color(0.48f, 1f, 0.66f, 0.95f);
            default:
                return Color.white;
        }
    }
}
