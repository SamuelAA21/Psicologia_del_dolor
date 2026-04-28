using System.Collections;
using UnityEngine;
using Yarn.Unity;

public class MiniGameBridge : MonoBehaviour
{
    [SerializeField] private string miniGameSceneName = "FirstMiniGame";

    [YarnCommand("iniciar_respiracion")]
    public void IniciarRespiracion()
    {
        DialogueRunner dialogueRunner = FindFirstObjectByType<DialogueRunner>();
        if (dialogueRunner != null)
        {
            dialogueRunner.Stop();
        }

        StartCoroutine(LoadMiniGameNextFrame());
    }

    private IEnumerator LoadMiniGameNextFrame()
    {
        yield return null;
        SceneLoader.LoadSceneSafe(miniGameSceneName);
    }
}
