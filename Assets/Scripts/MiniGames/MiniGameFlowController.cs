using UnityEngine;

public class MiniGameFlowController : MonoBehaviour
{
    [SerializeField] private BreathingController controller;
    [SerializeField] private string returnSceneName = "SampleScene";

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
        Chapter1ProgressState.ReportBreathingCompleted();
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
