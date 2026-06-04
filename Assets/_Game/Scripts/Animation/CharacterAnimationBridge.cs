using UnityEngine;

public class CharacterAnimationBridge : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string idleTrigger;
    [SerializeField] private string talkTrigger;
    [SerializeField] private string walkTrigger;
    [SerializeField] private string breathingTrigger;
    [SerializeField] private string talkingBool;
    [SerializeField] private string walkingBool;
    [SerializeField] private string breathingBool;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    public void PlayIdle()
    {
        Trigger(idleTrigger);
        SetBool(talkingBool, false);
        SetBool(walkingBool, false);
        SetBool(breathingBool, false);
    }

    public void PlayTalk()
    {
        Trigger(talkTrigger);
        SetBool(talkingBool, true);
    }

    public void PlayWalk()
    {
        Trigger(walkTrigger);
        SetBool(walkingBool, true);
    }

    public void PlayBreathing()
    {
        Trigger(breathingTrigger);
        SetBool(breathingBool, true);
    }

    public void SetBool(string parameter, bool value)
    {
        if (animator == null || string.IsNullOrWhiteSpace(parameter))
        {
            return;
        }

        animator.SetBool(parameter, value);
    }

    public void Trigger(string parameter)
    {
        if (animator == null || string.IsNullOrWhiteSpace(parameter))
        {
            return;
        }

        animator.SetTrigger(parameter);
    }
}
