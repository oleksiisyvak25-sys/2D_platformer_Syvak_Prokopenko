using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private CanvasGroup gameOverCanvasGroup;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button menuButton;

    [Header("Fight References")]
    [SerializeField] private BossFightManager bossFightManager;

    [Header("Audio")]
    [SerializeField] private AudioClip deathSound;
    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartLevel);

        if (menuButton != null)
            menuButton.onClick.AddListener(LoadMainMenu);

        if (gameOverCanvasGroup != null)
        {
            gameOverCanvasGroup.alpha = 0f;
            gameOverCanvasGroup.interactable = false;
            gameOverCanvasGroup.blocksRaycasts = false;
        }
    }

    public void TriggerGameOver()
    {
        StartCoroutine(ShowGameOverRoutine());
    }

    private IEnumerator ShowGameOverRoutine()
    {
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        yield return new WaitForSeconds(0.5f);

        if (gameOverCanvasGroup != null)
        {
            float duration = 1.0f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                gameOverCanvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
                yield return null;
            }

            gameOverCanvasGroup.alpha = 1f;
            gameOverCanvasGroup.interactable = true;
            gameOverCanvasGroup.blocksRaycasts = true;
        }

        Time.timeScale = 0f;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;

        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.CloseImmediately();
        }

        if (gameOverCanvasGroup != null)
        {
            gameOverCanvasGroup.alpha = 0f;
            gameOverCanvasGroup.interactable = false;
            gameOverCanvasGroup.blocksRaycasts = false;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            if (RespawnManager.Instance != null)
            {
                player.transform.position = RespawnManager.Instance.GetRespawnPosition();
            }

            var movement = player.GetComponent<PlayerMovement>();
            if (movement != null) movement.enabled = true;

            var health = player.GetComponent<PlayerHealth>();
            if (health != null) health.ResetHealth();
        }

        ResetBossEncounter();
    }

    private void ResetBossEncounter()
    {
        if (bossFightManager == null)
        {
            bossFightManager = FindFirstObjectByType<BossFightManager>(FindObjectsInactive.Include);
        }

        if (RespawnManager.HasEnteredCastle)
        {
            if (bossFightManager != null)
            {
                Debug.Log("[GameOverManager] Скидаємо поточну хвилю бою після смерті гравця");
                bossFightManager.ResetAfterPlayerDeath();
            }
            else
            {
                Debug.LogError("[GameOverManager] Не вдалося знайти BossFightManager на сцені!");
            }
        }
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneLoader.Load("MainMenu");
    }
}