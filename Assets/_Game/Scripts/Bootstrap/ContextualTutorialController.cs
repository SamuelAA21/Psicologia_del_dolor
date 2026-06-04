using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Yarn.Unity;

public class ContextualTutorialController : MonoBehaviour
{
    private const string TutorialShownKey = "Chapter1_ContextualTutorial_Shown";

    [SerializeField] private bool showOnlyOnce;
    [SerializeField] private float hintSeconds = 4.8f;
    [SerializeField] private int sortingOrder = 120;
    [SerializeField] private Canvas tutorialCanvas;
    [SerializeField] private Text hintText;
    [SerializeField] private DialogueRunner dialogueRunner;

    private readonly string[] hints =
    {
        "WASD para moverte por el mapa.",
        "Sigue la esfera azul para encontrar el objetivo.",
        "Presiona E cerca de puertas o personajes para interactuar.",
        "Usa 1-9 o la rueda del mouse para seleccionar objetos del inventario.",
        "Presiona Esc para pausar o volver al menu."
    };

    private Coroutine tutorialRoutine;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        EnsureForScene(SceneManager.GetActiveScene());
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureForScene(scene);
    }

    private static void EnsureForScene(Scene scene)
    {
        if (!IsGameplayScene(scene.name))
        {
            return;
        }

        if (FindAnyObjectByType<ContextualTutorialController>(FindObjectsInactive.Include) != null)
        {
            return;
        }

        GameObject tutorialObject = new GameObject("ContextualTutorialController");
        tutorialObject.AddComponent<ContextualTutorialController>();
    }

    private static bool IsGameplayScene(string sceneName)
    {
        return GameSceneNames.IsMainGame(sceneName);
    }

    private void Awake()
    {
        EnsureUi();
        HideHint();
    }

    private void Start()
    {
        if (showOnlyOnce && PlayerPrefs.GetInt(TutorialShownKey, 0) == 1)
        {
            return;
        }

        StopTutorial();
        tutorialRoutine = StartCoroutine(PlayTutorial());
    }

    private void OnDisable()
    {
        StopTutorial();
        HideHint();
    }

    public static void ResetTutorial()
    {
        PlayerPrefs.DeleteKey(TutorialShownKey);
        PlayerPrefs.Save();
    }

    private IEnumerator PlayTutorial()
    {
        yield return new WaitForSecondsRealtime(0.8f);

        for (int i = 0; i < hints.Length; i++)
        {
            yield return WaitUntilDialogueAllowsHints();
            ShowHint(hints[i]);
            yield return new WaitForSecondsRealtime(hintSeconds);
            HideHint();
            yield return new WaitForSecondsRealtime(0.35f);
        }

        PlayerPrefs.SetInt(TutorialShownKey, 1);
        PlayerPrefs.Save();
        tutorialRoutine = null;
    }

    private IEnumerator WaitUntilDialogueAllowsHints()
    {
        while (IsDialogueRunning())
        {
            HideHint();
            yield return null;
        }
    }

    private bool IsDialogueRunning()
    {
        if (dialogueRunner == null)
        {
            dialogueRunner = FindAnyObjectByType<DialogueRunner>(FindObjectsInactive.Include);
        }

        return dialogueRunner != null && dialogueRunner.IsDialogueRunning;
    }

    private void ShowHint(string message)
    {
        if (hintText != null)
        {
            hintText.text = message;
        }

        if (tutorialCanvas != null)
        {
            tutorialCanvas.gameObject.SetActive(true);
        }
    }

    private void HideHint()
    {
        if (tutorialCanvas != null)
        {
            tutorialCanvas.gameObject.SetActive(false);
        }
    }

    private void EnsureUi()
    {
        if (tutorialCanvas != null && hintText != null)
        {
            return;
        }

        GameObject canvasObject = new GameObject("ContextualTutorialCanvas");
        tutorialCanvas = canvasObject.AddComponent<Canvas>();
        tutorialCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        tutorialCanvas.sortingOrder = sortingOrder;

        RuntimeUiUtility.EnsureCanvasScaler(canvasObject, new Vector2(1920f, 1080f));

        GameObject panel = new GameObject("HintPanel");
        panel.transform.SetParent(canvasObject.transform, false);

        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.02f, 0.05f, 0.06f, 0.82f);

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 0f);
        panelRect.anchorMax = new Vector2(0f, 0f);
        panelRect.pivot = new Vector2(0f, 0f);
        panelRect.anchoredPosition = new Vector2(28f, 118f);
        panelRect.sizeDelta = new Vector2(520f, 66f);

        GameObject textObject = new GameObject("HintText");
        textObject.transform.SetParent(panel.transform, false);
        hintText = textObject.AddComponent<Text>();
        hintText.font = RuntimeUiUtility.DefaultFont;
        hintText.fontSize = 19;
        hintText.fontStyle = FontStyle.Bold;
        hintText.alignment = TextAnchor.MiddleLeft;
        hintText.color = Color.white;
        hintText.horizontalOverflow = HorizontalWrapMode.Wrap;
        hintText.verticalOverflow = VerticalWrapMode.Truncate;

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(20f, 10f);
        textRect.offsetMax = new Vector2(-20f, -10f);
    }

    private void StopTutorial()
    {
        if (tutorialRoutine == null)
        {
            return;
        }

        StopCoroutine(tutorialRoutine);
        tutorialRoutine = null;
    }
}
