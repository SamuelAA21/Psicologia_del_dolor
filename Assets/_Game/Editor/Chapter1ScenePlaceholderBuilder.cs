using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Yarn.Unity;

public static class Chapter1ScenePlaceholderBuilder
{
    private const string SampleScenePath = "Assets/_Game/Scenes/SampleScene.unity";
    private const string RootName = "Chapter1_Placeholders";
    private const string MaterialsFolder = "Assets/_Game/Art/Chapter1Placeholders/Materials";

    [MenuItem("Tools/Chapter 1/Build SampleScene Placeholders")]
    public static void BuildSampleScenePlaceholders()
    {
        if (EditorSceneManager.GetActiveScene().path != SampleScenePath)
        {
            EditorSceneManager.OpenScene(SampleScenePath);
        }

        DialogueRunner dialogueRunner = Object.FindAnyObjectByType<DialogueRunner>(FindObjectsInactive.Include);
        PlayerController player = Object.FindAnyObjectByType<PlayerController>(FindObjectsInactive.Include);

        Transform playerTransform = player != null ? player.transform : null;
        Vector3 origin = playerTransform != null ? playerTransform.position : Vector3.zero;
        Vector3 forward = playerTransform != null ? Vector3.ProjectOnPlane(playerTransform.forward, Vector3.up).normalized : Vector3.forward;
        if (forward.sqrMagnitude < 0.01f)
        {
            forward = Vector3.forward;
        }

        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
        Quaternion facingPlayer = Quaternion.LookRotation(-forward, Vector3.up);

        GameObject root = GetOrCreateRoot();
        Chapter1EnvironmentController environment = AddOrGet<Chapter1EnvironmentController>(root);
        Chapter1GuidanceController guidance = AddOrGet<Chapter1GuidanceController>(root);
        DialoguePlayerControlLock controlLock = AddOrGet<DialoguePlayerControlLock>(root);

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

        AssignObject(environment, "puerta1Retirada", door1.Visual);
        AssignObject(environment, "puerta2Entender", door2.Visual);
        AssignObject(environment, "puerta3Compromiso", door3.Visual);
        AssignObject(environment, "estacion4Preguntas", panel.Visual);
        AssignObject(environment, "brujulaDelCompromiso", reward);
        AssignObject(environment, "puerta1Interactable", door1.Interactable);
        AssignObject(environment, "puerta2Interactable", door2.Interactable);
        AssignObject(environment, "puerta3Interactable", door3.Interactable);
        AssignObject(environment, "estacion4Interactable", panel.Interactable);
        AssignObject(environment, "puerta1Animation", door1.Animation);
        AssignObject(environment, "puerta2Animation", door2.Animation);
        AssignObject(environment, "puerta3Animation", door3.Animation);
        AssignObject(environment, "rewardVisual", reward.GetComponent<RewardVisualController>());
        AssignObject(environment, "guidanceController", guidance);
        AssignArray(environment, "progressMarkers", marker1, marker2, marker3, marker4, marker5);
        AssignArray(environment, "ambientLights", door1.Light, door2.Light, door3.Light, panel.Light, reward.GetComponentInChildren<Light>(true));

        AssignObject(controlLock, "dialogueRunner", dialogueRunner);
        AssignObject(controlLock, "playerController", player);

        environment.UnlockStage("Puerta1");
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();

        Debug.Log("Chapter 1 placeholders were created or updated in SampleScene.");
    }

