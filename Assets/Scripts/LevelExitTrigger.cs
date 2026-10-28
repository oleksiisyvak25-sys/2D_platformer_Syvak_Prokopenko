using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class LevelExitTrigger : MonoBehaviour
{
    [Header("Куди переходити")]
    [SerializeField] private string sceneName = "";

    [Header("Затримка перед переходом (сек)")]
    [SerializeField] private float delay = 0f;

    private bool _triggered;

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_triggered || !IsPlayer(other)) return;

        _triggered = true;
        Invoke(nameof(LoadNext), delay);
    }

    private void LoadNext()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneLoader.Load(sceneName);
            return;
        }

        int next = SceneManager.GetActiveScene().buildIndex + 1;
        if (next < SceneManager.sceneCountInBuildSettings)
        {
            SceneLoader.LoadByIndex(next);
        }
        else
        {
            Debug.LogWarning("LevelExitTrigger: наступної сцени немає в Build Settings.", this);
            _triggered = false;
        }
    }

    private static bool IsPlayer(Collider2D c)
    {
        return c.CompareTag("Player") ||
               (c.attachedRigidbody != null && c.attachedRigidbody.CompareTag("Player"));
    }
}