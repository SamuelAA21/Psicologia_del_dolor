using UnityEngine;
using UnityEngine.SceneManagement;

public class MiniGameStarter : MonoBehaviour
{
    [SerializeField] private BreathingController controller;
    [SerializeField] private bool startOnlyInMiniGameScene = true;
    [SerializeField] private bool startOnlyWhenStopped = true;

    private void Start()
    {
        if (startOnlyInMiniGameScene
            && !GameSceneNames.IsBreathingMiniGame(SceneManager.GetActiveScene().name))
        {
            return;
        }

        if (controller == null)
        {
            controller = MiniGameRuntimeUtility.ResolveBreathingController();
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
