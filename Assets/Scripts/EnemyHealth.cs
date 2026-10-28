using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 1;
    [SerializeField] private float bounceOffForce = 8.5f;
    [SerializeField] private GameObject deathEffect;

    [Header("Death animation (Mario style)")]
    [Tooltip("Death sprite")]
    [SerializeField] private Sprite deathSprite;
    [Tooltip("Turn the sprite upsite down")]
    [SerializeField] private bool flipUpsideDown = true;
    [Tooltip("Jump")]
    [SerializeField] private float deathHopForce = 7f;
    [Tooltip("Horizontal speed")]
    [SerializeField] private float deathHorizontalSpeed = 2f;
    [Tooltip("Rotate speed")]
    [SerializeField] private float deathSpinSpeed = 0f;
    [SerializeField] private float deathGravityScale = 3f;
    [Tooltip("Disepear after:")]
    [SerializeField] private float deathLifetime = 1.5f;
    [Tooltip("Order in Layer")]
    [SerializeField] private int deathSortingBoost = 10;

    private int currentHealth;
    private bool isDead;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        TakeDamage(damage, transform.position);
    }

    public void TakeDamage(int damage, Vector2 hitFrom)
    {
        if (isDead) return;

        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die(hitFrom);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            Vector2 hitDirection = (collision.transform.position - transform.position).normalized;

            if (hitDirection.y > 0.4f)
            {
                var player = collision.gameObject.GetComponent<PlayerMovement>();
                if (player != null)
                {
                    player.Bounce(bounceOffForce);
                }
                TakeDamage(1, collision.transform.position);
            }
            else
            {
                var playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(1, hitDirection);
                }
            }
        }
    }

    private void Die(Vector2 hitFrom)
    {
        if (isDead) return;
        isDead = true;

        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        foreach (var behaviour in GetComponents<MonoBehaviour>())
        {
            if (behaviour != this) behaviour.enabled = false;
        }
        foreach (var col in GetComponentsInChildren<Collider2D>())
        {
            col.enabled = false;
        }

        var sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            if (deathSprite != null)
            {
                var animator = GetComponent<Animator>();
                if (animator != null) animator.enabled = false;
                sr.sprite = deathSprite;
            }

            if (flipUpsideDown) sr.flipY = !sr.flipY;
            sr.sortingOrder += deathSortingBoost;
        }

        var rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();

        float dx = transform.position.x - hitFrom.x;
        float dir = Mathf.Abs(dx) < 0.01f ? (Random.value < 0.5f ? -1f : 1f) : Mathf.Sign(dx);

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.simulated = true;
        rb.constraints = RigidbodyConstraints2D.None;
        rb.linearDamping = 0f;
        rb.gravityScale = deathGravityScale;
        rb.rotation = 0f;
        rb.linearVelocity = new Vector2(dir * deathHorizontalSpeed, deathHopForce);
        rb.angularVelocity = -dir * deathSpinSpeed;

        StartCoroutine(DeathRoutine(sr));
    }

    private IEnumerator DeathRoutine(SpriteRenderer sr)
    {
        float fade = Mathf.Min(0.3f, deathLifetime);
        yield return new WaitForSeconds(deathLifetime - fade);

        float t = 0f;
        while (t < fade)
        {
            t += Time.deltaTime;
            if (sr != null)
            {
                Color c = sr.color;
                c.a = 1f - Mathf.Clamp01(t / fade);
                sr.color = c;
            }
            yield return null;
        }

        Destroy(gameObject);
    }
}