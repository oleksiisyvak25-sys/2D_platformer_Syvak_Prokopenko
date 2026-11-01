using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class LevelExitDoor : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject interactPrompt;
    [SerializeField] private CanvasGroup victoryScreenGroup;

    [Header("Audio")]
    [SerializeField] private AudioClip victorySound;
    private AudioSource audioSource;

    [Header("After victory")]
    [SerializeField] private float returnDelay = 10f;
    [SerializeField] private string sceneAfterVictory = "MainMenu";
    [SerializeField] private float musicFadeOut = 0.6f;

    private bool isPlayerNearby = false;
    private bool isActivated = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;

        if (interactPrompt != null) interactPrompt.SetActive(false);
        if (victoryScreenGroup != null)
        {
            victoryScreenGroup.gameObject.SetActive(false);
            victoryScreenGroup.alpha = 0f;
        }
    }

    private void Update()
    {
        if (isPlayerNearby && !isActivated && BossFightManager.IsBossDefeated && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(FinishLevelRoutine());
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && BossFightManager.IsBossDefeated && !isActivated)
        {
            isPlayerNearby = true;
            if (interactPrompt != null) interactPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isActivated)
        {
            isPlayerNearby = false;
            if (interactPrompt != null) interactPrompt.SetActive(false);
        }
    }

    private IEnumerator FinishLevelRoutine()
    {
        isActivated = true;
        isPlayerNearby = false;

        if (interactPrompt != null) 
            interactPrompt.SetActive(false);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var movement = player.GetComponent<PlayerMovement>();
            if (movement != null) movement.enabled = false;

            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.FadeOutAndStop(musicFadeOut);
        }

        float startTime = Time.time;

        if (victorySound != null && audioSource != null)
        {
            audioSource.PlayOneShot(victorySound);
        }

        if (victoryScreenGroup != null)
        {
            victoryScreenGroup.gameObject.SetActive(true);
            float duration = 1.2f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                victoryScreenGroup.alpha = Mathf.Clamp01(elapsed / duration);
                yield return null;
            }
            victoryScreenGroup.alpha = 1f;
        }

        float remaining = returnDelay - (Time.time - startTime);
        if (remaining > 0f)
        {
            yield return new WaitForSecondsRealtime(remaining);
        }

        Time.timeScale = 1f;

        if (!string.IsNullOrEmpty(sceneAfterVictory))
        {
            SceneLoader.Load(sceneAfterVictory);
        }
    }
}