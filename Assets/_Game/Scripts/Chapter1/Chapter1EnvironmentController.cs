using System;
using UnityEngine;
using Yarn.Unity;

public class Chapter1EnvironmentController : MonoBehaviour
{
    public static Chapter1EnvironmentController Instance { get; private set; }

    private const string DefaultInitialStage = "Puerta1";

    [Header("Personajes")]
    [SerializeField] private GameObject avatarClinico;
    [SerializeField] private CharacterAnimationBridge avatarAnimation;

    [Header("Zonas del capitulo")]
    [SerializeField] private GameObject puerta1Retirada;
    [SerializeField] private GameObject puerta2Entender;
    [SerializeField] private GameObject puerta3Compromiso;
    [SerializeField] private GameObject estacion4Preguntas;
    [SerializeField] private GameObject brujulaDelCompromiso;

    [Header("Interaccion")]
    [SerializeField] private NarrativeInteractable puerta1Interactable;
    [SerializeField] private NarrativeInteractable puerta2Interactable;
    [SerializeField] private NarrativeInteractable puerta3Interactable;
    [SerializeField] private NarrativeInteractable estacion4Interactable;

    [Header("Animacion de puertas")]
    [SerializeField] private DoorAnimationBridge puerta1Animation;
    [SerializeField] private DoorAnimationBridge puerta2Animation;
    [SerializeField] private DoorAnimationBridge puerta3Animation;

    [Header("Recompensa")]
    [SerializeField] private RewardVisualController rewardVisual;

    [Header("Guia del jugador")]
    [SerializeField] private Chapter1GuidanceController guidanceController;

    [Header("Inventario")]
    [SerializeField] private Chapter1InventorySystem inventorySystem;

    [Header("Progreso")]
    [SerializeField] private Chapter1ProgressManager progressManager;

    [Header("Inicializacion automatica")]
    [SerializeField] private bool autoBindSceneObjects = true;
    [SerializeField] private bool autoInitializeOnStart = true;
    [SerializeField] private string initialStageName = DefaultInitialStage;

    [Header("Ambiente opcional")]
    [SerializeField] private GameObject[] progressMarkers;
    [SerializeField] private Light[] ambientLights;
    [SerializeField] private ParticleSystem[] stageParticles;
    [SerializeField] private AudioSource stageAudioSource;

