using UnityEngine;

public class GameAudioManager : MonoBehaviour
{
    public static GameAudioManager Instance { get; private set; }

    [Header("Volumen")]
    [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.42f;
    [SerializeField, Range(0f, 1f)] private float uiVolume = 0.78f;
    [SerializeField, Range(0f, 1f)] private float feedbackVolume = 0.85f;

    [Header("Clips")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip gameplayMusic;
    [SerializeField] private AudioClip uiClickClip;
    [SerializeField] private AudioClip rewardClip;
    [SerializeField] private AudioClip errorClip;
    [SerializeField] private AudioClip transitionClip;

    [Header("Recursos por defecto")]
    [SerializeField] private string menuMusicResource = "Audio/MenuMusic";
    [SerializeField] private string gameplayMusicResource = "Audio/GameplayMusic";
    [SerializeField] private string uiClickResource = "Audio/UiClick";
    [SerializeField] private string rewardResource = "Audio/Reward";
    [SerializeField] private string errorResource = "Audio/Error";
    [SerializeField] private string transitionResource = "Audio/Transition";

    private AudioSource musicSource;
    private AudioSource uiSource;
    private AudioSource feedbackSource;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureInstance()
    {
        if (Instance != null)
        {
            return;
        }

        GameObject audioObject = new GameObject("GameAudioManager");
        audioObject.AddComponent<GameAudioManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadMissingClipsFromResources();
        EnsureSources();
        ApplyVolumes();
    }

    public static void PlayUiClick()
    {
        Instance?.PlayOneShot(Instance.uiSource, Instance.uiClickClip, Instance.uiVolume);
    }

    public static void PlayReward()
    {
        Instance?.PlayOneShot(Instance.feedbackSource, Instance.rewardClip, Instance.feedbackVolume);
    }

    public static void PlayError()
    {
        Instance?.PlayOneShot(Instance.feedbackSource, Instance.errorClip, Instance.feedbackVolume);
    }

    public static void PlayTransition()
    {
        Instance?.PlayOneShot(Instance.uiSource, Instance.transitionClip, Instance.uiVolume);
    }

    public static void PlayMenuMusic()
    {
        Instance?.PlayMusic(Instance.menuMusic);
    }

    public static void PlayGameplayMusic()
    {
        Instance?.PlayMusic(Instance.gameplayMusic);
    }

    public void SetMasterVolume(float value)
    {
        masterVolume = Mathf.Clamp01(value);
        ApplyVolumes();
    }

    private void EnsureSources()
    {
        musicSource = EnsureSource("MusicSource", true);
        uiSource = EnsureSource("UiSource", false);
        feedbackSource = EnsureSource("FeedbackSource", false);
    }

    private void LoadMissingClipsFromResources()
    {
        menuMusic = LoadIfMissing(menuMusic, menuMusicResource);
        gameplayMusic = LoadIfMissing(gameplayMusic, gameplayMusicResource);
        uiClickClip = LoadIfMissing(uiClickClip, uiClickResource);
        rewardClip = LoadIfMissing(rewardClip, rewardResource);
        errorClip = LoadIfMissing(errorClip, errorResource);
        transitionClip = LoadIfMissing(transitionClip, transitionResource);
    }

    private static AudioClip LoadIfMissing(AudioClip currentClip, string resourcePath)
    {
        if (currentClip != null || string.IsNullOrWhiteSpace(resourcePath))
        {
            return currentClip;
        }

        return Resources.Load<AudioClip>(resourcePath);
    }

    private AudioSource EnsureSource(string objectName, bool loop)
    {
        Transform existing = transform.Find(objectName);
        GameObject sourceObject = existing != null ? existing.gameObject : new GameObject(objectName);
        sourceObject.transform.SetParent(transform, false);

        AudioSource source = sourceObject.GetComponent<AudioSource>();
        if (source == null)
        {
            source = sourceObject.AddComponent<AudioSource>();
        }

        source.playOnAwake = false;
        source.loop = loop;
        return source;
    }

    private void PlayMusic(AudioClip clip)
    {
        if (musicSource == null || clip == null || musicSource.clip == clip)
        {
            return;
        }

        musicSource.clip = clip;
        musicSource.volume = musicVolume * masterVolume;
        musicSource.Play();
    }

    private void PlayOneShot(AudioSource source, AudioClip clip, float volume)
    {
        if (source == null || clip == null)
        {
            return;
        }

        source.PlayOneShot(clip, volume * masterVolume);
    }

    private void ApplyVolumes()
    {
        if (musicSource != null)
        {
            musicSource.volume = musicVolume * masterVolume;
        }

        if (uiSource != null)
        {
            uiSource.volume = uiVolume * masterVolume;
        }

        if (feedbackSource != null)
        {
            feedbackSource.volume = feedbackVolume * masterVolume;
        }
    }
}
