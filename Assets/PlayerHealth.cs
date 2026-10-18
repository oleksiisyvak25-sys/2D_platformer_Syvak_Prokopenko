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

    [SerializeField] private Image[] heartImages;
    [SerializeField] private Sprite fullHeartSprite;
    [SerializeField] private Sprite emptyHeartSprite;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;

    private void Start()
    {
        currentHealth = maxHealth;
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();

        UpdateHeartsUI();
    }

    public void TakeDamage(int damage, Vector2 knockbackDirection)
    {
        if (isInvulnerable || currentHealth <= 0) return;

        currentHealth -= damage;
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
        Debug.Log("Player died!");
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }
}