    public string CurrentStage { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"Duplicate {nameof(Chapter1EnvironmentController)} found on {name}. Disabling the latest instance.");
            enabled = false;
            return;
        }

        Instance = this;
        EnsureGuidanceController();
        EnsureInventorySystem();
        EnsureProgressManager();

        if (autoBindSceneObjects)
        {
            AutoBindSceneObjects();
        }
    }

    private void Start()
    {
        if (autoInitializeOnStart && string.IsNullOrWhiteSpace(CurrentStage))
        {
            UnlockStage(string.IsNullOrWhiteSpace(initialStageName) ? DefaultInitialStage : initialStageName);
        }

        if (progressManager != null)
        {
            progressManager.ConsumePendingMiniGameResult();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    [YarnCommand("chapter1_stage")]
    public static void SetStageFromYarn(string stageName)
    {
        if (Instance == null)
        {
            return;
        }

        Instance.SetStage(stageName);
    }

    [YarnCommand("chapter1_reward_unlocked")]
    public static void UnlockRewardFromYarn()
    {
        if (Instance == null)
        {
            return;
        }

        Instance.UnlockReward();
    }

    [YarnCommand("chapter1_unlock")]
    public static void UnlockStageFromYarn(string stageName)
    {
        if (Instance == null)
        {
            return;
        }

        Instance.UnlockStage(stageName);
    }

    [YarnCommand("chapter1_give_compass")]
    public static void GiveCompassFromYarn()
    {
        if (Instance == null)
        {
            return;
        }

        Instance.EnsureInventorySystem();
        Instance.EnsureProgressManager();
        if (Instance.progressManager != null)
        {
            Instance.progressManager.MarkCompassObtained();
        }
        else if (Instance.inventorySystem != null)
        {
            Instance.inventorySystem.AddCompass();
        }
    }

    public void SetStage(string stageName)
    {
        if (string.IsNullOrWhiteSpace(stageName))
        {
            return;
        }

        CurrentStage = stageName;

        if (avatarClinico != null)
        {
            avatarClinico.SetActive(true);
        }

        if (avatarAnimation != null)
        {
            avatarAnimation.PlayTalk();
        }

        HighlightStage(stageName);
        PlayStageFeedback();

        if (stageName.Equals("Intro", StringComparison.OrdinalIgnoreCase))
        {
            UnlockStage("Puerta1");
        }
    }

    public void UnlockReward()
    {
        if (brujulaDelCompromiso != null)
        {
            brujulaDelCompromiso.SetActive(true);
        }

        if (rewardVisual != null)
        {
            rewardVisual.PlayUnlock();
        }

        if (progressManager != null)
        {
            progressManager.MarkCompassObtained();
        }
        else if (inventorySystem != null)
        {
            inventorySystem.AddCompass();
        }

        SetObjective("Recompensa obtenida: prepárate para respirar.", brujulaDelCompromiso);
    }

    public void UnlockStage(string stageName)
    {
        if (string.IsNullOrWhiteSpace(stageName))
        {
            return;
        }

        if (autoBindSceneObjects)
        {
            AutoBindSceneObjects();
        }

        CurrentStage = stageName;

        if (stageName.Equals("Puerta1", StringComparison.OrdinalIgnoreCase))
        {
            SetInteractableLocks(false, true, true, true);
            SetDoorLocked(puerta1Animation, false);
            SetDoorLocked(puerta2Animation, true);
            SetDoorLocked(puerta3Animation, true);
            HighlightStage("Puerta1");
            SetObjective("Acércate a la Puerta 1: Retirada.", puerta1Retirada);
            return;
        }

        if (stageName.Equals("Puerta2", StringComparison.OrdinalIgnoreCase))
        {
            SetInteractableLocks(true, false, true, true);
            SetDoorLocked(puerta1Animation, true);
            SetDoorLocked(puerta2Animation, false);
            SetDoorLocked(puerta3Animation, true);
            HighlightStage("Puerta2");
            SetObjective("La Puerta 2 está activa: entender sin quedarse quieto.", puerta2Entender);
            return;
        }

        if (stageName.Equals("Puerta3", StringComparison.OrdinalIgnoreCase))
        {
            SetInteractableLocks(true, true, false, true);
            SetDoorLocked(puerta1Animation, true);
            SetDoorLocked(puerta2Animation, true);
            SetDoorLocked(puerta3Animation, false);
            HighlightStage("Puerta3");
            SetObjective("La Puerta 3 está activa: compromiso.", puerta3Compromiso);
            return;
        }

        if (stageName.Equals("Estacion4", StringComparison.OrdinalIgnoreCase))
        {
            SetInteractableLocks(true, true, true, false);
            SetDoorLocked(puerta1Animation, true);
            SetDoorLocked(puerta2Animation, true);
            SetDoorLocked(puerta3Animation, true);
            HighlightStage("Estacion4");
            SetObjective("Abre el Archivo de preguntas.", estacion4Preguntas);
        }
    }

    public void ConfigurePlaceholders(
        GameObject puerta1,
        GameObject puerta2,
        GameObject puerta3,
        GameObject estacion4,
        GameObject brujula,
        NarrativeInteractable puerta1NarrativeInteractable,
        NarrativeInteractable puerta2NarrativeInteractable,
        NarrativeInteractable puerta3NarrativeInteractable,
        NarrativeInteractable estacion4NarrativeInteractable,
        DoorAnimationBridge puerta1DoorAnimation,
        DoorAnimationBridge puerta2DoorAnimation,
        DoorAnimationBridge puerta3DoorAnimation,
        RewardVisualController rewardController,
        Chapter1GuidanceController guidance,
        GameObject[] markers,
        Light[] lights)
    {
        puerta1Retirada = puerta1;
        puerta2Entender = puerta2;
        puerta3Compromiso = puerta3;
        estacion4Preguntas = estacion4;
        brujulaDelCompromiso = brujula;
        puerta1Interactable = puerta1NarrativeInteractable;
        puerta2Interactable = puerta2NarrativeInteractable;
        puerta3Interactable = puerta3NarrativeInteractable;
        estacion4Interactable = estacion4NarrativeInteractable;
        puerta1Animation = puerta1DoorAnimation;
        puerta2Animation = puerta2DoorAnimation;
        puerta3Animation = puerta3DoorAnimation;
        rewardVisual = rewardController;
        guidanceController = guidance;
        progressMarkers = markers;
        ambientLights = lights;

        if (progressManager != null)
        {
            progressManager.Configure(guidanceController, inventorySystem, brujulaDelCompromiso);
        }
    }

    private void HighlightStage(string stageName)
    {
        if (stageName.Equals("Puerta1", StringComparison.OrdinalIgnoreCase))
        {
            SetActiveIfAssigned(puerta1Retirada, true);
            PlayDoorHighlight(puerta1Animation);
            SetProgressMarker(0);
            return;
        }

        if (stageName.Equals("Puerta2", StringComparison.OrdinalIgnoreCase))
        {
            SetActiveIfAssigned(puerta2Entender, true);
            PlayDoorHighlight(puerta2Animation);
            SetProgressMarker(1);
            return;
        }

        if (stageName.Equals("Puerta3", StringComparison.OrdinalIgnoreCase))
        {
            SetActiveIfAssigned(puerta3Compromiso, true);
            PlayDoorHighlight(puerta3Animation);
            SetProgressMarker(2);
            return;
        }

        if (stageName.Equals("Estacion4", StringComparison.OrdinalIgnoreCase))
        {
            SetActiveIfAssigned(estacion4Preguntas, true);
            SetProgressMarker(3);
            return;
        }

        if (stageName.Equals("Final", StringComparison.OrdinalIgnoreCase))
        {
            SetProgressMarker(4);
            SetObjective("Has desbloqueado la Brújula del Compromiso.", brujulaDelCompromiso);
        }
    }

    private void PlayStageFeedback()
    {
        if (stageAudioSource != null)
        {
            stageAudioSource.Play();
        }

        if (stageParticles == null)
        {
            return;
        }

        foreach (ParticleSystem particles in stageParticles)
        {
            if (particles != null)
            {
                particles.Play();
            }
        }
    }

    private void SetProgressMarker(int activeIndex)
    {
        if (progressMarkers == null)
        {
            return;
        }

        for (int i = 0; i < progressMarkers.Length; i++)
        {
            if (progressMarkers[i] != null)
            {
                progressMarkers[i].SetActive(i <= activeIndex);
            }
        }

        if (ambientLights == null)
        {
            return;
        }

        for (int i = 0; i < ambientLights.Length; i++)
        {
            if (ambientLights[i] != null)
            {
                ambientLights[i].enabled = i <= activeIndex;
            }
        }
    }

    private void SetInteractableLocks(bool puerta1Locked, bool puerta2Locked, bool puerta3Locked, bool estacion4Locked)
    {
        SetInteractableLocked(puerta1Interactable, puerta1Locked);
        SetInteractableLocked(puerta2Interactable, puerta2Locked);
        SetInteractableLocked(puerta3Interactable, puerta3Locked);
        SetInteractableLocked(estacion4Interactable, estacion4Locked);
    }

    private static void SetActiveIfAssigned(GameObject target, bool active)
    {
        if (target != null)
        {
            target.SetActive(active);
        }
    }

    private static void SetInteractableLocked(NarrativeInteractable target, bool locked)
    {
        if (target != null)
        {
            target.SetLocked(locked);
        }
    }

    private static void SetDoorLocked(DoorAnimationBridge target, bool locked)
    {
        if (target != null)
        {
            target.SetLocked(locked);
        }
    }

    private static void PlayDoorHighlight(DoorAnimationBridge target)
    {
        if (target != null)
        {
            target.PlayHighlight();
        }
    }

    private void SetObjective(string text, GameObject target)
    {
        GameObject resolvedTarget = target;

        if (target == null && autoBindSceneObjects)
        {
            AutoBindSceneObjects();
            resolvedTarget = ResolveCurrentStageTarget();
        }

        Transform objectiveTarget = resolvedTarget != null ? resolvedTarget.transform : null;

        if (progressManager != null)
        {
            progressManager.SetObjective(text, objectiveTarget);
        }
        else if (guidanceController != null)
        {
            guidanceController.SetObjective(text, objectiveTarget);
        }
    }

    private GameObject ResolveCurrentStageTarget()
    {
        if (string.Equals(CurrentStage, "Puerta1", StringComparison.OrdinalIgnoreCase))
        {
            return puerta1Retirada;
        }

        if (string.Equals(CurrentStage, "Puerta2", StringComparison.OrdinalIgnoreCase))
        {
            return puerta2Entender;
        }

        if (string.Equals(CurrentStage, "Puerta3", StringComparison.OrdinalIgnoreCase))
        {
            return puerta3Compromiso;
        }

        if (string.Equals(CurrentStage, "Estacion4", StringComparison.OrdinalIgnoreCase))
        {
            return estacion4Preguntas;
        }

        return brujulaDelCompromiso;
    }

    private void EnsureGuidanceController()
    {
        if (guidanceController == null)
        {
            guidanceController = GetComponent<Chapter1GuidanceController>();
            if (guidanceController == null)
            {
                guidanceController = gameObject.AddComponent<Chapter1GuidanceController>();
            }
        }
    }

    private void EnsureInventorySystem()
    {
        if (inventorySystem == null)
        {
            inventorySystem = GetComponent<Chapter1InventorySystem>();
            if (inventorySystem == null)
            {
                inventorySystem = FindAnyObjectByType<Chapter1InventorySystem>(FindObjectsInactive.Include);
            }

            if (inventorySystem == null)
            {
                inventorySystem = gameObject.AddComponent<Chapter1InventorySystem>();
            }
        }
    }

    private void EnsureProgressManager()
    {
        if (progressManager == null)
        {
            progressManager = GetComponent<Chapter1ProgressManager>();
            if (progressManager == null)
            {
                progressManager = FindAnyObjectByType<Chapter1ProgressManager>(FindObjectsInactive.Include);
            }

            if (progressManager == null)
            {
                progressManager = gameObject.AddComponent<Chapter1ProgressManager>();
            }
        }

        progressManager.Configure(guidanceController, inventorySystem, brujulaDelCompromiso);
    }

    private void AutoBindSceneObjects()
    {
        DialogueRunner dialogueRunner = FindAnyObjectByType<DialogueRunner>(FindObjectsInactive.Include);

        if (puerta1Retirada == null)
        {
            puerta1Retirada = FindSceneObject(
                "NEGACION (1)",
                "PUERTA NEGACION (1)",
                "Puerta1_Retirada",
                "Puerta 1 Retirada");
        }

        if (puerta2Entender == null)
        {
            puerta2Entender = FindSceneObject(
                "NEGOCIACION (1)",
                "PUERTA NEGOCIACION (1)",
                "Puerta2_Entender",
                "Puerta 2 Entender");
        }

        if (puerta3Compromiso == null)
        {
            puerta3Compromiso = FindSceneObject(
                "PUERTA ACEPTACION (1)",
                "ACEPTACION (1)",
                "Puerta3_Compromiso",
                "Puerta 3 Compromiso");
        }

        if (estacion4Preguntas == null)
        {
            estacion4Preguntas = FindSceneObject("Estacion4_Preguntas", "Archivo de preguntas");
        }

        if (brujulaDelCompromiso == null)
        {
            brujulaDelCompromiso = FindSceneObject("BrujulaDelCompromiso", "BrújulaDelCompromiso");
        }

        if (progressManager != null)
        {
            progressManager.Configure(guidanceController, inventorySystem, brujulaDelCompromiso);
        }

        puerta1Animation = EnsureDoorAnimation(puerta1Retirada, puerta1Animation);
        puerta2Animation = EnsureDoorAnimation(puerta2Entender, puerta2Animation);
        puerta3Animation = EnsureDoorAnimation(puerta3Compromiso, puerta3Animation);

        puerta1Interactable = EnsureNarrativeInteractable(
            puerta1Retirada,
            puerta1Interactable,
            "Puerta1",
            "E  Abrir",
            "Bloqueada",
            dialogueRunner,
            puerta1Animation);

        puerta2Interactable = EnsureNarrativeInteractable(
            puerta2Entender,
            puerta2Interactable,
            "Puerta2",
            "E  Abrir",
            "Bloqueada",
            dialogueRunner,
            puerta2Animation);

        puerta3Interactable = EnsureNarrativeInteractable(
            puerta3Compromiso,
            puerta3Interactable,
            "Puerta3",
            "E  Abrir",
            "Bloqueada",
            dialogueRunner,
            puerta3Animation);

        estacion4Interactable = EnsureNarrativeInteractable(
            estacion4Preguntas,
            estacion4Interactable,
            "Estacion4",
            "E  Responder",
            "Completa las puertas",
            dialogueRunner,
            null);
    }

    private static GameObject FindSceneObject(params string[] names)
    {
        Transform[] transforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (string objectName in names)
        {
            if (string.IsNullOrWhiteSpace(objectName))
            {
                continue;
            }

            foreach (Transform candidate in transforms)
            {
                if (candidate != null && candidate.name.Equals(objectName, StringComparison.OrdinalIgnoreCase))
                {
                    return candidate.gameObject;
                }
            }
        }

        foreach (string objectName in names)
        {
            if (string.IsNullOrWhiteSpace(objectName))
            {
                continue;
            }

            string normalizedName = NormalizeName(objectName);
            foreach (Transform candidate in transforms)
            {
                if (candidate == null)
                {
                    continue;
                }

                string normalizedCandidate = NormalizeName(candidate.name);
                if (normalizedCandidate.Contains(normalizedName) || normalizedName.Contains(normalizedCandidate))
                {
                    return candidate.gameObject;
                }
            }
        }

        return null;
    }

    private static string NormalizeName(string value)
    {
        return value
            .Replace(" ", string.Empty)
            .Replace("_", string.Empty)
            .Replace("-", string.Empty)
            .Replace("(", string.Empty)
            .Replace(")", string.Empty)
            .ToUpperInvariant();
    }

    private static DoorAnimationBridge EnsureDoorAnimation(GameObject target, DoorAnimationBridge existing)
    {
        if (existing != null || target == null)
        {
            return existing;
        }

        DoorAnimationBridge animationBridge = target.GetComponent<DoorAnimationBridge>();
        if (animationBridge == null)
        {
            animationBridge = target.AddComponent<DoorAnimationBridge>();
        }

        return animationBridge;
    }

    private static NarrativeInteractable EnsureNarrativeInteractable(
        GameObject target,
        NarrativeInteractable existing,
        string yarnNodeName,
        string promptText,
        string lockedPromptText,
        DialogueRunner dialogueRunner,
        DoorAnimationBridge doorAnimation)
    {
        if (target == null)
        {
            return existing;
        }

        NarrativeInteractable interactable = existing != null && existing.transform.IsChildOf(target.transform)
            ? existing
            : target.GetComponentInChildren<NarrativeInteractable>(true);

        if (interactable == null)
        {
            GameObject triggerObject = new GameObject("Chapter1_InteractionTrigger");
            triggerObject.transform.SetParent(target.transform, false);
            triggerObject.transform.localRotation = Quaternion.identity;
            triggerObject.transform.localScale = Vector3.one;

            BoxCollider collider = triggerObject.AddComponent<BoxCollider>();
            ConfigureTriggerCollider(collider, target);

            interactable = triggerObject.AddComponent<NarrativeInteractable>();
        }

        interactable.SetYarnNodeName(yarnNodeName);
        interactable.SetDialogueRunner(dialogueRunner);
        interactable.SetPlayerTag(string.Empty);
        interactable.SetDoorAnimationBridge(doorAnimation);
        interactable.SetPromptObject(EnsurePrompt(target.transform, "Chapter1_Prompt", promptText, new Color(0.65f, 0.95f, 1f)));
        interactable.SetLockedPromptObject(EnsurePrompt(target.transform, "Chapter1_LockedPrompt", lockedPromptText, new Color(1f, 0.55f, 0.45f)));

        return interactable;
    }

    private static void ConfigureTriggerCollider(BoxCollider collider, GameObject target)
    {
        Bounds bounds = GetWorldBounds(target);
        Vector3 localCenter = target.transform.InverseTransformPoint(bounds.center);
        Vector3 localSize = new Vector3(
            SafeDivide(bounds.size.x + 1.5f, target.transform.lossyScale.x),
            SafeDivide(Mathf.Max(bounds.size.y, 2.5f) + 1f, target.transform.lossyScale.y),
            SafeDivide(bounds.size.z + 2f, target.transform.lossyScale.z));

        collider.center = localCenter;
        collider.size = new Vector3(
            Mathf.Max(localSize.x, 2.5f),
            Mathf.Max(localSize.y, 3f),
            Mathf.Max(localSize.z, 2.5f));
        collider.isTrigger = true;
    }

    private static Bounds GetWorldBounds(GameObject target)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            return new Bounds(target.transform.position, new Vector3(2f, 3f, 1f));
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds;
    }

    private static float SafeDivide(float value, float divisor)
    {
        return Mathf.Abs(divisor) <= 0.001f ? value : value / Mathf.Abs(divisor);
    }

    private static GameObject EnsurePrompt(Transform parent, string name, string text, Color color)
    {
        Transform existing = parent.Find(name);
        GameObject promptObject = existing != null ? existing.gameObject : new GameObject(name);
        promptObject.transform.SetParent(parent, false);
        promptObject.transform.localPosition = Vector3.up * 2.4f;
        promptObject.transform.localRotation = Quaternion.identity;

        TextMesh textMesh = promptObject.GetComponent<TextMesh>();
        if (textMesh == null)
        {
            textMesh = promptObject.AddComponent<TextMesh>();
        }

        textMesh.text = text;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.characterSize = 0.12f;
        textMesh.fontSize = 48;
        textMesh.color = color;

        if (promptObject.GetComponent<Chapter1Billboard>() == null)
        {
            promptObject.AddComponent<Chapter1Billboard>();
        }

        promptObject.SetActive(false);
        return promptObject;
    }
}
