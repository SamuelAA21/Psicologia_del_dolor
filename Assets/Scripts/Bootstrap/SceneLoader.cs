using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [SerializeField] private string mainSceneName = "SampleScene";
    [SerializeField] private float fadeDuration = 0.35f;
    [SerializeField]
    private string[] loadingPhrases =
    {
        "Respira. Observa. Decide.",
        "Cada puerta abre una forma distinta de mirar el dolor.",
        "Avanza con calma: el mapa responde a tus decisiones.",
        "La brujula marca direccion, no destino."
    };

    private string currentScene;
    private Coroutine activeLoadRoutine;
    private Canvas fadeCanvas;
    private Image fadeImage;
    private Text loadingText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("SceneLoader received an empty scene name.");
            return;
        }

        if (activeLoadRoutine != null)
        {
            StopCoroutine(activeLoadRoutine);
        }

        activeLoadRoutine = StartCoroutine(LoadRoutine(sceneName));
    }

    public void LoadMainScene()
    {
        LoadScene(mainSceneName);
    }

    public static void LoadSceneSafe(string sceneName)
    {
        if (Instance != null)
        {
            Instance.LoadScene(sceneName);
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    public static void LoadMainSceneSafe()
    {
        if (Instance != null)
        {
            Instance.LoadMainScene();
            return;
        }

        SceneManager.LoadScene("SampleScene");
    }

    private IEnumerator LoadRoutine(string sceneName)
    {
        EnsureFadeCanvas();
        SetLoadingPhrase(sceneName);
        GameAudioManager.PlayTransition();
        yield return Fade(1f);

        Time.timeScale = 1f;
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        while (loadOperation != null && !loadOperation.isDone)
        {
            yield return null;
        }

        currentScene = sceneName;
        PlaySceneMusic(sceneName);
        yield return Fade(0f);
        activeLoadRoutine = null;
    }

    private void EnsureFadeCanvas()
    {
        if (fadeCanvas != null && fadeImage != null)
        {
            return;
        }

        GameObject canvasObject = new GameObject("SceneTransitionCanvas");
        DontDestroyOnLoad(canvasObject);

        fadeCanvas = canvasObject.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 1000;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject imageObject = new GameObject("Fade");
        imageObject.transform.SetParent(canvasObject.transform, false);
        fadeImage = imageObject.AddComponent<Image>();
        fadeImage.color = new Color(0f, 0f, 0f, 0f);
        fadeImage.raycastTarget = true;

        RectTransform imageRect = imageObject.GetComponent<RectTransform>();
        imageRect.anchorMin = Vector2.zero;
        imageRect.anchorMax = Vector2.one;
        imageRect.offsetMin = Vector2.zero;
        imageRect.offsetMax = Vector2.zero;

        GameObject textObject = new GameObject("LoadingPhrase");
        textObject.transform.SetParent(canvasObject.transform, false);
        loadingText = textObject.AddComponent<Text>();
        loadingText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        loadingText.fontSize = 26;
        loadingText.fontStyle = FontStyle.Bold;
        loadingText.alignment = TextAnchor.MiddleCenter;
        loadingText.color = new Color(0.86f, 0.97f, 1f, 0f);
        loadingText.horizontalOverflow = HorizontalWrapMode.Wrap;
        loadingText.verticalOverflow = VerticalWrapMode.Truncate;

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = new Vector2(0f, -285f);
        textRect.sizeDelta = new Vector2(900f, 72f);

        fadeCanvas.gameObject.SetActive(false);
    }

    private IEnumerator Fade(float targetAlpha)
    {
        if (fadeImage == null)
        {
            yield break;
        }

        fadeCanvas.gameObject.SetActive(true);

        Color color = fadeImage.color;
        float startAlpha = color.a;
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, fadeDuration);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            color.a = Mathf.Lerp(startAlpha, targetAlpha, t);
            fadeImage.color = color;
            SetLoadingTextAlpha(color.a);
            yield return null;
        }

        color.a = targetAlpha;
        fadeImage.color = color;
        SetLoadingTextAlpha(targetAlpha);
        fadeCanvas.gameObject.SetActive(targetAlpha > 0.001f);
    }

    private void SetLoadingPhrase(string sceneName)
    {
        if (loadingText == null || loadingPhrases == null || loadingPhrases.Length == 0)
        {
            return;
        }

        int index = Mathf.Abs(sceneName.GetHashCode()) % loadingPhrases.Length;
        loadingText.text = loadingPhrases[index];
    }

    private void SetLoadingTextAlpha(float alpha)
    {
        if (loadingText == null)
        {
            return;
        }

        Color color = loadingText.color;
        color.a = Mathf.Clamp01(alpha);
        loadingText.color = color;
    }

    private static void PlaySceneMusic(string sceneName)
    {
        if (sceneName.Equals("Interfaz", System.StringComparison.OrdinalIgnoreCase))
        {
            GameAudioManager.PlayMenuMusic();
            return;
        }

        GameAudioManager.PlayGameplayMusic();
    }
}