    private static DoorSetup CreateDoor(Transform parent, string name, string label, string yarnNode, Vector3 position, Quaternion rotation, DialogueRunner dialogueRunner, bool locked)
    {
        GameObject visual = GetOrCreatePrimitive(parent, name, PrimitiveType.Cube);
        visual.transform.SetPositionAndRotation(position + Vector3.up * 1.5f, rotation);
        visual.transform.localScale = new Vector3(1.5f, 3f, 0.25f);
        SetMaterial(visual, "DoorPlaceholder", new Color(0.18f, 0.21f, 0.26f));

        DoorAnimationBridge animation = AddOrGet<DoorAnimationBridge>(visual);

        GameObject labelObject = GetOrCreateText(visual.transform, "Label", label, new Vector3(0f, 0.35f, -0.65f), Color.white);
        labelObject.transform.localScale = Vector3.one;

        GameObject prompt = GetOrCreateText(visual.transform, "Prompt", "Presiona E para abrir", new Vector3(0f, 1.45f, -0.85f), new Color(0.65f, 0.9f, 1f));
        prompt.SetActive(false);

        GameObject lockedPrompt = GetOrCreateText(visual.transform, "LockedPrompt", "Bloqueada", new Vector3(0f, 1.45f, -0.85f), new Color(1f, 0.55f, 0.45f));
        lockedPrompt.SetActive(false);

        Light doorLight = GetOrCreateLight(visual.transform, "GuidanceLight", new Vector3(0f, 1.7f, -0.8f), new Color(0.35f, 0.75f, 1f), 3.5f, 4f);
        doorLight.enabled = !locked;
        AddOrGet<Chapter1AmbientMotion>(doorLight.gameObject).Configure(false, 0f, false, Vector3.zero, false);

        GameObject trigger = GetOrCreateChild(visual.transform, "InteractionTrigger");
        trigger.transform.localPosition = new Vector3(0f, 0f, -0.8f);
        trigger.transform.localRotation = Quaternion.identity;
        trigger.transform.localScale = Vector3.one;

        BoxCollider collider = AddOrGet<BoxCollider>(trigger);
        collider.isTrigger = true;
        collider.size = new Vector3(2.3f, 2.7f, 2.2f);

        NarrativeInteractable interactable = AddOrGet<NarrativeInteractable>(trigger);
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
        GameObject visual = GetOrCreatePrimitive(parent, "Estacion4_Preguntas", PrimitiveType.Cube);
        visual.transform.SetPositionAndRotation(position + Vector3.up * 1.35f, rotation);
        visual.transform.localScale = new Vector3(4.2f, 2.4f, 0.2f);
        SetMaterial(visual, "PanelPlaceholder", new Color(0.08f, 0.16f, 0.18f));

        GetOrCreateText(visual.transform, "Label", "Archivo de preguntas\nInteractua para abrir el cuestionario", new Vector3(0f, 0.2f, -0.65f), new Color(0.75f, 1f, 0.9f));

        GameObject prompt = GetOrCreateText(visual.transform, "Prompt", "Presiona E para responder", new Vector3(0f, 1.25f, -0.85f), new Color(0.65f, 1f, 0.8f));
        prompt.SetActive(false);

        GameObject lockedPrompt = GetOrCreateText(visual.transform, "LockedPrompt", "Completa las puertas primero", new Vector3(0f, 1.25f, -0.85f), new Color(1f, 0.55f, 0.45f));
        lockedPrompt.SetActive(false);

        Light panelLight = GetOrCreateLight(visual.transform, "GuidanceLight", new Vector3(0f, 1.3f, -0.85f), new Color(0.45f, 1f, 0.75f), 4.5f, 4f);
        panelLight.enabled = false;
        AddOrGet<Chapter1AmbientMotion>(panelLight.gameObject).Configure(false, 0f, false, Vector3.zero, false);

        GameObject trigger = GetOrCreateChild(visual.transform, "InteractionTrigger");
        trigger.transform.localPosition = new Vector3(0f, 0f, -0.9f);
        trigger.transform.localRotation = Quaternion.identity;
        trigger.transform.localScale = Vector3.one;

        BoxCollider collider = AddOrGet<BoxCollider>(trigger);
        collider.isTrigger = true;
        collider.size = new Vector3(4.8f, 2.7f, 2.2f);

        NarrativeInteractable interactable = AddOrGet<NarrativeInteractable>(trigger);
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
        GameObject reward = GetOrCreatePrimitive(parent, "BrujulaDelCompromiso", PrimitiveType.Sphere);
        reward.transform.position = position;
        reward.transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);
        SetMaterial(reward, "RewardPlaceholder", new Color(1f, 0.82f, 0.25f));

        RewardVisualController rewardVisual = AddOrGet<RewardVisualController>(reward);
        AssignObject(rewardVisual, "rewardObject", reward);
        AddOrGet<Chapter1AmbientMotion>(reward).Configure(true, 0.16f, true, new Vector3(0f, 45f, 0f), true);

