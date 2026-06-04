using UnityEngine;

public static class MiniGameRuntimeUtility
{
    public static BreathingController ResolveBreathingController(BreathingController current = null)
    {
        if (current != null)
        {
            return current;
        }

        MiniGameFlowController flowController = Object.FindAnyObjectByType<MiniGameFlowController>();
        if (flowController != null && flowController.Controller != null)
        {
            return flowController.Controller;
        }

        return Object.FindAnyObjectByType<BreathingController>();
    }

    public static Transform ResolvePlayerTransform(Transform current = null)
    {
        if (current != null)
        {
            return current;
        }

        PlayerJetpack player = Object.FindAnyObjectByType<PlayerJetpack>();
        return player != null ? player.transform : null;
    }

    public static BreathingTherapyGuide ResolveTherapyGuide(BreathingTherapyGuide current = null)
    {
        return current != null ? current : Object.FindAnyObjectByType<BreathingTherapyGuide>();
    }
}
