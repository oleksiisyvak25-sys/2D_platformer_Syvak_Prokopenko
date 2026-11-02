using UnityEngine;
using UnityEngine.SceneManagement;

public static class LevelProgress
{
    private const string KeyPrefix = "level_reached_";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu" || scene.name == SceneLoader.LoadingSceneName) return;

        MarkReached(scene.name);
    }

    public static void MarkReached(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return;

        PlayerPrefs.SetInt(KeyPrefix + sceneName, 1);
        PlayerPrefs.Save();
    }

    public static bool IsReached(string sceneName)
    {
        return PlayerPrefs.GetInt(KeyPrefix + sceneName, 0) == 1;
    }
}