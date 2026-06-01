using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

public class Chapter1ProgressManager : MonoBehaviour
{
    public static Chapter1ProgressManager Instance { get; private set; }

    [Header("Referencias")]
    [SerializeField] private Chapter1GuidanceController guidanceController;
    [SerializeField] private Chapter1InventorySystem inventorySystem;
    [SerializeField] private GameObject compassTarget;
    [SerializeField] private DialogueRunner dialogueRunner;

    [Header("Estado")]
    [SerializeField] private string currentObjective;
    [SerializeField] private bool compassObtained;
    [SerializeField] private bool breathingCompleted;

    [Header("Notificaciones")]
    [SerializeField] private Canvas notificationCanvas;
    [SerializeField] private Text notificationText;
    [SerializeField] private float notificationSeconds = 3.25f;
    [SerializeField] private int sortingOrder = 3;

    private Coroutine notificationRoutine;

    public string CurrentObjective => currentObjective;
    public bool CompassObtained => compassObtained;
    public bool BreathingCompleted => breathingCompleted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        EnsureReferences();
        EnsureNotificationHud();
        HideNotification();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void SetObjective(string objective, Transform target)
    {
        currentObjective = objective;

        if (guidanceController != null)
        {
            guidanceController.SetObjective(objective, target);
        }
    }

    public void MarkCompassObtained()
    {
        compassObtained = true;
        Chapter1ProgressState.MarkCompassObtained();

        if (inventorySystem != null)
        {
            inventorySystem.AddCompass();
        }

        Notify("Brújula obtenida");
    }

    public void ConsumePendingMiniGameResult()
    {
        Chapter1MiniGameResult result = Chapter1ProgressState.ConsumePendingMiniGameResult();
        if (result == Chapter1MiniGameResult.None)
        {
            return;
        }

        if (result == Chapter1MiniGameResult.Completed)
        {
            breathingCompleted = true;
            Notify("Respiración completada");
            SetObjective("Respiración completada. Vuelve a la Brújula del Compromiso.", compassTarget != null ? compassTarget.transform : null);
            return;
        }

        Notify("Respiración interrumpida");
        SetObjective("Respiración interrumpida. Busca la Brújula e inténtalo de nuevo.", compassTarget != null ? compassTarget.transform : null);
    }

    public void Notify(string message)
    {
        if (string.IsNullOrWhiteSpace(message) || notificationCanvas == null || notificationText == null)
        {
            return;
        }

        if (notificationRoutine != null)
        {
            StopCoroutine(notificationRoutine);
        }

        notificationRoutine = StartCoroutine(ShowNotification(message));
    }

    public void Configure(Chapter1GuidanceController guidance, Chapter1InventorySystem inventory, GameObject compass)
    {
        guidanceController = guidance;
        inventorySystem = inventory;
        compassTarget = compass;
        EnsureReferences();
    }

    private void EnsureReferences()
    {
        if (guidanceController == null)
        {
            guidanceController = GetComponent<Chapter1GuidanceController>();
            if (guidanceController == null)
            {
                guidanceController = FindAnyObjectByType<Chapter1GuidanceController>(FindObjectsInactive.Include);
            }
        }

        if (inventorySystem == null)
        {
            inventorySystem = GetComponent<Chapter1InventorySystem>();
            if (inventorySystem == null)
            {
                inventorySystem = FindAnyObjectByType<Chapter1InventorySystem>(FindObjectsInactive.Include);
            }
        }

        if (dialogueRunner == null)
        {
            dialogueRunner = FindAnyObjectByType<DialogueRunner>(FindObjectsInactive.Include);
        }
    }

    private void EnsureNotificationHud()
    {
        if (notificationCanvas != null && notificationText != null)
        {
            notificationCanvas.sortingOrder = sortingOrder;
            return;
        }

        GameObject canvasObject = notificationCanvas != null
            ? notificationCanvas.gameObject
            : new GameObject("Chapter1_ProgressNotifications");

        notificationCanvas = canvasObject.GetComponent<Canvas>();
        if (notificationCanvas == null)
        {
            notificationCanvas = canvasObject.AddComponent<Canvas>();
        }

        notificationCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        notificationCanvas.sortingOrder = sortingOrder;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = canvasObject.AddComponent<CanvasScaler>();
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        if (canvasObject.GetComponent<GraphicRaycaster>() == null)
        {
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        GameObject panel = new GameObject("NotificationPanel");
        panel.transform.SetParent(canvasObject.transform, false);

        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.02f, 0.04f, 0.05f, 0.84f);

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 1f);
        panelRect.anchorMax = new Vector2(0.5f, 1f);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.anchoredPosition = new Vector2(0f, -32f);
        panelRect.sizeDelta = new Vector2(460f, 54f);

        GameObject textObject = new GameObject("NotificationText");
        textObject.transform.SetParent(panel.transform, false);
        notificationText = textObject.AddComponent<Text>();
        notificationText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        notificationText.fontSize = 20;
        notificationText.fontStyle = FontStyle.Bold;
        notificationText.alignment = TextAnchor.MiddleCenter;
        notificationText.color = Color.white;
        notificationText.horizontalOverflow = HorizontalWrapMode.Wrap;
        notificationText.verticalOverflow = VerticalWrapMode.Truncate;

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(18f, 8f);
        textRect.offsetMax = new Vector2(-18f, -8f);
    }

    private IEnumerator ShowNotification(string message)
    {
        EnsureReferences();

        notificationText.text = message;
        notificationCanvas.gameObject.SetActive(true);

        float elapsed = 0f;
        while (elapsed < notificationSeconds)
        {
            if (dialogueRunner != null && dialogueRunner.IsDialogueRunning)
            {
                notificationCanvas.gameObject.SetActive(false);
            }
            else if (!notificationCanvas.gameObject.activeSelf)
            {
                notificationCanvas.gameObject.SetActive(true);
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        HideNotification();
        notificationRoutine = null;
    }

    private void HideNotification()
    {
        if (notificationCanvas != null)
        {
            notificationCanvas.gameObject.SetActive(false);
        }
    }
}
