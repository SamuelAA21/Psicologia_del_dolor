using UnityEngine;

public class Bootstrapper : MonoBehaviour
{
    [SerializeField] private string firstScene = "Interfaz";

    private void Start()
    {
        SceneLoader.Instance.LoadScene(firstScene);
    }
}
