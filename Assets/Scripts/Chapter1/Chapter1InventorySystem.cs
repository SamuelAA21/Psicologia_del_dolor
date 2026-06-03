using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Yarn.Unity;

public class Chapter1InventorySystem : MonoBehaviour
{
    public static Chapter1InventorySystem Instance { get; private set; }

    [Serializable]
    private class InventorySlot
    {
        public string itemId;
        public string displayName;
        public Sprite icon;
        public Color color = Color.white;
        public int quantity;

        public bool IsEmpty => string.IsNullOrWhiteSpace(itemId) || quantity <= 0;

        public void Set(string newItemId, string newDisplayName, Sprite newIcon, Color newColor, int amount)
        {
            itemId = newItemId;
            displayName = newDisplayName;
            icon = newIcon;
            color = newColor;
            quantity = Mathf.Max(1, amount);
        }

        public void Add(int amount)
        {
            quantity += Mathf.Max(1, amount);
        }

        public void Clear()
        {
            itemId = string.Empty;
            displayName = string.Empty;
            icon = null;
            color = Color.white;
            quantity = 0;
        }
    }

    [Header("Configuracion")]
    [SerializeField, Range(1, 9)] private int slotCount = 9;
    [SerializeField] private bool createHudOnAwake = true;
    [SerializeField] private bool allowNumberSelection = true;
    [SerializeField] private bool allowMouseWheelSelection = true;

    [Header("Item de brujula")]
    [SerializeField] private string compassItemId = "compass";
    [SerializeField] private string compassDisplayName = "Brújula del Compromiso";
    [SerializeField] private Sprite compassIcon;
    [SerializeField] private Color compassColor = new Color(1f, 0.82f, 0.25f, 1f);

    [Header("HUD")]
    [SerializeField] private Canvas inventoryCanvas;
    [SerializeField] private int sortingOrder = 2;
    [SerializeField] private Vector2 referenceResolution = new Vector2(1920f, 1080f);
    [SerializeField] private Vector2 slotSize = new Vector2(68f, 68f);
    [SerializeField] private float slotSpacing = 8f;
    [SerializeField] private float bottomOffset = 34f;
    [SerializeField] private Color slotColor = new Color(0.04f, 0.05f, 0.06f, 0.82f);
    [SerializeField] private Color selectedSlotColor = new Color(0.18f, 0.42f, 0.52f, 0.96f);
    [SerializeField] private Color borderColor = new Color(0.85f, 0.94f, 1f, 1f);
    [SerializeField] private Text selectedItemLabel;

    [Header("Comportamiento con dialogos")]
    [SerializeField] private bool hideDuringDialogue = true;
    [SerializeField] private DialogueRunner dialogueRunner;

    private InventorySlot[] slots;
    private Image[] slotBackgrounds;
    private Image[] iconImages;
    private Text[] fallbackIconTexts;
    private Text[] quantityTexts;
    private int selectedSlotIndex;
    private bool inventoryVisible = true;

    public int SelectedSlotIndex => selectedSlotIndex;
    public bool HasCompass => HasItem(compassItemId);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        EnsureSlots();

        if (createHudOnAwake)
        {
            EnsureHud();
        }

        EnsureDialogueRunner();
        RefreshHud();
        RefreshDialogueVisibility();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        RefreshDialogueVisibility();

        if (IsDialogueRunning())
        {
            return;
        }

        if (allowNumberSelection)
        {
            HandleNumberSelection();
        }

