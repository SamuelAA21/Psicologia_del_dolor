using System.Collections;
using UnityEngine;
using Yarn.Unity;

public class MiniGameBridge : MonoBehaviour
{
    [SerializeField] private string miniGameSceneName = GameSceneNames.BreathingMiniGame;

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
