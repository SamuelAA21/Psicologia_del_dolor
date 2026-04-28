using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [SerializeField] private string mainSceneName = "SampleScene";

    private string currentScene;
    private Coroutine activeLoadRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("SceneLoader received an empty scene name.");
            return;
        }

        if (activeLoadRoutine != null)
        {
            StopCoroutine(activeLoadRoutine);
        }

        activeLoadRoutine = StartCoroutine(LoadRoutine(sceneName));
    }

    public void LoadMainScene()
    {
        LoadScene(mainSceneName);
    }

    public static void LoadSceneSafe(string sceneName)
    {
        if (Instance != null)
        {
            Instance.LoadScene(sceneName);
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    public static void LoadMainSceneSafe()
    {
        if (Instance != null)
        {
            Instance.LoadMainScene();
            return;
        }

        SceneManager.LoadScene("SampleScene");
    }

    private IEnumerator LoadRoutine(string sceneName)
    {
        yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        currentScene = sceneName;
        activeLoadRoutine = null;
    }
}
