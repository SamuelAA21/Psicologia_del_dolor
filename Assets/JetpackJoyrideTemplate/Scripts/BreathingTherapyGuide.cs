using UnityEngine;

public class BreathingTherapyGuide : MonoBehaviour
{
    [SerializeField] private BreathingController breathingController;
    [SerializeField] private Transform playerTransform;

    private float alignedTime;
    private float totalTrackedTime;

    public bool IsPlayerInTargetZone { get; private set; }
    public float RegulationScore => totalTrackedTime <= 0f ? 0f : alignedTime / totalTrackedTime;
    public string CurrentInstruction => breathingController != null && breathingController.TryGetCurrentPhaseSettings(out BreathingPhaseSettings settings)
        ? settings.guidanceText
        : string.Empty;

    private void Awake()
    {
        if (breathingController == null)
        {
            breathingController = FindObjectOfType<BreathingController>();
        }
    }

    private void Update()
    {
        if (playerTransform == null || breathingController == null || !breathingController.IsRunning)
        {
            IsPlayerInTargetZone = false;
            return;
        }

        if (GameManager.Instance != null && !GameManager.Instance.CanPlay)
        {
            return;
        }

        if (!breathingController.TryGetCurrentPhaseSettings(out BreathingPhaseSettings settings))
        {
            IsPlayerInTargetZone = false;
            return;
        }

        Vector2 targetRange = settings.GetSortedPlayerTargetRange();
        float playerY = playerTransform.position.y;

        IsPlayerInTargetZone = playerY >= targetRange.x && playerY <= targetRange.y;
        totalTrackedTime += Time.deltaTime;

        if (IsPlayerInTargetZone)
        {
            alignedTime += Time.deltaTime;
        }
    }
}
