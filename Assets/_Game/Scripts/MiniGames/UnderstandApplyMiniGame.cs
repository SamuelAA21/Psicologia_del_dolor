using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UnderstandApplyMiniGame : MonoBehaviour
{
    private const string DefaultDeckResourcePath = "MiniGames/UnderstandApply/Chapter1UnderstandApplyDeck";

    [SerializeField] private UnderstandApplyDeck deck;

    private readonly List<UnderstandApplyCard> cards = new List<UnderstandApplyCard>();
    private Action completed;
    private Canvas canvas;
    private RectTransform cardRect;
    private RectTransform understandZone;
    private RectTransform applyZone;
    private Text cardText;
    private Text progressText;
    private Text feedbackText;
    private Text titleText;
    private Text finalText;
    private Button understandButton;
    private Button applyButton;
    private Button continueButton;
    private int currentIndex;
    private bool showingFinal;

    public static void StartGame(UnderstandApplyDeck deckOverride, Action onCompleted)
    {
        if (FindAnyObjectByType<UnderstandApplyMiniGame>(FindObjectsInactive.Include) != null)
        {
            return;
        }

        GameObject gameObject = new GameObject(nameof(UnderstandApplyMiniGame));
        UnderstandApplyMiniGame miniGame = gameObject.AddComponent<UnderstandApplyMiniGame>();
        miniGame.deck = deckOverride;
        miniGame.completed = onCompleted;
    }

    private void Awake()
    {
        ResolveDeck();
        BuildCards();
        if (cards.Count == 0)
        {
            Debug.LogWarning($"{nameof(UnderstandApplyMiniGame)} has no cards. Completing immediately.", this);
            Complete();
            return;
        }

        BuildUi();
        ShowCard(0);
        SetPlayerControl(false);
    }

    private void OnDestroy()
    {
        SetPlayerControl(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        SetPlayerControl(false);
    }

    private void ResolveDeck()
    {
        if (deck != null)
        {
            return;
        }

        deck = Resources.Load<UnderstandApplyDeck>(DefaultDeckResourcePath);
    }

    private void BuildCards()
    {
        cards.Clear();

        if (deck != null && deck.Cards != null)
        {
            foreach (UnderstandApplyCard card in deck.Cards)
            {
                if (card != null && !string.IsNullOrWhiteSpace(card.Text))
                {
                    cards.Add(card);
                }
            }
        }

    }

    private void BuildUi()
    {
        canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1225;

        RuntimeUiUtility.EnsureCanvasScaler(gameObject, new Vector2(1920f, 1080f));
        RuntimeUiUtility.EnsureGraphicRaycaster(gameObject);
        RuntimeUiUtility.EnsureEventSystem();

        Font font = RuntimeUiUtility.DefaultFont;

        Image shade = CreateImage("Shade", transform, new Color(0.86f, 0.96f, 0.9f, 0.94f));
        RuntimeUiUtility.Stretch(shade.rectTransform);

        Image panel = CreateImage("Panel", transform, new Color(0.98f, 1f, 0.96f, 1f));
        RectTransform panelRect = panel.rectTransform;
        RuntimeUiUtility.SetRect(panelRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(920f, 640f));

        titleText = CreateText("Title", panelRect, font, 30, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.1f, 0.24f, 0.2f));
        RuntimeUiUtility.SetRect(titleText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -34f), new Vector2(780f, 48f));
        titleText.text = "ENTENDER O APLICAR";

        progressText = CreateText("Progress", panelRect, font, 20, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.18f, 0.56f, 0.35f));
        RuntimeUiUtility.SetRect(progressText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -82f), new Vector2(220f, 32f));

        Image card = CreateImage("Card", panelRect, Color.white);
        cardRect = card.rectTransform;
        RuntimeUiUtility.SetRect(cardRect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -148f), new Vector2(700f, 160f));
        card.gameObject.AddComponent<UnderstandApplyDraggableCard>().Configure(this);

        cardText = CreateText("CardText", cardRect, font, 27, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.1f, 0.14f, 0.18f));
        RuntimeUiUtility.Stretch(cardText.rectTransform);
        cardText.rectTransform.offsetMin = new Vector2(36f, 20f);
        cardText.rectTransform.offsetMax = new Vector2(-36f, -20f);

        understandButton = CreateZone(panelRect, "UnderstandZone", "LA ENTIENDO", new Vector2(-220f, -118f), new Color(0.71f, 0.9f, 1f), out understandZone);
        applyButton = CreateZone(panelRect, "ApplyZone", "LA APLICO", new Vector2(220f, -118f), new Color(0.74f, 0.95f, 0.67f), out applyZone);
        understandButton.onClick.AddListener(() => Submit(UnderstandApplyCategory.Understand));
        applyButton.onClick.AddListener(() => Submit(UnderstandApplyCategory.Apply));

        feedbackText = CreateText("Feedback", panelRect, font, 24, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.18f, 0.5f, 0.32f));
        RuntimeUiUtility.SetRect(feedbackText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 42f), new Vector2(760f, 42f));

        finalText = CreateText("FinalText", panelRect, font, 25, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.1f, 0.24f, 0.2f));
        RuntimeUiUtility.SetRect(finalText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 8f), new Vector2(760f, 250f));
        finalText.gameObject.SetActive(false);

        continueButton = CreateButton(panelRect, "ContinueButton", "CONTINUAR", new Vector2(0f, -232f), new Vector2(250f, 56f), new Color(0.32f, 0.78f, 0.42f), font);
        continueButton.onClick.AddListener(Complete);
        continueButton.gameObject.SetActive(false);
    }

    public void SubmitDroppedCard(Vector2 screenPosition)
    {
        if (showingFinal)
        {
            return;
        }

        Camera eventCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
        if (RectTransformUtility.RectangleContainsScreenPoint(understandZone, screenPosition, eventCamera))
        {
            Submit(UnderstandApplyCategory.Understand);
            return;
        }

        if (RectTransformUtility.RectangleContainsScreenPoint(applyZone, screenPosition, eventCamera))
        {
            Submit(UnderstandApplyCategory.Apply);
        }
    }

    private void Submit(UnderstandApplyCategory selected)
    {
        if (showingFinal || currentIndex < 0 || currentIndex >= cards.Count)
        {
            return;
        }

        UnderstandApplyCard currentCard = cards[currentIndex];
        if (currentCard.Category != selected)
        {
            feedbackText.color = new Color(0.54f, 0.38f, 0.13f);
            feedbackText.text = "Piensa si esto describe comprender o actuar.";
            StartCoroutine(Pulse(cardRect, new Vector3(1.02f, 1.02f, 1f)));
            return;
        }

        GameAudioManager.PlayReward();
        feedbackText.color = new Color(0.16f, 0.54f, 0.3f);
        feedbackText.text = selected == UnderstandApplyCategory.Apply ? "Eso es aplicación." : "Correcto.";
        StartCoroutine(AdvanceAfterFeedback());
    }

    private IEnumerator AdvanceAfterFeedback()
    {
        SetZonesInteractable(false);
        yield return Pulse(cardRect, new Vector3(1.06f, 1.06f, 1f));
        yield return new WaitForSecondsRealtime(0.35f);

        currentIndex++;
        if (currentIndex >= cards.Count)
        {
            ShowFinal();
            yield break;
        }

        ShowCard(currentIndex);
        SetZonesInteractable(true);
    }

    private void ShowCard(int index)
    {
        currentIndex = index;
        feedbackText.text = string.Empty;
        progressText.text = $"{currentIndex + 1}/{cards.Count}";
        cardText.text = cards[currentIndex].Text;
        cardRect.anchoredPosition = new Vector2(0f, -148f);
        cardRect.localScale = Vector3.one;
    }

    private void ShowFinal()
    {
        showingFinal = true;
        SetZonesInteractable(false);
        cardRect.gameObject.SetActive(false);
        understandButton.gameObject.SetActive(false);
        applyButton.gameObject.SetActive(false);
        progressText.text = $"{cards.Count}/{cards.Count}";
        feedbackText.text = string.Empty;
        titleText.text = "AVATAR CLINICO";
        finalText.text =
            "Comprender abre la puerta. Aplicar te permite cruzarla.\n\n" +
            "Cada vez que llevas una idea a la práctica, el aprendizaje deja de quedarse en la cabeza y empieza a cambiar tu experiencia.";
        finalText.gameObject.SetActive(true);
        continueButton.gameObject.SetActive(true);
    }

    private void Complete()
    {
        completed?.Invoke();
        Destroy(gameObject);
    }

    private void SetZonesInteractable(bool interactable)
    {
        understandButton.interactable = interactable;
        applyButton.interactable = interactable;
    }

    private static IEnumerator Pulse(RectTransform rect, Vector3 targetScale)
    {
        Vector3 start = rect.localScale;
        const float duration = 0.16f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            rect.localScale = Vector3.Lerp(start, targetScale, t);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            rect.localScale = Vector3.Lerp(targetScale, Vector3.one, t);
            yield return null;
        }
    }

    private static Button CreateZone(Transform parent, string objectName, string label, Vector2 position, Color color, out RectTransform zoneRect)
    {
        Button button = CreateButton(parent, objectName, label, position, new Vector2(330f, 118f), color, RuntimeUiUtility.DefaultFont);
        zoneRect = button.transform as RectTransform;
        return button;
    }

    private static Button CreateButton(Transform parent, string objectName, string label, Vector2 position, Vector2 size, Color color, Font font)
    {
        Image image = CreateImage(objectName, parent, color);
        RectTransform rect = image.rectTransform;
        RuntimeUiUtility.SetRect(rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, size);

        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;

        Text text = CreateText("Label", rect, font, 25, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.08f, 0.18f, 0.16f));
        RuntimeUiUtility.Stretch(text.rectTransform);
        text.text = label;
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

    private static void SetPlayerControl(bool enabled)
    {
        PlayerController player = FindAnyObjectByType<PlayerController>(FindObjectsInactive.Include);
        if (player != null)
        {
            player.SetControlEnabled(enabled);
        }

        Cursor.lockState = enabled ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !enabled;
    }
}
