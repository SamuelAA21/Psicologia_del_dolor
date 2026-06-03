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
        ResolveReferences();
    }

    private void Update()
    {
        ResolveReferences();

        if (playerTransform == null || breathingController == null || !breathingController.IsRunning)
        {
            IsPlayerInTargetZone = false;
            return;
        }

        if (GameManager.Instance != null && !GameManager.Instance.CanPlay)
        {
            IsPlayerInTargetZone = false;
            return;
        }

        if (!breathingController.TryGetCurrentPhaseSettings(out _))
        {
            IsPlayerInTargetZone = false;
            return;
        }

        Vector2 targetRange = breathingController.GetTherapeuticTargetRange();
        float playerY = playerTransform.position.y;

        IsPlayerInTargetZone = playerY >= targetRange.x && playerY <= targetRange.y;
        totalTrackedTime += Time.deltaTime;

        if (IsPlayerInTargetZone)
        {
            alignedTime += Time.deltaTime;
        }
    }

    private void ResolveReferences()
    {
        if (breathingController == null)
        {
            MiniGameFlowController flowController = FindAnyObjectByType<MiniGameFlowController>();
            breathingController = flowController != null && flowController.Controller != null
                ? flowController.Controller
                : FindAnyObjectByType<BreathingController>();
        }

        if (playerTransform == null)
        {
            PlayerJetpack player = FindAnyObjectByType<PlayerJetpack>();
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }
    }
}
