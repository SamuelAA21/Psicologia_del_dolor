using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn.Unity;

public static class Chapter1RuntimePlaceholderSpawner
{
    private const string RootName = "Chapter1_Placeholders";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterSceneLoadedCallback()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!GameSceneNames.IsMainGame(scene.name) || GameObject.Find(RootName) != null)
        {
            return;
        }

        BuildPlaceholders();
    }

    private static void BuildPlaceholders()
    {
        DialogueRunner dialogueRunner = Object.FindAnyObjectByType<DialogueRunner>();
        PlayerController player = Object.FindAnyObjectByType<PlayerController>();

        Transform playerTransform = player != null ? player.transform : null;
        Vector3 origin = playerTransform != null ? playerTransform.position : Vector3.zero;
        Vector3 forward = playerTransform != null ? Vector3.ProjectOnPlane(playerTransform.forward, Vector3.up).normalized : Vector3.forward;
        if (forward.sqrMagnitude < 0.01f)
        {
            forward = Vector3.forward;
        }

        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
        Quaternion facingPlayer = Quaternion.LookRotation(-forward, Vector3.up);

        GameObject root = new GameObject(RootName);
        Chapter1EnvironmentController environment = root.AddComponent<Chapter1EnvironmentController>();
        Chapter1GuidanceController guidance = root.GetComponent<Chapter1GuidanceController>();
        DialoguePlayerControlLock controlLock = root.AddComponent<DialoguePlayerControlLock>();
        controlLock.Configure(dialogueRunner, player);

        DoorSetup door1 = CreateDoor(root.transform, "Puerta1_Retirada", "Puerta 1", "Retirada", "Puerta1", origin + forward * 8f - right * 3f, facingPlayer, dialogueRunner, false);
        DoorSetup door2 = CreateDoor(root.transform, "Puerta2_Entender", "Puerta 2", "Entender", "Puerta2", origin + forward * 8f, facingPlayer, dialogueRunner, true);
        DoorSetup door3 = CreateDoor(root.transform, "Puerta3_Compromiso", "Puerta 3", "Compromiso", "Puerta3", origin + forward * 8f + right * 3f, facingPlayer, dialogueRunner, true);
        PanelSetup panel = CreateQuestionPanel(root.transform, origin + forward * 13f, facingPlayer, dialogueRunner);
        GameObject reward = CreateReward(root.transform, origin + forward * 13f + right * 2.8f + Vector3.up * 1.2f);

        GameObject marker1 = CreateMarker(root.transform, "Progreso_1_Retirada", door1.Visual.transform.position + Vector3.up * 2.25f);
        GameObject marker2 = CreateMarker(root.transform, "Progreso_2_Entender", door2.Visual.transform.position + Vector3.up * 2.25f);
        GameObject marker3 = CreateMarker(root.transform, "Progreso_3_Compromiso", door3.Visual.transform.position + Vector3.up * 2.25f);
        GameObject marker4 = CreateMarker(root.transform, "Progreso_4_Preguntas", panel.Visual.transform.position + Vector3.up * 2f);
        GameObject marker5 = CreateMarker(root.transform, "Progreso_5_Brujula", reward.transform.position + Vector3.up * 0.75f);

        environment.ConfigurePlaceholders(
            door1.Visual,
            door2.Visual,
            door3.Visual,
            panel.Visual,
            reward,
            door1.Interactable,
            door2.Interactable,
            door3.Interactable,
            panel.Interactable,
            door1.Animation,
            door2.Animation,
            door3.Animation,
            reward.GetComponent<RewardVisualController>(),
            guidance,
            new[] { marker1, marker2, marker3, marker4, marker5 },
            new[] { door1.Light, door2.Light, door3.Light, panel.Light, reward.GetComponentInChildren<Light>(true) });

        environment.UnlockStage("Puerta1");
    }

    private static DoorSetup CreateDoor(Transform parent, string name, string title, string subtitle, string yarnNode, Vector3 position, Quaternion rotation, DialogueRunner dialogueRunner, bool locked)
    {
        GameObject visual = new GameObject(name);
        visual.name = name;
        visual.transform.SetParent(parent);
        visual.transform.SetPositionAndRotation(position + Vector3.up * 1.15f, rotation);

        BoxCollider solidCollider = visual.AddComponent<BoxCollider>();
        solidCollider.center = new Vector3(0f, 0.25f, 0f);
        solidCollider.size = new Vector3(1.4f, 2.35f, 0.45f);

        bool hasModel = TryAddModelChild(visual.transform, "DoorwayModel", "wall-doorway", new Vector3(0f, -1.15f, 0f), Vector3.one * 0.82f, new Color(0.58f, 0.72f, 0.88f));
        hasModel |= TryAddModelChild(visual.transform, "DoorModel", "door-rotate", new Vector3(0f, -1.15f, -0.03f), Vector3.one * 0.82f, new Color(0.16f, 0.2f, 0.25f));
        if (!hasModel)
        {
            GameObject fallbackBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fallbackBody.name = "FallbackDoorBody";
            fallbackBody.transform.SetParent(visual.transform, false);
            fallbackBody.transform.localScale = new Vector3(1.2f, 2.3f, 0.22f);
            Object.Destroy(fallbackBody.GetComponent<Collider>());
            SetMaterial(fallbackBody, new Color(0.18f, 0.21f, 0.26f));
        }

        DoorAnimationBridge animation = visual.AddComponent<DoorAnimationBridge>();
        CreateText(visual.transform, "Title", title, new Vector3(0f, 1.28f, -0.58f), Color.white, 0.026f);
        CreateText(visual.transform, "Subtitle", subtitle, new Vector3(0f, 1.08f, -0.58f), new Color(0.78f, 0.9f, 1f), 0.018f);

        GameObject prompt = CreateText(visual.transform, "Prompt", "E  Abrir", new Vector3(0f, 0.65f, -0.72f), new Color(0.65f, 0.95f, 1f), 0.018f);
        prompt.SetActive(false);

        GameObject lockedPrompt = CreateText(visual.transform, "LockedPrompt", "Bloqueada", new Vector3(0f, 0.65f, -0.72f), new Color(1f, 0.55f, 0.45f), 0.018f);
        lockedPrompt.SetActive(false);

        Light doorLight = CreateLight(visual.transform, "GuidanceLight", new Vector3(0f, 1.35f, -0.5f), new Color(0.35f, 0.75f, 1f), 2.2f, 3f);
        doorLight.enabled = !locked;
        doorLight.gameObject.AddComponent<Chapter1AmbientMotion>().Configure(false, 0f, false, Vector3.zero, false);

        GameObject trigger = new GameObject("InteractionTrigger");
        trigger.transform.SetParent(visual.transform);
        trigger.transform.localPosition = new Vector3(0f, 0f, -0.65f);
        trigger.transform.localRotation = Quaternion.identity;

        BoxCollider collider = trigger.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.size = new Vector3(1.9f, 2.5f, 1.8f);

        NarrativeInteractable interactable = trigger.AddComponent<NarrativeInteractable>();
        interactable.SetYarnNodeName(yarnNode);
        interactable.SetDialogueRunner(dialogueRunner);
        interactable.SetPromptObject(prompt);
        interactable.SetLockedPromptObject(lockedPrompt);
        interactable.SetDoorAnimationBridge(animation);
        interactable.SetPlayerTag(string.Empty);
        interactable.SetLocked(locked);

        return new DoorSetup(visual, interactable, animation, doorLight);
    }

    private static PanelSetup CreateQuestionPanel(Transform parent, Vector3 position, Quaternion rotation, DialogueRunner dialogueRunner)
    {
        GameObject visual = new GameObject("Estacion4_Preguntas");
        visual.name = "Estacion4_Preguntas";
        visual.transform.SetParent(parent);
        visual.transform.SetPositionAndRotation(position + Vector3.up * 1.05f, rotation);

        BoxCollider solidCollider = visual.AddComponent<BoxCollider>();
        solidCollider.size = new Vector3(3.4f, 1.9f, 0.35f);

        bool hasModel = TryAddModelChild(visual.transform, "PanelBaseModel", "indicator-special-area", new Vector3(0f, -0.9f, -0.12f), new Vector3(2.15f, 2.15f, 2.15f), new Color(0.18f, 0.52f, 0.56f));
        hasModel |= TryAddModelChild(visual.transform, "PanelFrameModel", "wall", new Vector3(0f, -0.95f, 0.06f), new Vector3(2.4f, 1.05f, 0.25f), new Color(0.1f, 0.16f, 0.2f));
        if (!hasModel)
        {
            GameObject fallbackPanel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fallbackPanel.name = "FallbackPanelBody";
            fallbackPanel.transform.SetParent(visual.transform, false);
            fallbackPanel.transform.localScale = new Vector3(3.4f, 1.9f, 0.2f);
            Object.Destroy(fallbackPanel.GetComponent<Collider>());
            SetMaterial(fallbackPanel, new Color(0.08f, 0.16f, 0.18f));
        }

        CreateText(visual.transform, "Label", "Archivo de preguntas", new Vector3(0f, 0.62f, -0.6f), new Color(0.75f, 1f, 0.9f), 0.024f);

        GameObject prompt = CreateText(visual.transform, "Prompt", "E  Responder", new Vector3(0f, 0.25f, -0.7f), new Color(0.65f, 1f, 0.8f), 0.018f);
        prompt.SetActive(false);

        GameObject lockedPrompt = CreateText(visual.transform, "LockedPrompt", "Completa las puertas", new Vector3(0f, 0.25f, -0.7f), new Color(1f, 0.55f, 0.45f), 0.018f);
        lockedPrompt.SetActive(false);

        Light panelLight = CreateLight(visual.transform, "GuidanceLight", new Vector3(0f, 0.9f, -0.75f), new Color(0.45f, 1f, 0.75f), 2.8f, 3.2f);
        panelLight.enabled = false;
        panelLight.gameObject.AddComponent<Chapter1AmbientMotion>().Configure(false, 0f, false, Vector3.zero, false);

        GameObject trigger = new GameObject("InteractionTrigger");
        trigger.transform.SetParent(visual.transform);
        trigger.transform.localPosition = new Vector3(0f, 0f, -0.75f);
        trigger.transform.localRotation = Quaternion.identity;

        BoxCollider collider = trigger.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.size = new Vector3(4f, 2.2f, 1.8f);

        NarrativeInteractable interactable = trigger.AddComponent<NarrativeInteractable>();
        interactable.SetYarnNodeName("Estacion4");
        interactable.SetDialogueRunner(dialogueRunner);
        interactable.SetPromptObject(prompt);
        interactable.SetLockedPromptObject(lockedPrompt);
        interactable.SetPlayerTag(string.Empty);
        interactable.SetLocked(true);

        return new PanelSetup(visual, interactable, panelLight);
    }

    private static GameObject CreateReward(Transform parent, Vector3 position)
    {
        GameObject reward = new GameObject("BrujulaDelCompromiso");
        reward.name = "BrujulaDelCompromiso";
        reward.transform.SetParent(parent);
        reward.transform.position = position;

        bool hasModel = TryAddModelChild(reward.transform, "CompassPlaceholderModel", "coin", Vector3.zero, Vector3.one * 0.9f, new Color(1f, 0.82f, 0.25f));
        if (!hasModel)
        {
            GameObject fallbackReward = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            fallbackReward.name = "FallbackRewardBody";
            fallbackReward.transform.SetParent(reward.transform, false);
            fallbackReward.transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);
            Object.Destroy(fallbackReward.GetComponent<Collider>());
            SetMaterial(fallbackReward, new Color(1f, 0.82f, 0.25f));
        }

        RewardVisualController rewardVisual = reward.AddComponent<RewardVisualController>();
        rewardVisual.Configure(reward);
        reward.AddComponent<Chapter1AmbientMotion>().Configure(true, 0.16f, true, new Vector3(0f, 45f, 0f), true);

        Light rewardLight = CreateLight(reward.transform, "RewardLight", Vector3.up * 0.4f, new Color(1f, 0.85f, 0.35f), 3f, 3f);
        rewardLight.enabled = false;

        reward.SetActive(false);
        return reward;
    }

    private static GameObject CreateMarker(Transform parent, string name, Vector3 position)
    {
        GameObject marker = new GameObject(name);
        marker.name = name;
        marker.transform.SetParent(parent);
        marker.transform.position = position;

        bool hasModel = TryAddModelChild(marker.transform, "MarkerModel", "indicator-special-arrow", Vector3.zero, Vector3.one * 0.38f, new Color(0.35f, 0.9f, 1f));
        if (!hasModel)
        {
            GameObject fallbackMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            fallbackMarker.name = "FallbackMarkerBody";
            fallbackMarker.transform.SetParent(marker.transform, false);
            fallbackMarker.transform.localScale = Vector3.one * 0.25f;
            Object.Destroy(fallbackMarker.GetComponent<Collider>());
            SetMaterial(fallbackMarker, new Color(0.45f, 0.9f, 1f));
        }

        marker.AddComponent<Chapter1AmbientMotion>().Configure(true, 0.1f, true, new Vector3(0f, 70f, 0f), true);
        marker.SetActive(false);
        return marker;
    }

    private static GameObject CreateText(Transform parent, string name, string text, Vector3 localPosition, Color color, float characterSize)
    {
        GameObject target = new GameObject(name);
        target.transform.SetParent(parent);
        target.transform.localPosition = localPosition;
        target.transform.localRotation = Quaternion.identity;

        TextMesh textMesh = target.AddComponent<TextMesh>();
        textMesh.text = text;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.characterSize = characterSize;
        textMesh.fontSize = 36;
        textMesh.color = color;
        target.AddComponent<Chapter1Billboard>();
        return target;
    }

    private static Light CreateLight(Transform parent, string name, Vector3 localPosition, Color color, float intensity, float range)
    {
        GameObject target = new GameObject(name);
        target.transform.SetParent(parent);
        target.transform.localPosition = localPosition;
        target.transform.localRotation = Quaternion.identity;

        Light light = target.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        return light;
    }

    private static void SetMaterial(GameObject target, Color color)
    {
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer == null)
        {
            return;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material material = new Material(shader);
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }
        else if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }

        renderer.material = material;
    }

    private static bool TryAddModelChild(Transform parent, string childName, string resourceName, Vector3 localPosition, Vector3 localScale, Color tint)
    {
        GameObject prefab = Resources.Load<GameObject>($"KenneyPrototypeKit/Models/{resourceName}");
        if (prefab == null)
        {
            return false;
        }

        GameObject instance = Object.Instantiate(prefab, parent);
        instance.name = childName;
        instance.transform.localPosition = localPosition;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = localScale;
        TintRenderers(instance, tint);
        return true;
    }

    private static void TintRenderers(GameObject root, Color color)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in renderers)
        {
            if (renderer == null || renderer.sharedMaterial == null)
            {
                continue;
            }

            Material material = new Material(renderer.sharedMaterial);
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }
            else if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }

            renderer.material = material;
        }
    }

    private readonly struct DoorSetup
    {
        public DoorSetup(GameObject visual, NarrativeInteractable interactable, DoorAnimationBridge animation, Light light)
        {
            Visual = visual;
            Interactable = interactable;
            Animation = animation;
            Light = light;
        }

        public GameObject Visual { get; }
        public NarrativeInteractable Interactable { get; }
        public DoorAnimationBridge Animation { get; }
        public Light Light { get; }
    }

    private readonly struct PanelSetup
    {
        public PanelSetup(GameObject visual, NarrativeInteractable interactable, Light light)
        {
            Visual = visual;
            Interactable = interactable;
            Light = light;
        }

        public GameObject Visual { get; }
        public NarrativeInteractable Interactable { get; }
        public Light Light { get; }
    }
}
