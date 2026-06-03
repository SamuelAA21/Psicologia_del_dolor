using UnityEngine;

public class MiniGameFlowController : MonoBehaviour
{
    [SerializeField] private BreathingController controller;
    [SerializeField] private string returnSceneName = "SampleScene";

    private bool completed;

    public BreathingController Controller
    {
        get
        {
            EnsureController();
            return controller;
        }
    }

    private void Start()
    {
        EnsureController();

        if (controller != null)
        {
            controller.SessionCompleted += OnWin;
            return;
        }

        Debug.LogWarning($"{nameof(MiniGameFlowController)} could not find a {nameof(BreathingController)} in the scene.", this);
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
        if (completed)
        {
            return;
        }

        completed = true;
        Chapter1ProgressState.ReportBreathingCompleted();

        if (JetpackCompletionOverlay.Instance != null)
        {
            JetpackCompletionOverlay.Instance.ShowCompletion(ReturnToScene);
            return;
        }

        ReturnToScene();
    }

    private void ReturnToScene()
    {
        SceneLoader.LoadSceneSafe(returnSceneName);
    }

    private void EnsureController()
    {
        if (controller == null)
        {
            controller = FindAnyObjectByType<BreathingController>();
        }
    }
}
