using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class YarnUiKenneySkin
{
    private static readonly Dictionary<string, Sprite> SpriteCache = new Dictionary<string, Sprite>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterSceneLoadedCallback()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!GameSceneNames.IsMainGame(scene.name))
        {
            return;
        }

        ApplySkin();
    }

    private static void ApplySkin()
    {
        Sprite dialogueSprite = LoadSprite("button_rectangle_depth_gradient", new Vector4(22f, 22f, 22f, 22f));
        Sprite buttonSprite = LoadSprite("button_rectangle_depth_flat", new Vector4(20f, 20f, 20f, 20f));
        Sprite buttonBorderSprite = LoadSprite("button_rectangle_depth_border", new Vector4(20f, 20f, 20f, 20f));

        Transform linePresenter = FindTransform("Line Presenter");
        if (linePresenter != null)
        {
            SkinPanel(linePresenter, dialogueSprite, new Color(0.03f, 0.08f, 0.14f, 0.94f));
            ResizeBottomPanel(linePresenter, 64f, 42f, 420f, 172f);
            // Corner icon (star) removed to avoid decorative overlap with dialog content.
        }

        Transform optionsPresenter = FindTransform("Options Presenter");
        if (optionsPresenter != null)
        {
            SkinPanel(optionsPresenter, dialogueSprite, new Color(0.02f, 0.07f, 0.12f, 0.93f));
            ResizeBottomPanel(optionsPresenter, 80f, 58f, 420f, 280f);
            EnsureOptionsLayout(optionsPresenter);
        }

        foreach (GameObject background in FindAllByName("Background"))
        {
            Image image = background.GetComponent<Image>();
            if (image == null)
            {
                continue;
            }

            image.sprite = dialogueSprite;
            image.type = Image.Type.Sliced;
            image.color = new Color(0.03f, 0.08f, 0.14f, 0.94f);
            image.raycastTarget = false;
        }

        SkinContinueButton(buttonSprite, buttonBorderSprite);
        SkinOptionButtons(buttonSprite, buttonBorderSprite);
        SkinDialogueText();
    }

    private static void SkinPanel(Transform panel, Sprite sprite, Color color)
    {
        Image image = panel.GetComponent<Image>();
        if (image == null)
        {
            image = panel.gameObject.AddComponent<Image>();
            image.raycastTarget = false;
        }

        image.sprite = sprite;
        image.type = Image.Type.Sliced;
        image.color = color;
    }

    private static void ResizeBottomPanel(Transform panel, float horizontalMargin, float bottomOffset, float maxWidthTrim, float height)
    {
        RectTransform rect = panel as RectTransform;
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, bottomOffset);
        rect.sizeDelta = new Vector2(-maxWidthTrim + horizontalMargin, height);
    }

    private static void AddCornerIcon(Transform panel, Sprite iconSprite)
    {
        if (iconSprite == null || panel.Find("KenneySkin_CornerIcon") != null)
        {
            return;
        }

        GameObject iconObject = new GameObject("KenneySkin_CornerIcon");
        iconObject.transform.SetParent(panel, false);

        Image image = iconObject.AddComponent<Image>();
        image.sprite = iconSprite;
        image.color = new Color(1f, 0.85f, 0.36f, 0.95f);
        image.raycastTarget = false;

        RectTransform rect = image.rectTransform;
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-18f, -18f);
        rect.sizeDelta = new Vector2(34f, 34f);
    }

    private static void SkinContinueButton(Sprite normalSprite, Sprite highlightedSprite)
    {
        Transform continueButton = FindTransform("Continue Button");
        if (continueButton == null)
        {
            return;
        }

        Image image = continueButton.GetComponent<Image>();
        if (image != null)
        {
            image.sprite = normalSprite;
            image.type = Image.Type.Sliced;
            image.color = new Color(0.12f, 0.35f, 0.55f, 1f);
        }

        Button button = continueButton.GetComponent<Button>();
        if (button != null)
        {
            button.targetGraphic = image;
            button.transition = Selectable.Transition.SpriteSwap;
            button.spriteState = new SpriteState
            {
                highlightedSprite = highlightedSprite,
                selectedSprite = highlightedSprite,
                pressedSprite = highlightedSprite,
                disabledSprite = normalSprite
            };
        }
    }

    private static void SkinOptionButtons(Sprite normalSprite, Sprite highlightedSprite)
    {
        foreach (Button button in Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (button == null || button.gameObject.name == "Continue Button")
            {
                continue;
            }

            Image image = button.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = normalSprite;
                image.type = Image.Type.Sliced;
                image.color = new Color(0.1f, 0.28f, 0.46f, 1f);
            }

            button.targetGraphic = image;
            button.transition = Selectable.Transition.SpriteSwap;
            button.spriteState = new SpriteState
            {
                highlightedSprite = highlightedSprite,
                selectedSprite = highlightedSprite,
                pressedSprite = highlightedSprite,
                disabledSprite = normalSprite
            };

            TMP_Text[] labels = button.GetComponentsInChildren<TMP_Text>(true);
            foreach (TMP_Text label in labels)
            {
                label.color = Color.white;
                label.fontSize = Mathf.Max(label.fontSize, 24f);
                label.enableWordWrapping = true;
            }
        }
    }

    private static void EnsureOptionsLayout(Transform optionsPresenter)
    {
        VerticalLayoutGroup layout = optionsPresenter.GetComponentInChildren<VerticalLayoutGroup>(true);
        if (layout != null)
        {
            layout.spacing = Mathf.Max(layout.spacing, 12f);
            layout.padding.top = Mathf.Max(layout.padding.top, 18);
            layout.padding.bottom = Mathf.Max(layout.padding.bottom, 18);
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
        }

        ContentSizeFitter fitter = optionsPresenter.GetComponentInChildren<ContentSizeFitter>(true);
        if (fitter != null)
        {
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        foreach (Button button in optionsPresenter.GetComponentsInChildren<Button>(true))
        {
            RectTransform rect = button.transform as RectTransform;
            if (rect != null)
            {
                rect.sizeDelta = new Vector2(rect.sizeDelta.x, Mathf.Max(rect.sizeDelta.y, 52f));
            }

            LayoutElement layoutElement = button.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = button.gameObject.AddComponent<LayoutElement>();
            }

            layoutElement.minHeight = Mathf.Max(layoutElement.minHeight, 52f);
            layoutElement.preferredHeight = Mathf.Max(layoutElement.preferredHeight, 58f);
        }
    }

    private static void SkinDialogueText()
    {
        foreach (TMP_Text text in Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (text == null)
            {
                continue;
            }

            if (text.gameObject.name == "Character Name")
            {
                text.color = new Color(1f, 0.86f, 0.42f, 1f);
                text.fontStyle = FontStyles.Bold;
                text.fontSize = 30f;
                continue;
            }

            if (text.gameObject.name == "Last Line" || text.transform.IsChildOfName("Line Presenter"))
            {
                text.color = new Color(0.94f, 0.98f, 1f, 1f);
                text.fontSize = Mathf.Max(text.fontSize, 27f);
                text.lineSpacing = 8f;
            }
        }
    }

    private static Sprite LoadSprite(string resourceName, Vector4 border)
    {
        if (SpriteCache.TryGetValue(resourceName, out Sprite cached))
        {
            return cached;
        }

        Texture2D texture = Resources.Load<Texture2D>($"KenneyUi/{resourceName}");
        if (texture == null)
        {
            return null;
        }

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect,
            border);

        SpriteCache[resourceName] = sprite;
        return sprite;
    }

    private static Transform FindTransform(string objectName)
    {
        foreach (Transform transform in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (transform.name == objectName)
            {
                return transform;
            }
        }

        return null;
    }

    private static IEnumerable<GameObject> FindAllByName(string objectName)
    {
        foreach (Transform transform in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (transform.name == objectName)
            {
                yield return transform.gameObject;
            }
        }
    }

    private static bool IsChildOfName(this Transform transform, string parentName)
    {
        Transform current = transform.parent;
        while (current != null)
        {
            if (current.name == parentName)
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }
}
