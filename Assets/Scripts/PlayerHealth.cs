using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    [SerializeField] private float knockbackForce = 7f;
    [SerializeField] private float invulnerabilityDuration = 1f;
    private bool isInvulnerable = false;
    private bool isDead = false;

    [SerializeField] private Image[] heartImages;
    [SerializeField] private Sprite fullHeartSprite;
    [SerializeField] private Sprite emptyHeartSprite;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;

    [Header("Audio")]
    [SerializeField] private AudioClip hurtSound;
    private AudioSource playerAudio;

    private void Start()
    {
        currentHealth = maxHealth;
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();
        playerAudio = GetComponent<AudioSource>();
        if (playerAudio == null)
        {
            playerAudio = gameObject.AddComponent<AudioSource>();
            playerAudio.playOnAwake = false;
        }

        UpdateHeartsUI();
    }

    public void TakeDamage(int damage, Vector2 knockbackDirection)
    {
        if (isInvulnerable || currentHealth <= 0) return;

        currentHealth -= damage;
        if (hurtSound != null && playerAudio != null)
        {
            playerAudio.PlayOneShot(hurtSound);
        }
        UpdateHeartsUI();

        if (animator != null)
        {
            animator.SetTrigger("Hurt");
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            Vector2 force = new Vector2(knockbackDirection.x * knockbackForce, knockbackForce * 0.7f);
            rb.AddForce(force, ForceMode2D.Impulse);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvulnerabilityRoutine());
        }
    }

    private void UpdateHeartsUI()
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] == null) continue;

            if (i < currentHealth)
                heartImages[i].sprite = fullHeartSprite;
            else if (emptyHeartSprite != null)
                heartImages[i].sprite = emptyHeartSprite;
            else
                heartImages[i].enabled = false;
        }
    }

    private IEnumerator InvulnerabilityRoutine()
    {
        isInvulnerable = true;

        float elapsed = 0f;
        while (elapsed < invulnerabilityDuration)
        {
            spriteRenderer.color = new Color(1f, 1f, 1f, 0.3f);
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.2f;
        }

        spriteRenderer.color = Color.white;
        isInvulnerable = false;
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        var movement = GetComponent<PlayerMovement>();
        if (movement != null) 
            movement.enabled = false;

        if (GameOverManager.Instance != null)
        {
            GameOverManager.Instance.TriggerGameOver();
        }
    }

    public void Kill()
    {
        currentHealth = 0;
        UpdateHeartsUI();
        Die();
    }

    public void ResetHealth()
    {
        isDead = false;
        currentHealth = maxHealth;

        if (heartImages != null)
        {
            for (int i = 0; i < heartImages.Length; i++)
            {
                if (heartImages[i] != null && fullHeartSprite != null)
                {
                    heartImages[i].sprite = fullHeartSprite;
                    heartImages[i].enabled = true;
                }
            }
        }
    }
}