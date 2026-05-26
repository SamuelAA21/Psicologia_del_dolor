using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn.Unity;

public static class Chapter1RuntimePlaceholderSpawner
{
    private const string TargetSceneName = "SampleScene";
    private const string RootName = "Chapter1_Placeholders";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterSceneLoadedCallback()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != TargetSceneName || GameObject.Find(RootName) != null)
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
        DialoguePlayerControlLock controlLock = root.AddComponent<DialoguePlayerControlLock>();
        controlLock.Configure(dialogueRunner, player);

        DoorSetup door1 = CreateDoor(root.transform, "Puerta1_Retirada", "Puerta 1\nRetirada", "Puerta1", origin + forward * 8f - right * 3f, facingPlayer, dialogueRunner, false);
        DoorSetup door2 = CreateDoor(root.transform, "Puerta2_Entender", "Puerta 2\nEntender", "Puerta2", origin + forward * 8f, facingPlayer, dialogueRunner, true);
        DoorSetup door3 = CreateDoor(root.transform, "Puerta3_Compromiso", "Puerta 3\nCompromiso", "Puerta3", origin + forward * 8f + right * 3f, facingPlayer, dialogueRunner, true);
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
            new[] { marker1, marker2, marker3, marker4, marker5 },
            new[] { door1.Light, door2.Light, door3.Light, panel.Light, reward.GetComponentInChildren<Light>(true) });

        environment.UnlockStage("Puerta1");
    }

    private static DoorSetup CreateDoor(Transform parent, string name, string label, string yarnNode, Vector3 position, Quaternion rotation, DialogueRunner dialogueRunner, bool locked)
    {
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.name = name;
        visual.transform.SetParent(parent);
        visual.transform.SetPositionAndRotation(position + Vector3.up * 1.5f, rotation);
        visual.transform.localScale = new Vector3(1.5f, 3f, 0.25f);
        SetMaterial(visual, new Color(0.18f, 0.21f, 0.26f));

        DoorAnimationBridge animation = visual.AddComponent<DoorAnimationBridge>();
        CreateText(visual.transform, "Label", label, new Vector3(0f, 0.35f, -0.65f), Color.white);

        GameObject prompt = CreateText(visual.transform, "Prompt", "Presiona E para abrir", new Vector3(0f, 1.45f, -0.85f), new Color(0.65f, 0.9f, 1f));
        prompt.SetActive(false);

        GameObject lockedPrompt = CreateText(visual.transform, "LockedPrompt", "Bloqueada", new Vector3(0f, 1.45f, -0.85f), new Color(1f, 0.55f, 0.45f));
        lockedPrompt.SetActive(false);

        Light doorLight = CreateLight(visual.transform, "GuidanceLight", new Vector3(0f, 1.7f, -0.8f), new Color(0.35f, 0.75f, 1f), 3.5f, 4f);
        doorLight.enabled = !locked;

        GameObject trigger = new GameObject("InteractionTrigger");
        trigger.transform.SetParent(visual.transform);
        trigger.transform.localPosition = new Vector3(0f, 0f, -0.8f);
        trigger.transform.localRotation = Quaternion.identity;

        BoxCollider collider = trigger.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.size = new Vector3(2.3f, 2.7f, 2.2f);

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
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.name = "Estacion4_Preguntas";
        visual.transform.SetParent(parent);
        visual.transform.SetPositionAndRotation(position + Vector3.up * 1.35f, rotation);
        visual.transform.localScale = new Vector3(4.2f, 2.4f, 0.2f);
        SetMaterial(visual, new Color(0.08f, 0.16f, 0.18f));

        CreateText(visual.transform, "Label", "Archivo de preguntas\nInteractua para abrir el cuestionario", new Vector3(0f, 0.2f, -0.65f), new Color(0.75f, 1f, 0.9f));

        GameObject prompt = CreateText(visual.transform, "Prompt", "Presiona E para responder", new Vector3(0f, 1.25f, -0.85f), new Color(0.65f, 1f, 0.8f));
        prompt.SetActive(false);

        GameObject lockedPrompt = CreateText(visual.transform, "LockedPrompt", "Completa las puertas primero", new Vector3(0f, 1.25f, -0.85f), new Color(1f, 0.55f, 0.45f));
        lockedPrompt.SetActive(false);

        Light panelLight = CreateLight(visual.transform, "GuidanceLight", new Vector3(0f, 1.3f, -0.85f), new Color(0.45f, 1f, 0.75f), 4.5f, 4f);
        panelLight.enabled = false;

        GameObject trigger = new GameObject("InteractionTrigger");
        trigger.transform.SetParent(visual.transform);
        trigger.transform.localPosition = new Vector3(0f, 0f, -0.9f);
        trigger.transform.localRotation = Quaternion.identity;

        BoxCollider collider = trigger.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.size = new Vector3(4.8f, 2.7f, 2.2f);

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
        GameObject reward = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        reward.name = "BrujulaDelCompromiso";
        reward.transform.SetParent(parent);
        reward.transform.position = position;
        reward.transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);
        SetMaterial(reward, new Color(1f, 0.82f, 0.25f));

        RewardVisualController rewardVisual = reward.AddComponent<RewardVisualController>();
        rewardVisual.Configure(reward);

        Light rewardLight = CreateLight(reward.transform, "RewardLight", Vector3.up * 0.4f, new Color(1f, 0.85f, 0.35f), 3f, 3f);
        rewardLight.enabled = false;

        reward.SetActive(false);
        return reward;
    }

    private static GameObject CreateMarker(Transform parent, string name, Vector3 position)
    {
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marker.name = name;
        marker.transform.SetParent(parent);
        marker.transform.position = position;
        marker.transform.localScale = Vector3.one * 0.25f;
        SetMaterial(marker, new Color(0.45f, 0.9f, 1f));
        marker.SetActive(false);
        return marker;
    }

    private static GameObject CreateText(Transform parent, string name, string text, Vector3 localPosition, Color color)
    {
        GameObject target = new GameObject(name);
        target.transform.SetParent(parent);
        target.transform.localPosition = localPosition;
        target.transform.localRotation = Quaternion.identity;

        TextMesh textMesh = target.AddComponent<TextMesh>();
        textMesh.text = text;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.characterSize = 0.08f;
        textMesh.fontSize = 48;
        textMesh.color = color;
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
