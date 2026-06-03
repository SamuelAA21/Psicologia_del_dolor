using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class JetpackUiPrefabCreator
{
    private const string ResourcesFolder = "Assets/Resources";
    private const string UiFolder = "Assets/Resources/UI";
    private const string OverlayPrefabPath = "Assets/Resources/UI/JetpackPreGameOverlay.prefab";
    private const string HudPrefabPath = "Assets/Resources/UI/JetpackTherapyHud.prefab";
    private const string CompletionPrefabPath = "Assets/Resources/UI/JetpackCompletionOverlay.prefab";

    [InitializeOnLoadMethod]
    private static void CreateMissingPrefabsOnLoad()
    {
        EditorApplication.delayCall += () =>
        {
            EnsureFolders();

            bool changed = false;
            if (!AssetDatabase.LoadAssetAtPath<GameObject>(OverlayPrefabPath))
            {
                SaveOverlayPrefab();
                changed = true;
            }

            if (!AssetDatabase.LoadAssetAtPath<GameObject>(HudPrefabPath))
            {
                SaveHudPrefab();
                changed = true;
            }

            if (!AssetDatabase.LoadAssetAtPath<GameObject>(CompletionPrefabPath))
            {
                SaveCompletionPrefab();
                changed = true;
            }

            if (changed)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        };
    }

    [MenuItem("Tools/Psicologia del Dolor/Regenerar UI Jetpack editable")]
    public static void RegeneratePrefabs()
    {
        EnsureFolders();
        SaveOverlayPrefab();
        SaveHudPrefab();
        SaveCompletionPrefab();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder(ResourcesFolder))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        if (!AssetDatabase.IsValidFolder(UiFolder))
        {
            AssetDatabase.CreateFolder(ResourcesFolder, "UI");
        }
    }

    private static void SaveOverlayPrefab()
    {
        GameObject root = new GameObject("JetpackPreGameOverlay");
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1200;

        CanvasScaler scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        root.AddComponent<GraphicRaycaster>();

        Font font = GetDefaultFont();

        Image dimmer = CreateImage("SoftBlurOverlay", root.transform, new Color(0.64f, 0.83f, 0.9f, 0.38f));
        Stretch(dimmer.rectTransform);

        Image shade = CreateImage("DepthShade", root.transform, new Color(0f, 0.05f, 0.08f, 0.42f));
        Stretch(shade.rectTransform);

        Image panel = CreateImage("InstructionPanel", root.transform, new Color(0.02f, 0.05f, 0.07f, 0.82f));
        RectTransform panelRect = panel.rectTransform;
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(620f, 390f);

        Text title = CreateText("Title", panelRect, font, 34, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
        title.text = "RESPIRACION GUIADA";
        SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -34f), new Vector2(540f, 48f));

        Text body = CreateText("Body", panelRect, font, 21, FontStyle.Normal, TextAnchor.UpperLeft, new Color(0.9f, 0.98f, 1f, 1f));
        body.text =
            "Inhala: manten Espacio para subir.\n" +
            "Sosten: pulsa y suelta suave para mantener altura.\n" +
            "Exhala: suelta Espacio para bajar.\n\n" +
            "Sigue la franja y respira con ritmo, no con prisa.";
        SetRect(body.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -104f), new Vector2(500f, 155f));

        Image buttonImage = CreateImage("PlayButton", panelRect, new Color(0.05f, 0.62f, 0.72f, 0.96f));
        SetRect(buttonImage.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 42f), new Vector2(250f, 62f));
        Button button = buttonImage.gameObject.AddComponent<Button>();
        button.targetGraphic = buttonImage;

        Text label = CreateText("Label", buttonImage.transform, font, 25, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
        label.text = "PLAY";
        Stretch(label.rectTransform);

        PrefabUtility.SaveAsPrefabAsset(root, OverlayPrefabPath);
        Object.DestroyImmediate(root);
    }

    private static void SaveHudPrefab()
    {
        GameObject root = new GameObject("JetpackTherapyHud");
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        CanvasScaler scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        root.AddComponent<GraphicRaycaster>();

        Font font = GetDefaultFont();

        Image panel = CreateImage("Panel", root.transform, new Color(0.03f, 0.05f, 0.07f, 0.48f));
        RectTransform panelRect = panel.rectTransform;
        SetRect(panelRect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -18f), new Vector2(430f, 104f));

        Text phase = CreateText("Phase", panelRect, font, 22, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white);
        phase.text = "INHALA";
        SetRect(phase.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -10f), new Vector2(170f, 28f));

        Text instruction = CreateText("Instruction", panelRect, font, 15, FontStyle.Normal, TextAnchor.MiddleLeft, new Color(0.93f, 0.97f, 1f));
        instruction.text = "Manten Espacio: sube lento.";
        SetRect(instruction.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -38f), new Vector2(265f, 24f));

        Image barBack = CreateImage("PhaseProgressBack", panelRect, new Color(1f, 1f, 1f, 0.18f));
        SetRect(barBack.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 12f), new Vector2(-32f, 8f));

        Image fill = CreateImage("PhaseProgressFill", barBack.transform, new Color(0.38f, 0.84f, 1f, 0.95f));
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = 0;
        Stretch(fill.rectTransform);

        Text cycle = CreateText("Cycle", panelRect, font, 14, FontStyle.Bold, TextAnchor.MiddleRight, Color.white);
        cycle.text = "Ciclo 1/4";
        SetRect(cycle.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-16f, -12f), new Vector2(120f, 22f));

        Text score = CreateText("Score", panelRect, font, 12, FontStyle.Normal, TextAnchor.MiddleRight, new Color(0.78f, 0.9f, 1f));
        score.text = "Ritmo 0%";
        SetRect(score.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-16f, -34f), new Vector2(120f, 18f));

        Text lives = CreateText("Lives", panelRect, font, 12, FontStyle.Bold, TextAnchor.MiddleRight, new Color(1f, 0.74f, 0.58f));
        lives.text = "Intentos 3/3";
        SetRect(lives.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-16f, -56f), new Vector2(120f, 18f));

        Image statusDot = CreateImage("StatusDot", panelRect, Color.white);
        SetRect(statusDot.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-121f, 22f), new Vector2(8f, 8f));

        Text status = CreateText("Status", panelRect, font, 12, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white);
        status.text = "Sigue la franja";
        SetRect(status.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-16f, 14f), new Vector2(105f, 18f));

        PrefabUtility.SaveAsPrefabAsset(root, HudPrefabPath);
        Object.DestroyImmediate(root);
    }

    private static void SaveCompletionPrefab()
    {
        GameObject root = new GameObject("JetpackCompletionOverlay");
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1210;

        CanvasScaler scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        root.AddComponent<GraphicRaycaster>();

        Font font = GetDefaultFont();

        Image shade = CreateImage("CompletionShade", root.transform, new Color(0f, 0.05f, 0.08f, 0.62f));
        Stretch(shade.rectTransform);

        Image panel = CreateImage("CompletionPanel", root.transform, new Color(0.02f, 0.05f, 0.07f, 0.9f));
        RectTransform panelRect = panel.rectTransform;
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(720f, 470f);

        Text title = CreateText("Title", panelRect, font, 34, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
        title.text = "PRACTICA COMPLETADA";
        SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -34f), new Vector2(620f, 48f));

        Text summary = CreateText("Summary", panelRect, font, 20, FontStyle.Normal, TextAnchor.UpperLeft, new Color(0.92f, 0.98f, 1f));
        summary.text =
            "Completaste los 4 ciclos de respiracion.\n\n" +
            "La practica no busca velocidad: busca notar el ritmo, subir con la inhalacion, sostener con calma y bajar con la exhalacion.";
        SetRect(summary.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -100f), new Vector2(600f, 135f));

        Text rewards = CreateText("Rewards", panelRect, font, 19, FontStyle.Bold, TextAnchor.UpperLeft, new Color(1f, 0.86f, 0.52f));
        rewards.text =
            "Objetos y avances conseguidos:\n" +
            "- Respiracion consciente completada\n" +
            "- Progreso del compromiso registrado\n" +
            "- Camino de regreso a la Brujula desbloqueado";
        SetRect(rewards.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -245f), new Vector2(600f, 100f));

        Image buttonImage = CreateImage("ContinueButton", panelRect, new Color(0.05f, 0.62f, 0.72f, 0.96f));
        SetRect(buttonImage.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 38f), new Vector2(270f, 58f));
        Button button = buttonImage.gameObject.AddComponent<Button>();
        button.targetGraphic = buttonImage;

        Text label = CreateText("Label", buttonImage.transform, font, 23, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
        label.text = "CONTINUAR";
        Stretch(label.rectTransform);

        PrefabUtility.SaveAsPrefabAsset(root, CompletionPrefabPath);
        Object.DestroyImmediate(root);
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

    private static Font GetDefaultFont()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return font != null ? font : Resources.GetBuiltinResource<Font>("Arial.ttf");
    }
}
