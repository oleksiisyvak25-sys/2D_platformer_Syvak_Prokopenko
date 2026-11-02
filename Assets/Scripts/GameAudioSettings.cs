using UnityEngine;

public static class GameAudioSettings
{
    private const string MasterKey = "master_volume";
    private const string MusicKey = "music_volume";

    public static float MasterVolume { get; private set; } = 1f;
    public static float MusicVolume { get; private set; } = 1f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Load()
    {
        MasterVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MasterKey, 1f));
        MusicVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MusicKey, 1f));
        AudioListener.volume = MasterVolume;
    }

    public static void SetMasterVolume(float value)
    {
        MasterVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(MasterKey, MasterVolume);
        AudioListener.volume = MasterVolume;
    }

    public static void SetMusicVolume(float value)
    {
        MusicVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(MusicKey, MusicVolume);

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.RefreshVolume();
        }
    }

    public static void Save()
    {
        PlayerPrefs.Save();
    }
}