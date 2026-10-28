using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    [Serializable]
    public class SceneMusic
    {
        public string sceneName;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 0.4f;
    }

    public static MusicManager Instance { get; private set; }

    [SerializeField] private SceneMusic[] scenes;
    [SerializeField] private float fadeTime = 1f;

    private AudioSource _source;
    private Coroutine _routine;
    private AudioClip _currentClip;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Instance = null;
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

        _source = GetComponent<AudioSource>();
        _source.loop = true;
        _source.playOnAwake = false;
        _source.volume = 0f;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        if (Instance != this) return;

        ApplyScene(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyScene(scene.name);
    }

    public void FadeOutAndStop(float duration = 0.6f)
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(StopRoutine(duration));
    }

    private IEnumerator StopRoutine(float duration)
    {
        if (_source.isPlaying && duration > 0f)
        {
            float startVolume = _source.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                _source.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                yield return null;
            }
        }

        _source.Stop();
        _source.volume = 0f;
        _source.clip = null;
        _currentClip = null;
    }

    private void ApplyScene(string sceneName)
    {
        if (scenes == null) return;

        foreach (var entry in scenes)
        {
            if (entry.sceneName == sceneName)
            {
                Play(entry);
                return;
            }
        }
    }

    private void Play(SceneMusic entry)
    {
        if (_routine != null) StopCoroutine(_routine);

        if (entry.clip != null && entry.clip == _currentClip)
        {
            _routine = StartCoroutine(Fade(_source.volume, entry.volume));
            return;
        }

        _routine = StartCoroutine(SwitchRoutine(entry.clip, entry.volume));
    }

    private IEnumerator SwitchRoutine(AudioClip clip, float targetVolume)
    {
        if (_source.isPlaying)
        {
            yield return Fade(_source.volume, 0f);
        }

        _source.Stop();
        _currentClip = clip;
        _source.clip = clip;

        if (clip == null) yield break;

        _source.Play();
        yield return Fade(0f, targetVolume);
    }

    private IEnumerator Fade(float from, float to)
    {
        if (fadeTime <= 0f)
        {
            _source.volume = to;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.unscaledDeltaTime;
            _source.volume = Mathf.Lerp(from, to, elapsed / fadeTime);
            yield return null;
        }

        _source.volume = to;
    }
}