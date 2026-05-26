using System;
using UnityEngine;
using Yarn.Unity;

public class Chapter1EnvironmentController : MonoBehaviour
{
    public static Chapter1EnvironmentController Instance { get; private set; }

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
    }

    public void UnlockStage(string stageName)
    {
        if (string.IsNullOrWhiteSpace(stageName))
        {
            return;
        }

        if (stageName.Equals("Puerta1", StringComparison.OrdinalIgnoreCase))
        {
            SetInteractableLocks(false, true, true, true);
            SetDoorLocked(puerta1Animation, false);
            SetDoorLocked(puerta2Animation, true);
            SetDoorLocked(puerta3Animation, true);
            HighlightStage("Puerta1");
            return;
        }

        if (stageName.Equals("Puerta2", StringComparison.OrdinalIgnoreCase))
        {
            SetInteractableLocks(true, false, true, true);
            SetDoorLocked(puerta1Animation, true);
            SetDoorLocked(puerta2Animation, false);
            SetDoorLocked(puerta3Animation, true);
            HighlightStage("Puerta2");
            return;
        }

        if (stageName.Equals("Puerta3", StringComparison.OrdinalIgnoreCase))
        {
            SetInteractableLocks(true, true, false, true);
            SetDoorLocked(puerta1Animation, true);
            SetDoorLocked(puerta2Animation, true);
            SetDoorLocked(puerta3Animation, false);
            HighlightStage("Puerta3");
            return;
        }

        if (stageName.Equals("Estacion4", StringComparison.OrdinalIgnoreCase))
        {
            SetInteractableLocks(true, true, true, false);
            SetDoorLocked(puerta1Animation, true);
            SetDoorLocked(puerta2Animation, true);
            SetDoorLocked(puerta3Animation, true);
            HighlightStage("Estacion4");
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
        progressMarkers = markers;
        ambientLights = lights;
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
}