        Light rewardLight = GetOrCreateLight(reward.transform, "RewardLight", Vector3.up * 0.4f, new Color(1f, 0.85f, 0.35f), 3f, 3f);
        rewardLight.enabled = false;

        reward.SetActive(false);
        return reward;
    }

    private static GameObject CreateMarker(Transform parent, string name, Vector3 position)
    {
        GameObject marker = GetOrCreatePrimitive(parent, name, PrimitiveType.Sphere);
        marker.transform.position = position;
        marker.transform.localScale = Vector3.one * 0.25f;
        SetMaterial(marker, "ProgressMarker", new Color(0.45f, 0.9f, 1f));
        AddOrGet<Chapter1AmbientMotion>(marker).Configure(true, 0.1f, true, new Vector3(0f, 70f, 0f), true);
        marker.SetActive(false);
        return marker;
    }

    private static GameObject GetOrCreateRoot()
    {
        GameObject root = GameObject.Find(RootName);
        if (root != null)
        {
            return root;
        }

        root = new GameObject(RootName);
        root.transform.position = Vector3.zero;
        return root;
    }

    private static GameObject GetOrCreatePrimitive(Transform parent, string name, PrimitiveType primitiveType)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
        {
            return existing.gameObject;
        }

        GameObject target = GameObject.CreatePrimitive(primitiveType);
        target.name = name;
        target.transform.SetParent(parent);
        return target;
    }

    private static GameObject GetOrCreateChild(Transform parent, string name)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
        {
            return existing.gameObject;
        }

        GameObject target = new GameObject(name);
        target.transform.SetParent(parent);
        return target;
    }

    private static GameObject GetOrCreateText(Transform parent, string name, string text, Vector3 localPosition, Color color)
    {
        GameObject target = GetOrCreateChild(parent, name);
        target.transform.localPosition = localPosition;
        target.transform.localRotation = Quaternion.identity;
        target.transform.localScale = Vector3.one;

        TextMesh textMesh = AddOrGet<TextMesh>(target);
        textMesh.text = text;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.characterSize = 0.08f;
        textMesh.fontSize = 48;
        textMesh.color = color;
        AddOrGet<Chapter1Billboard>(target);

        MeshRenderer renderer = target.GetComponent<MeshRenderer>();
        // Do not force a sorting order here; let Unity determine render order
        // to avoid UI/dialogue overlapping issues.

        return target;
    }

    private static Light GetOrCreateLight(Transform parent, string name, Vector3 localPosition, Color color, float intensity, float range)
    {
        GameObject target = GetOrCreateChild(parent, name);
        target.transform.localPosition = localPosition;
        target.transform.localRotation = Quaternion.identity;

        Light light = AddOrGet<Light>(target);
        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        return light;
    }

    private static T AddOrGet<T>(GameObject target) where T : Component
    {
        T component = target.GetComponent<T>();
        if (component == null)
        {
            component = target.AddComponent<T>();
        }

        return component;
    }

    private static void SetMaterial(GameObject target, string materialName, Color color)
    {
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer == null)
        {
            return;
        }

        renderer.sharedMaterial = GetMaterial(materialName, color);
    }

    private static Material GetMaterial(string name, Color color)
    {
        if (!AssetDatabase.IsValidFolder("Assets/_Game/Art/Chapter1Placeholders"))
        {
            AssetDatabase.CreateFolder("Assets/_Game/Art", "Chapter1Placeholders");
        }

        if (!AssetDatabase.IsValidFolder(MaterialsFolder))
        {
            AssetDatabase.CreateFolder("Assets/_Game/Art/Chapter1Placeholders", "Materials");
        }

        string path = Path.Combine(MaterialsFolder, name + ".mat").Replace("\\", "/");
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }
        else if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }

        EditorUtility.SetDirty(material);
        return material;
    }

    private static void AssignObject(Object target, string propertyName, Object value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void AssignArray(Object target, string propertyName, params Object[] values)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null || !property.isArray)
        {
            return;
        }

        property.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
        {
            property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }

        serializedObject.ApplyModifiedPropertiesWithoutUndo();
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