        if (allowMouseWheelSelection)
        {
            HandleMouseWheelSelection();
        }
    }

    public void AddCompass()
    {
        if (HasCompass)
        {
            SelectFirstSlotWithItem(compassItemId);
            return;
        }

        AddItem(compassItemId, compassDisplayName, compassIcon, compassColor, 1);
        SelectFirstSlotWithItem(compassItemId);
    }

    public bool HasItem(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return false;
        }

        EnsureSlots();
        foreach (InventorySlot slot in slots)
        {
            if (!slot.IsEmpty && slot.itemId == itemId)
            {
                return true;
            }
        }

        return false;
    }

    public bool AddItem(string itemId, string displayName, Sprite icon, Color color, int amount = 1)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return false;
        }

        EnsureSlots();

        foreach (InventorySlot slot in slots)
        {
            if (!slot.IsEmpty && slot.itemId == itemId)
            {
                slot.Add(amount);
                RefreshHud();
                return true;
            }
        }

        foreach (InventorySlot slot in slots)
        {
            if (slot.IsEmpty)
            {
                slot.Set(itemId, displayName, icon, color, amount);
                RefreshHud();
                return true;
            }
        }

        return false;
    }

    public void SelectSlot(int index)
    {
        EnsureSlots();
        selectedSlotIndex = Mathf.Clamp(index, 0, slots.Length - 1);
        RefreshHud();
    }

    private void SelectFirstSlotWithItem(string itemId)
    {
        EnsureSlots();
        for (int i = 0; i < slots.Length; i++)
        {
            if (!slots[i].IsEmpty && slots[i].itemId == itemId)
            {
                SelectSlot(i);
                return;
            }
        }
    }

    public void Clear()
    {
        EnsureSlots();
        foreach (InventorySlot slot in slots)
        {
            slot.Clear();
        }

        selectedSlotIndex = 0;
        RefreshHud();
    }

    private void EnsureSlots()
    {
        int safeSlotCount = Mathf.Clamp(slotCount, 1, 9);
        if (slots != null && slots.Length == safeSlotCount)
        {
            return;
        }

        InventorySlot[] previousSlots = slots;
        slots = new InventorySlot[safeSlotCount];
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i] = previousSlots != null && i < previousSlots.Length && previousSlots[i] != null
                ? previousSlots[i]
                : new InventorySlot();
        }

        selectedSlotIndex = Mathf.Clamp(selectedSlotIndex, 0, slots.Length - 1);
    }

    private void EnsureHud()
    {
        if (inventoryCanvas != null && slotBackgrounds != null && slotBackgrounds.Length == slots.Length)
        {
            return;
        }

        GameObject canvasObject = inventoryCanvas != null
            ? inventoryCanvas.gameObject
            : new GameObject("Chapter1_InventoryHUD");

        inventoryCanvas = canvasObject.GetComponent<Canvas>();
        if (inventoryCanvas == null)
        {
            inventoryCanvas = canvasObject.AddComponent<Canvas>();
        }

        inventoryCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        inventoryCanvas.sortingOrder = sortingOrder;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = canvasObject.AddComponent<CanvasScaler>();
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = referenceResolution;
        scaler.matchWidthOrHeight = 0.5f;

        if (canvasObject.GetComponent<GraphicRaycaster>() == null)
        {
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        Transform existingRoot = canvasObject.transform.Find("Hotbar");
        if (existingRoot != null)
        {
            Destroy(existingRoot.gameObject);
        }

        GameObject hotbar = new GameObject("Hotbar");
        hotbar.transform.SetParent(canvasObject.transform, false);

        RectTransform hotbarRect = hotbar.AddComponent<RectTransform>();
        hotbarRect.anchorMin = new Vector2(0.5f, 0f);
        hotbarRect.anchorMax = new Vector2(0.5f, 0f);
        hotbarRect.pivot = new Vector2(0.5f, 0f);
        hotbarRect.anchoredPosition = new Vector2(0f, bottomOffset);
        hotbarRect.sizeDelta = new Vector2((slotSize.x * slots.Length) + (slotSpacing * (slots.Length - 1)), slotSize.y);

        slotBackgrounds = new Image[slots.Length];
        iconImages = new Image[slots.Length];
        fallbackIconTexts = new Text[slots.Length];
        quantityTexts = new Text[slots.Length];

        for (int i = 0; i < slots.Length; i++)
        {
            CreateSlot(hotbar.transform, i);
        }

        selectedItemLabel = CreateText(canvasObject.transform, "SelectedItemLabel", 18, FontStyle.Bold, TextAnchor.MiddleCenter);
        RectTransform labelRect = selectedItemLabel.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.5f, 0f);
        labelRect.anchorMax = new Vector2(0.5f, 0f);
        labelRect.pivot = new Vector2(0.5f, 0f);
        labelRect.anchoredPosition = new Vector2(0f, bottomOffset + slotSize.y + 22f);
        labelRect.sizeDelta = new Vector2(420f, 28f);
    }

    private void CreateSlot(Transform parent, int index)
    {
        GameObject slotObject = new GameObject($"Slot_{index + 1}");
        slotObject.transform.SetParent(parent, false);

        RectTransform slotRect = slotObject.AddComponent<RectTransform>();
        slotRect.anchorMin = new Vector2(0f, 0.5f);
        slotRect.anchorMax = new Vector2(0f, 0.5f);
        slotRect.pivot = new Vector2(0f, 0.5f);
        slotRect.anchoredPosition = new Vector2(index * (slotSize.x + slotSpacing), 0f);
        slotRect.sizeDelta = slotSize;

        Image background = slotObject.AddComponent<Image>();
        background.color = slotColor;
        slotBackgrounds[index] = background;

        Text numberText = CreateText(slotObject.transform, "Number", 12, FontStyle.Bold, TextAnchor.UpperLeft);
        numberText.text = (index + 1).ToString();
        numberText.color = new Color(0.8f, 0.85f, 0.9f, 0.9f);
        RectTransform numberRect = numberText.GetComponent<RectTransform>();
        numberRect.anchorMin = Vector2.zero;
        numberRect.anchorMax = Vector2.one;
        numberRect.offsetMin = new Vector2(7f, 4f);
        numberRect.offsetMax = new Vector2(-6f, -4f);

        GameObject iconObject = new GameObject("Icon");
        iconObject.transform.SetParent(slotObject.transform, false);
        Image iconImage = iconObject.AddComponent<Image>();
        iconImage.preserveAspect = true;
        iconImages[index] = iconImage;

        RectTransform iconRect = iconObject.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.pivot = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = Vector2.zero;
        iconRect.sizeDelta = slotSize * 0.58f;

        Text fallbackText = CreateText(slotObject.transform, "FallbackIcon", 26, FontStyle.Bold, TextAnchor.MiddleCenter);
        fallbackIconTexts[index] = fallbackText;
        RectTransform fallbackRect = fallbackText.GetComponent<RectTransform>();
        fallbackRect.anchorMin = Vector2.zero;
        fallbackRect.anchorMax = Vector2.one;
        fallbackRect.offsetMin = Vector2.zero;
        fallbackRect.offsetMax = Vector2.zero;

        Text quantityText = CreateText(slotObject.transform, "Quantity", 14, FontStyle.Bold, TextAnchor.LowerRight);
        quantityTexts[index] = quantityText;
        RectTransform quantityRect = quantityText.GetComponent<RectTransform>();
        quantityRect.anchorMin = Vector2.zero;
        quantityRect.anchorMax = Vector2.one;
        quantityRect.offsetMin = new Vector2(6f, 4f);
        quantityRect.offsetMax = new Vector2(-7f, -5f);
    }

    private static Text CreateText(Transform parent, string name, int fontSize, FontStyle fontStyle, TextAnchor alignment)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);

        Text text = textObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = alignment;
        text.color = Color.white;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        return text;
    }

    private void RefreshHud()
    {
        if (inventoryCanvas == null || slotBackgrounds == null)
        {
            return;
        }

        for (int i = 0; i < slots.Length; i++)
        {
            InventorySlot slot = slots[i];
            bool selected = i == selectedSlotIndex;

            slotBackgrounds[i].color = selected ? selectedSlotColor : slotColor;

            if (slot.IsEmpty)
            {
                iconImages[i].enabled = false;
                fallbackIconTexts[i].text = string.Empty;
                quantityTexts[i].text = string.Empty;
                continue;
            }

            iconImages[i].enabled = slot.icon != null;
            iconImages[i].sprite = slot.icon;
            iconImages[i].color = slot.color;

            fallbackIconTexts[i].text = slot.icon == null ? GetFallbackGlyph(slot) : string.Empty;
            fallbackIconTexts[i].color = slot.color;
            quantityTexts[i].text = slot.quantity > 1 ? slot.quantity.ToString() : string.Empty;
        }

        if (selectedItemLabel != null)
        {
            InventorySlot selectedSlot = slots[selectedSlotIndex];
            selectedItemLabel.text = selectedSlot.IsEmpty ? string.Empty : selectedSlot.displayName;
        }
    }

    private static string GetFallbackGlyph(InventorySlot slot)
    {
        if (slot.itemId == "compass")
        {
            return "N";
        }

        return string.IsNullOrWhiteSpace(slot.displayName)
            ? "?"
            : slot.displayName.Substring(0, 1).ToUpperInvariant();
    }

    private void EnsureDialogueRunner()
    {
        if (dialogueRunner == null)
        {
            dialogueRunner = FindAnyObjectByType<DialogueRunner>(FindObjectsInactive.Include);
        }
    }

    private void RefreshDialogueVisibility()
    {
        if (!hideDuringDialogue || inventoryCanvas == null)
        {
            return;
        }

        EnsureDialogueRunner();

        bool shouldBeVisible = dialogueRunner == null || !dialogueRunner.IsDialogueRunning;
        if (inventoryVisible == shouldBeVisible)
        {
            return;
        }

        inventoryVisible = shouldBeVisible;
        inventoryCanvas.gameObject.SetActive(shouldBeVisible);
    }

    private bool IsDialogueRunning()
    {
        EnsureDialogueRunner();
        return dialogueRunner != null && dialogueRunner.IsDialogueRunning;
    }

    private void HandleNumberSelection()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        Key[] numberKeys =
        {
            Key.Digit1,
            Key.Digit2,
            Key.Digit3,
            Key.Digit4,
            Key.Digit5,
            Key.Digit6,
            Key.Digit7,
            Key.Digit8,
            Key.Digit9
        };

        for (int i = 0; i < slots.Length && i < numberKeys.Length; i++)
        {
            if (keyboard[numberKeys[i]].wasPressedThisFrame)
            {
                SelectSlot(i);
                return;
            }
        }
    }

    private void HandleMouseWheelSelection()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null)
        {
            return;
        }

        float scrollY = mouse.scroll.ReadValue().y;
        if (Mathf.Abs(scrollY) < 0.01f)
        {
            return;
        }

        int direction = scrollY > 0f ? -1 : 1;
        int nextSlot = (selectedSlotIndex + direction + slots.Length) % slots.Length;
        SelectSlot(nextSlot);
    }
}
