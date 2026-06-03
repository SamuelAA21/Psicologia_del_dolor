using System.Collections;
using UnityEngine;

public class GameAudioManager : MonoBehaviour
{
    public static GameAudioManager Instance { get; private set; }
    private const string MusicVolumeKey = "GameAudioManager_MusicVolume";

    [Header("Volumen")]
    [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.42f;
    [SerializeField, Range(0f, 1f)] private float uiVolume = 0.78f;
    [SerializeField, Range(0f, 1f)] private float feedbackVolume = 0.85f;

    [Header("Clips")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip gameplayMusic;
    [SerializeField] private AudioClip gameplayAccentMusic;
    [SerializeField] private AudioClip uiClickClip;
    [SerializeField] private AudioClip rewardClip;
    [SerializeField] private AudioClip errorClip;
    [SerializeField] private AudioClip transitionClip;

    [Header("Recursos por defecto")]
    [SerializeField] private string menuMusicResource = "Audio/MenuMusic";
    [SerializeField] private string gameplayMusicResource = "AmbientalLoop";
    [SerializeField] private string gameplayAccentMusicResource = "Audio/GameplayMusic";
    [SerializeField] private string uiClickResource = "Audio/UiClick";
    [SerializeField] private string rewardResource = "Audio/Reward";
    [SerializeField] private string errorResource = "Audio/Error";
    [SerializeField] private string transitionResource = "Audio/Transition";

    private AudioSource musicSource;
    private AudioSource uiSource;
    private AudioSource feedbackSource;
    private Coroutine gameplayMusicRoutine;

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
        musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, musicVolume);
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
        if (Instance == null)
        {
            return;
        }

        Instance.StopGameplayMusicRoutine();
        Instance.PlayMusic(Instance.menuMusic, true);
    }

    public static void PlayGameplayMusic()
    {
        Instance?.StartGameplayMusicRoutine();
    }

    public void SetMasterVolume(float value)
    {
        masterVolume = Mathf.Clamp01(value);
        ApplyVolumes();
    }

    public static float MusicVolume => Instance != null ? Instance.musicVolume : 0f;

    public static void AdjustMusicVolume(float delta)
    {
        if (Instance == null)
        {
            return;
        }

        Instance.SetMusicVolume(Instance.musicVolume + delta);
    }

    public static void SetMusicVolume01(float value)
    {
        Instance?.SetMusicVolume(value);
    }

    private void EnsureSources()
    {
        musicSource = EnsureSource("MusicSource", true);
        uiSource = EnsureSource("UiSource", false);
        feedbackSource = EnsureSource("FeedbackSource", false);
    }

    private void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(MusicVolumeKey, musicVolume);
        PlayerPrefs.Save();
        ApplyVolumes();
    }

    private void LoadMissingClipsFromResources()
    {
        menuMusic = LoadIfMissing(menuMusic, menuMusicResource);
        gameplayMusic = LoadIfMissing(gameplayMusic, gameplayMusicResource);
        gameplayAccentMusic = LoadIfMissing(gameplayAccentMusic, gameplayAccentMusicResource);
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

    private void StartGameplayMusicRoutine()
    {
        if (gameplayMusicRoutine != null)
        {
            return;
        }

        if (gameplayMusic == null)
        {
            PlayMusic(gameplayAccentMusic, true);
            return;
        }

        gameplayMusicRoutine = StartCoroutine(PlayGameplayMusicCycle());
    }

    private void StopGameplayMusicRoutine()
    {
        if (gameplayMusicRoutine == null)
        {
            return;
        }

        StopCoroutine(gameplayMusicRoutine);
        gameplayMusicRoutine = null;
    }

    private IEnumerator PlayGameplayMusicCycle()
    {
        while (true)
        {
            PlayMusic(gameplayMusic, true);
            yield return new WaitForSecondsRealtime(60f);

            if (gameplayAccentMusic != null)
            {
                PlayMusic(gameplayAccentMusic, true);
                yield return new WaitForSecondsRealtime(45f);
            }
        }
    }

    private void PlayMusic(AudioClip clip, bool loop)
    {
        if (musicSource == null || clip == null)
        {
            return;
        }

        bool clipChanged = musicSource.clip != clip;
        if (clipChanged)
        {
            musicSource.clip = clip;
        }

        musicSource.loop = loop;
        musicSource.volume = musicVolume * masterVolume;

        if (clipChanged || !musicSource.isPlaying)
        {
            musicSource.Play();
        }
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
