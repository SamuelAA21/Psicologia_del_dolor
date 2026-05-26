using System.Collections;
using UnityEngine;
using Yarn.Unity;

public class MiniGameBridge : MonoBehaviour
{
    [SerializeField] private string miniGameSceneName = "FirstMiniGame";

    [YarnCommand("iniciar_respiracion")]
    public void IniciarRespiracion()
    {
        StartCoroutine(LoadMiniGameNextFrame());
    }

    private IEnumerator LoadMiniGameNextFrame()
    {
        yield return null;
        SceneLoader.LoadSceneSafe(miniGameSceneName);
    }
}
