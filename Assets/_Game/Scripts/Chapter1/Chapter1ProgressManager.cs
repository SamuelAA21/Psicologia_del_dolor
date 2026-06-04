using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

public class Chapter1ProgressManager : MonoBehaviour
{
    public static Chapter1ProgressManager Instance { get; private set; }

    private const string Door1PostBreathingNode = "Puerta1_PostRespiracion";

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
    private Coroutine dialogueStartRoutine;
    private Coroutine restorePlayerRoutine;

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

        GameAudioManager.PlayReward();
        Notify("Brújula obtenida");
    }

    public void ConsumePendingMiniGameResult()
    {
        bool wasDoor1BreathingChallenge = Chapter1ProgressState.ConsumePendingDoor1BreathingChallenge();
        Chapter1MiniGameResult result = Chapter1ProgressState.ConsumePendingMiniGameResult();
        if (result == Chapter1MiniGameResult.None)
        {
            return;
        }

        if (result == Chapter1MiniGameResult.Completed)
        {
            breathingCompleted = true;
            GameAudioManager.PlayReward();
            if (wasDoor1BreathingChallenge && Chapter1EnvironmentController.Instance != null)
            {
                Notify("Respiracion completada");
                Chapter1EnvironmentController.Instance.UnlockStage("Puerta1");
                RestoreSavedPlayerTransform();
                StartDialogueNodeWhenReady(Door1PostBreathingNode);
                return;
            }

            Notify("Respiración completada");
            SetObjective("Respiración completada. Continúa con la narrativa.", null);
            return;
        }

        GameAudioManager.PlayError();
        if (wasDoor1BreathingChallenge && Chapter1EnvironmentController.Instance != null)
        {
            Notify("Respiracion interrumpida");
            Chapter1EnvironmentController.Instance.UnlockStage("Puerta1");
            RestoreSavedPlayerTransform();
            return;
        }

        Notify("Respiración interrumpida");
        SetObjective("Respiración interrumpida. Vuelve a intentarlo desde la Puerta 1.", null);
    }

    private void StartDialogueNodeWhenReady(string nodeName)
    {
        if (dialogueStartRoutine != null)
        {
            StopCoroutine(dialogueStartRoutine);
        }

        dialogueStartRoutine = StartCoroutine(StartDialogueNodeRoutine(nodeName));
    }

    private void RestoreSavedPlayerTransform()
    {
        if (restorePlayerRoutine != null)
        {
            StopCoroutine(restorePlayerRoutine);
        }

        restorePlayerRoutine = StartCoroutine(RestoreSavedPlayerTransformRoutine());
    }

    private IEnumerator RestoreSavedPlayerTransformRoutine()
    {
        const int maxFramesToWait = 90;
        for (int frame = 0; frame < maxFramesToWait; frame++)
        {
            if (!Chapter1ProgressState.TryGetSavedPlayerTransform(out Vector3 position, out Quaternion rotation))
            {
                restorePlayerRoutine = null;
                yield break;
            }

            PlayerController player = FindAnyObjectByType<PlayerController>(FindObjectsInactive.Include);
            if (player != null)
            {
                player.TeleportTo(position, rotation);
                Chapter1ProgressState.ClearSavedPlayerTransform();
                restorePlayerRoutine = null;
                yield break;
            }

            yield return null;
        }

        Debug.LogWarning($"{nameof(Chapter1ProgressManager)} could not restore the player position after returning from the breathing minigame.", this);
        restorePlayerRoutine = null;
    }

    private IEnumerator StartDialogueNodeRoutine(string nodeName)
    {
        if (string.IsNullOrWhiteSpace(nodeName))
        {
            yield break;
        }

        const int maxFramesToWait = 90;
        for (int frame = 0; frame < maxFramesToWait; frame++)
        {
            EnsureReferences();

            if (dialogueRunner != null && !dialogueRunner.IsDialogueRunning)
            {
                yield return null;
                _ = dialogueRunner.StartDialogue(nodeName);
                dialogueStartRoutine = null;
                yield break;
            }

            yield return null;
        }

        Debug.LogWarning($"{nameof(Chapter1ProgressManager)} could not continue Yarn node '{nodeName}' after returning from the breathing minigame.", this);
        dialogueStartRoutine = null;
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
        notificationText.font = RuntimeUiUtility.DefaultFont;
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
