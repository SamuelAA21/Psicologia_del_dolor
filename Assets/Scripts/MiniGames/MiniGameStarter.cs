using UnityEngine;
using UnityEngine.SceneManagement;

public class MiniGameStarter : MonoBehaviour
{
    private const string TargetSceneName = "FirstMiniGame";

    [SerializeField] private BreathingController controller;
    [SerializeField] private bool startOnlyInMiniGameScene = true;
    [SerializeField] private bool startOnlyWhenStopped = true;

    private void Start()
    {
        if (startOnlyInMiniGameScene
            && !SceneManager.GetActiveScene().name.Equals(TargetSceneName, System.StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (controller == null)
        {
            controller = FindAnyObjectByType<BreathingController>();
        }

        if (controller == null)
        {
            Debug.LogWarning($"{nameof(MiniGameStarter)} could not find a {nameof(BreathingController)} in {SceneManager.GetActiveScene().name}.", this);
            return;
        }

        if (startOnlyWhenStopped && controller.IsRunning)
        {
            return;
        }

        controller.StartCycle();
    }
}
