using UnityEngine;

public class MiniGameStarter : MonoBehaviour
{
    [SerializeField] private BreathingController controller;

    private void Start()
    {
        if (controller == null)
            controller = FindFirstObjectByType<BreathingController>();

        controller?.StartCycle();
    }
}