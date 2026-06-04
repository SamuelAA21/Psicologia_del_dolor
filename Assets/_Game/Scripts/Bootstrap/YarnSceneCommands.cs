using UnityEngine;
using Yarn.Unity;

public class YarnSceneCommands : MonoBehaviour
{
    [YarnCommand("load_scene")]
    public void LoadScene(string sceneName)
    {
        SceneLoader.LoadSceneSafe(sceneName);
    }
}
