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

    public void SetStage(string stageName)
    {
        if (string.IsNullOrWhiteSpace(stageName))
        {
            return;
        }

        CurrentStage = stageName;
        avatarClinico?.SetActive(true);
        avatarAnimation?.PlayTalk();
        HighlightStage(stageName);
        PlayStageFeedback();
    }

    public void UnlockReward()
    {
        brujulaDelCompromiso?.SetActive(true);

        if (rewardVisual != null)
        {
            rewardVisual.PlayUnlock();
        }
    }

    private void HighlightStage(string stageName)
    {
        if (stageName.Equals("Puerta1", StringComparison.OrdinalIgnoreCase))
        {
            puerta1Retirada?.SetActive(true);
            puerta1Animation?.PlayHighlight();
            SetProgressMarker(0);
            return;
        }

        if (stageName.Equals("Puerta2", StringComparison.OrdinalIgnoreCase))
        {
            puerta2Entender?.SetActive(true);
            puerta2Animation?.PlayHighlight();
            SetProgressMarker(1);
            return;
        }

        if (stageName.Equals("Puerta3", StringComparison.OrdinalIgnoreCase))
        {
            puerta3Compromiso?.SetActive(true);
            puerta3Animation?.PlayHighlight();
            SetProgressMarker(2);
            return;
        }

        if (stageName.Equals("Estacion4", StringComparison.OrdinalIgnoreCase))
        {
            estacion4Preguntas?.SetActive(true);
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
}
