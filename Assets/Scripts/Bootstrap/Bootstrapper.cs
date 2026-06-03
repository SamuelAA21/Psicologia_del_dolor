using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrapper : MonoBehaviour
{
    [SerializeField] private string firstScene = GameSceneNames.MainMenu;

    private void Start()
    {
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadScene(firstScene);
            return;
        }

        if (!string.IsNullOrWhiteSpace(firstScene))
        {
            SceneManager.LoadScene(firstScene);
        }
    }
}
