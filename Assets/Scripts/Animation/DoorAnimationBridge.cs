using UnityEngine;

public class DoorAnimationBridge : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string openTrigger;
    [SerializeField] private string closeTrigger;
    [SerializeField] private string highlightTrigger;
    [SerializeField] private string lockedBool;
    [SerializeField] private GameObject lockedVisual;
    [SerializeField] private GameObject unlockedVisual;
    [SerializeField] private GameObject highlightVisual;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private ParticleSystem highlightParticles;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    public void PlayOpen()
    {
        Trigger(openTrigger);
        SetLocked(false);
    }

    public void PlayClose()
    {
        Trigger(closeTrigger);
    }

    public void PlayHighlight()
    {
        Trigger(highlightTrigger);
        SetActiveIfAssigned(highlightVisual, true);

        if (highlightParticles != null)
        {
            highlightParticles.Play();
        }

        if (audioSource != null)
        {
            audioSource.Play();
        }
    }

    public void SetLocked(bool locked)
    {
        if (animator != null && !string.IsNullOrWhiteSpace(lockedBool))
        {
            animator.SetBool(lockedBool, locked);
        }

        SetActiveIfAssigned(lockedVisual, locked);
        SetActiveIfAssigned(unlockedVisual, !locked);
    }

    private void Trigger(string parameter)
    {
        if (animator == null || string.IsNullOrWhiteSpace(parameter))
        {
            return;
        }

        animator.SetTrigger(parameter);
    }

    private static void SetActiveIfAssigned(GameObject target, bool active)
    {
        if (target != null)
        {
            target.SetActive(active);
        }
    }
}
