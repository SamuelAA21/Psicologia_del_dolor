using UnityEngine;

public class RewardVisualController : MonoBehaviour
{
    [SerializeField] private GameObject rewardObject;
    [SerializeField] private bool hideOnAwake = true;
    [SerializeField] private Animator animator;
    [SerializeField] private string unlockTrigger;
    [SerializeField] private string visibleBool;
    [SerializeField] private AudioSource unlockAudioSource;
    [SerializeField] private ParticleSystem[] unlockParticles;

    public void Configure(GameObject targetRewardObject)
    {
        rewardObject = targetRewardObject;
    }

    private void Awake()
    {
        if (rewardObject == null)
        {
            rewardObject = gameObject;
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (hideOnAwake)
        {
            HideReward();
        }
    }

    public void ShowReward()
    {
        if (rewardObject != null)
        {
            rewardObject.SetActive(true);
        }

        SetVisibleBool(true);
    }

    public void HideReward()
    {
        SetVisibleBool(false);

        if (rewardObject != null)
        {
            rewardObject.SetActive(false);
        }
    }

    public void PlayUnlock()
    {
        ShowReward();

        if (animator != null && !string.IsNullOrWhiteSpace(unlockTrigger))
        {
            animator.SetTrigger(unlockTrigger);
        }

        if (unlockAudioSource != null)
        {
            unlockAudioSource.Play();
        }

        if (unlockParticles == null)
        {
            return;
        }

        foreach (ParticleSystem particles in unlockParticles)
        {
            if (particles != null)
            {
                particles.Play();
            }
        }
    }

    private void SetVisibleBool(bool visible)
    {
        if (animator != null && !string.IsNullOrWhiteSpace(visibleBool))
        {
            animator.SetBool(visibleBool, visible);
        }
    }
}
