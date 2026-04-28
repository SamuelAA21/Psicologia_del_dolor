using UnityEngine;

public class Bootstrapper : MonoBehaviour
{
    [SerializeField] private string firstScene = "SampleScene";

    private void Start()
    {
        SceneLoader.Instance.LoadScene(firstScene);
    }
}