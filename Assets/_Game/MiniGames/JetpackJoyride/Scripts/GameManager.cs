using UnityEngine;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private bool isGameOver;
    [SerializeField] private string returnSceneName = GameSceneNames.MainGame;
    [SerializeField, Min(1)] private int maxMistakes = 3;

    private int mistakes;

    public bool IsGameOver => isGameOver;
    public bool CanPlay => !isGameOver;
    public int MaxMistakes => maxMistakes;
    public int RemainingMistakes => Mathf.Max(0, maxMistakes - mistakes);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"Duplicate {nameof(GameManager)} found on {name}. Destroying the latest instance.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        mistakes = 0;
        Time.timeScale = 1f;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public bool RegisterMistake()
    {
        if (isGameOver)
        {
            return true;
        }

        mistakes++;

        if (mistakes >= maxMistakes)
        {
            GameOver();
            return true;
        }

        return false;
    }

    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        Time.timeScale = 1f;

        Chapter1ProgressState.ReportBreathingFailed();
        SceneLoader.LoadSceneSafe(returnSceneName);
    }

    public void ResetSession()
    {
        isGameOver = false;
        mistakes = 0;
        Time.timeScale = 1f;
    }
}
