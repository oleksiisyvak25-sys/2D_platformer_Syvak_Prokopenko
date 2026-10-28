using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private float minimumTime = 2f;
    [SerializeField] private string fallbackScene = "MainMenu";
    [SerializeField] private Slider progressBar;

    private IEnumerator Start()
    {
        Time.timeScale = 1f;

        string target = string.IsNullOrEmpty(SceneLoader.TargetScene)
            ? fallbackScene
            : SceneLoader.TargetScene;

        AsyncOperation op = SceneManager.LoadSceneAsync(target);
        op.allowSceneActivation = false;

        float elapsed = 0f;
        while (elapsed < minimumTime || op.progress < 0.9f)
        {
            elapsed += Time.unscaledDeltaTime;

            if (progressBar != null)
            {
                float timeProgress = Mathf.Clamp01(elapsed / minimumTime);
                float loadProgress = Mathf.Clamp01(op.progress / 0.9f);
                progressBar.value = Mathf.Min(timeProgress, loadProgress);
            }

            yield return null;
        }

        if (progressBar != null) progressBar.value = 1f;
        op.allowSceneActivation = true;
    }
}