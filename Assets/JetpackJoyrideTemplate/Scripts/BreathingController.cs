using System;
using UnityEngine;

public enum BreathPhase
{
    Inhale,
    Hold,
    Exhale
}

[Serializable]
public class BreathingPhaseSettings
{
    public BreathPhase phase;
    [Min(0.1f)] public float duration = 4f;
    public Vector2 playerTargetYRange = new Vector2(-1f, 1f);
    public Vector2 obstacleSpawnYRange = new Vector2(-1f, 1f);
    [TextArea] public string guidanceText;

    public Vector2 GetSortedPlayerTargetRange()
    {
        return new Vector2(
            Mathf.Min(playerTargetYRange.x, playerTargetYRange.y),
            Mathf.Max(playerTargetYRange.x, playerTargetYRange.y));
    }

    public Vector2 GetSortedObstacleSpawnRange()
    {
        return new Vector2(
            Mathf.Min(obstacleSpawnYRange.x, obstacleSpawnYRange.y),
            Mathf.Max(obstacleSpawnYRange.x, obstacleSpawnYRange.y));
    }
}

public class BreathingController : MonoBehaviour
{
    [SerializeField]
    private BreathingPhaseSettings[] phaseSequence =
    {
        new BreathingPhaseSettings
        {
            phase = BreathPhase.Inhale,
            duration = 4f,
            playerTargetYRange = new Vector2(2.25f, 3.25f),
            obstacleSpawnYRange = new Vector2(2.25f, 3.25f),
            guidanceText = "Inhala y asciende suavemente."
        },
        new BreathingPhaseSettings
        {
            phase = BreathPhase.Hold,
            duration = 7f,
            playerTargetYRange = new Vector2(2.25f, 3.25f),
            obstacleSpawnYRange = new Vector2(2.25f, 3.25f),
            guidanceText = "Sosten con estabilidad en el centro."
        },
        new BreathingPhaseSettings
        {
            phase = BreathPhase.Exhale,
            duration = 8f,
            playerTargetYRange = new Vector2(0f, 1f),
            obstacleSpawnYRange = new Vector2(0f, 1f),
            guidanceText = "Exhala y desciende sin prisa."
        }
    };

    [SerializeField] private bool playOnStart = true;

    private int currentPhaseIndex;
    private float phaseTimer;
    private bool isRunning;

    public event Action<BreathingPhaseSettings> PhaseChanged;

    public BreathPhase CurrentPhase => CurrentPhaseSettings.phase;
    public BreathingPhaseSettings CurrentPhaseSettings => phaseSequence[currentPhaseIndex];
    public float CurrentPhaseProgress => CurrentPhaseSettings.duration <= 0f
        ? 1f
        : Mathf.Clamp01(phaseTimer / CurrentPhaseSettings.duration);
    public bool IsRunning => isRunning;

    private void Awake()
    {
        EnsureValidConfiguration();
        currentPhaseIndex = 0;
    }

    private void Start()
    {
        if (playOnStart)
        {
            StartCycle();
        }
    }

    private void Update()
    {
        if (!isRunning || !CanSimulate())
        {
            return;
        }

        phaseTimer += Time.deltaTime;

        if (phaseTimer < CurrentPhaseSettings.duration)
        {
            return;
        }

        AdvancePhase();
    }

    public void StartCycle()
    {
        EnsureValidConfiguration();
        isRunning = true;
        currentPhaseIndex = Mathf.Clamp(currentPhaseIndex, 0, phaseSequence.Length - 1);
        phaseTimer = 0f;
        NotifyPhaseChanged();
    }

    public void StopCycle()
    {
        isRunning = false;
    }

    public void ResetCycle()
    {
        currentPhaseIndex = 0;
        phaseTimer = 0f;
        NotifyPhaseChanged();
    }

    public bool TryGetCurrentPhaseSettings(out BreathingPhaseSettings settings)
    {
        settings = null;

        if (phaseSequence == null || phaseSequence.Length == 0)
        {
            return false;
        }

        settings = CurrentPhaseSettings;
        return true;
    }

    public float GetTherapeuticCenterY()
    {
        if (!TryGetCurrentPhaseSettings(out BreathingPhaseSettings currentSettings))
        {
            return 0f;
        }

        float currentCenter = GetRangeCenter(currentSettings.GetSortedPlayerTargetRange());

        switch (CurrentPhase)
        {
            case BreathPhase.Inhale:
                return Mathf.Lerp(
                    GetPreviousPhaseCenter(),
                    currentCenter,
                    EaseInhale(CurrentPhaseProgress));

            case BreathPhase.Hold:
                return currentCenter;

            case BreathPhase.Exhale:
                return Mathf.Lerp(
                    GetPreviousPhaseCenter(),
                    currentCenter,
                    EaseExhale(CurrentPhaseProgress));

            default:
                return currentCenter;
        }
    }

    private void AdvancePhase()
    {
        phaseTimer = 0f;
        currentPhaseIndex = (currentPhaseIndex + 1) % phaseSequence.Length;
        NotifyPhaseChanged();
    }

    private void NotifyPhaseChanged()
    {
        PhaseChanged?.Invoke(CurrentPhaseSettings);
    }

    private bool CanSimulate()
    {
        return GameManager.Instance == null || GameManager.Instance.CanPlay;
    }

    private void EnsureValidConfiguration()
    {
        if (phaseSequence != null && phaseSequence.Length > 0)
        {
            for (int i = 0; i < phaseSequence.Length; i++)
            {
                if (phaseSequence[i] == null)
                {
                    phaseSequence[i] = new BreathingPhaseSettings();
                }

                phaseSequence[i].duration = Mathf.Max(0.1f, phaseSequence[i].duration);
            }

            return;
        }

        phaseSequence = new[]
        {
            new BreathingPhaseSettings
            {
                phase = BreathPhase.Inhale,
                duration = 4f,
                playerTargetYRange = new Vector2(2.25f, 3.25f),
                obstacleSpawnYRange = new Vector2(2.25f, 3.25f),
                guidanceText = "Inhala y asciende suavemente."
            },
            new BreathingPhaseSettings
            {
                phase = BreathPhase.Hold,
                duration = 7f,
                playerTargetYRange = new Vector2(2.25f, 3.25f),
                obstacleSpawnYRange = new Vector2(2.25f, 3.25f),
                guidanceText = "Sosten con estabilidad en el centro."
            },
            new BreathingPhaseSettings
            {
                phase = BreathPhase.Exhale,
                duration = 8f,
                playerTargetYRange = new Vector2(0f, 1f),
                obstacleSpawnYRange = new Vector2(0f, 1f),
                guidanceText = "Exhala y desciende sin prisa."
            }
        };
    }

    private float GetPreviousPhaseCenter()
    {
        int previousIndex = (currentPhaseIndex - 1 + phaseSequence.Length) % phaseSequence.Length;
        return GetRangeCenter(phaseSequence[previousIndex].GetSortedPlayerTargetRange());
    }

    private static float GetRangeCenter(Vector2 range)
    {
        return (range.x + range.y) * 0.5f;
    }

    private static float EaseInhale(float progress)
    {
        return Mathf.Sin(progress * Mathf.PI * 0.5f);
    }

    private static float EaseExhale(float progress)
    {
        return 1f - Mathf.Cos(progress * Mathf.PI * 0.5f);
    }
}
