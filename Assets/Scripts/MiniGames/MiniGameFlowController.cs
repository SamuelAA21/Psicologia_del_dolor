using UnityEngine;

public class MiniGameFlowController : MonoBehaviour
{
    [SerializeField] private BreathingController controller;
    [SerializeField] private string returnSceneName = "SampleScene";

    private void Start()
    {
        if (controller == null)
            controller = FindFirstObjectByType<BreathingController>();

        if (controller != null)
        {
            controller.SessionCompleted += OnWin;
        }
    }

    private void OnDestroy()
    {
        if (controller != null)
        {
            controller.SessionCompleted -= OnWin;
        }
    }

    private void OnWin()
    {
        SceneLoader.LoadSceneSafe(returnSceneName);
    }
}
