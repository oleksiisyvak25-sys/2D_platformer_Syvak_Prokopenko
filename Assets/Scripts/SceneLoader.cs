using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public const string LoadingSceneName = "Loading";

    public static string TargetScene { get; private set; }

    public static void Load(string sceneName)
    {
        if (sceneName == LoadingSceneName) return;

        TargetScene = sceneName;
        SceneManager.LoadScene(LoadingSceneName);
    }

    public static void LoadByIndex(int buildIndex)
    {
        string path = SceneUtility.GetScenePathByBuildIndex(buildIndex);
        Load(Path.GetFileNameWithoutExtension(path));
    }
}