using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private bool isGameOver;

    public bool IsGameOver => isGameOver;
    public bool CanPlay => !isGameOver;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"Duplicate {nameof(GameManager)} found on {name}. Destroying the latest instance.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Time.timeScale = 1f;
    }

    public void GameOver()
    {
        if (isGameOver)
        {
            return;
        }

        isGameOver = true;
        Time.timeScale = 0f;
        Debug.Log("GAME OVER");
    }

    public void ResetSession()
    {
        isGameOver = false;
        Time.timeScale = 1f;
    }
}
