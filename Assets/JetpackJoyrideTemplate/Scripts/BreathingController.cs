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
            playerTargetYRange = new Vector2(1.45f, 2.45f),
            obstacleSpawnYRange = new Vector2(1.45f, 2.45f),
            guidanceText = "Manten Espacio: sube lento."
        },
        new BreathingPhaseSettings
        {
            phase = BreathPhase.Hold,
            duration = 3f,
            playerTargetYRange = new Vector2(1.45f, 2.45f),
            obstacleSpawnYRange = new Vector2(1.45f, 2.45f),
            guidanceText = "Pulsa suave: manten altura."
        },
        new BreathingPhaseSettings
        {
            phase = BreathPhase.Exhale,
            duration = 4f,
            playerTargetYRange = new Vector2(-0.55f, 0.45f),
            obstacleSpawnYRange = new Vector2(-0.55f, 0.45f),
            guidanceText = "Suelta Espacio: baja lento."
        }
    };

    [SerializeField] private bool playOnStart = false;
    [SerializeField, Min(1)] private int sessionCycleCount = 4;

    private int currentPhaseIndex;
    private float phaseTimer;
    private bool isRunning;
    private float phaseEntryCenterY;
    private int completedCycles;

    public event Action<BreathingPhaseSettings> PhaseChanged;
    public event Action SessionCompleted;

    public BreathPhase CurrentPhase => CurrentPhaseSettings.phase;
    public BreathingPhaseSettings CurrentPhaseSettings => phaseSequence[currentPhaseIndex];
    public float CurrentPhaseProgress => CurrentPhaseSettings.duration <= 0f
        ? 1f
        : Mathf.Clamp01(phaseTimer / CurrentPhaseSettings.duration);
    public bool IsRunning => isRunning;
    public int CompletedCycles => completedCycles;
    public int SessionCycleCount => sessionCycleCount;

    private void Awake()
    {
        EnsureValidConfiguration();
        currentPhaseIndex = 0;
        phaseEntryCenterY = GetRangeCenter(CurrentPhaseSettings.GetSortedPlayerTargetRange());
        completedCycles = 0;
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
        completedCycles = 0;
        currentPhaseIndex = Mathf.Clamp(currentPhaseIndex, 0, phaseSequence.Length - 1);
        phaseTimer = 0f;
        phaseEntryCenterY = GetPhaseInitialCenter(currentPhaseIndex);
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
        phaseEntryCenterY = GetPhaseInitialCenter(currentPhaseIndex);
        completedCycles = 0;
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
        return GetTherapeuticCenterY(0f);
    }

    public float GetTherapeuticCenterY(float timeOffsetSeconds)
    {
        return TryEvaluateAtOffset(timeOffsetSeconds, out _, out float centerY)
            ? centerY
            : 0f;
    }

    public Vector2 GetTherapeuticTargetRange(float timeOffsetSeconds = 0f)
    {
        if (!TryEvaluateAtOffset(timeOffsetSeconds, out BreathingPhaseSettings settings, out float centerY))
        {
            return Vector2.zero;
        }

        Vector2 configuredRange = settings.GetSortedPlayerTargetRange();
        float halfHeight = Mathf.Max(0.35f, (configuredRange.y - configuredRange.x) * 0.5f);
        return new Vector2(centerY - halfHeight, centerY + halfHeight);
    }

    private bool TryEvaluateAtOffset(float timeOffsetSeconds, out BreathingPhaseSettings settings, out float centerY)
    {
        settings = null;
        centerY = 0f;

        if (!TryGetCurrentPhaseSettings(out _))
        {
            return false;
        }

        int phaseIndex = currentPhaseIndex;
        float simulatedTimer = phaseTimer + Mathf.Max(0f, timeOffsetSeconds);
        float simulatedEntryCenter = phaseEntryCenterY;
        int guard = 0;
        int maxIterations = Mathf.Max(phaseSequence.Length * Mathf.Max(1, sessionCycleCount + 1), phaseSequence.Length + 1);

        while (guard < maxIterations && simulatedTimer >= phaseSequence[phaseIndex].duration)
        {
            BreathingPhaseSettings phaseSettings = phaseSequence[phaseIndex];
            simulatedTimer -= phaseSettings.duration;
            simulatedEntryCenter = EvaluatePhaseCenter(phaseSettings, 1f, simulatedEntryCenter);
            phaseIndex = (phaseIndex + 1) % phaseSequence.Length;
            guard++;
        }

        settings = phaseSequence[phaseIndex];
        float progress = settings.duration <= 0f ? 1f : Mathf.Clamp01(simulatedTimer / settings.duration);
        centerY = EvaluatePhaseCenter(settings, progress, simulatedEntryCenter);
        return true;
    }

    private float EvaluatePhaseCenter(BreathingPhaseSettings currentSettings, float progress)
    {
        return EvaluatePhaseCenter(currentSettings, progress, phaseEntryCenterY);
    }

    private float EvaluatePhaseCenter(BreathingPhaseSettings currentSettings, float progress, float entryCenterY)
    {
        float currentCenter = GetRangeCenter(currentSettings.GetSortedPlayerTargetRange());

        switch (currentSettings.phase)
        {
            case BreathPhase.Inhale:
                return Mathf.Lerp(
                    entryCenterY,
                    currentCenter,
                    EaseInhale(progress));

            case BreathPhase.Hold:
                return entryCenterY;

            case BreathPhase.Exhale:
                return Mathf.Lerp(
                    entryCenterY,
                    currentCenter,
                    EaseExhale(progress));

            default:
                return currentCenter;
        }
    }

    private void AdvancePhase()
    {
        float exitCenterY = EvaluatePhaseCenter(CurrentPhaseSettings, 1f);
        bool completedFullBreath = currentPhaseIndex == phaseSequence.Length - 1;

        if (completedFullBreath)
        {
            completedCycles++;
        }

        if (completedFullBreath && completedCycles >= sessionCycleCount)
        {
            phaseTimer = CurrentPhaseSettings.duration;
            phaseEntryCenterY = exitCenterY;
            isRunning = false;
            SessionCompleted?.Invoke();
            return;
        }

        phaseTimer = 0f;
        currentPhaseIndex = (currentPhaseIndex + 1) % phaseSequence.Length;
        phaseEntryCenterY = exitCenterY;
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
                playerTargetYRange = new Vector2(1.45f, 2.45f),
                obstacleSpawnYRange = new Vector2(1.45f, 2.45f),
                guidanceText = "Manten Espacio: sube lento."
            },
            new BreathingPhaseSettings
            {
                phase = BreathPhase.Hold,
                duration = 3f,
                playerTargetYRange = new Vector2(1.45f, 2.45f),
                obstacleSpawnYRange = new Vector2(1.45f, 2.45f),
                guidanceText = "Pulsa suave: manten altura."
            },
            new BreathingPhaseSettings
            {
                phase = BreathPhase.Exhale,
                duration = 4f,
                playerTargetYRange = new Vector2(-0.55f, 0.45f),
                obstacleSpawnYRange = new Vector2(-0.55f, 0.45f),
                guidanceText = "Suelta Espacio: baja lento."
            }
        };
    }

    private float GetPhaseInitialCenter(int phaseIndex)
    {
        int previousIndex = (phaseIndex - 1 + phaseSequence.Length) % phaseSequence.Length;
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
