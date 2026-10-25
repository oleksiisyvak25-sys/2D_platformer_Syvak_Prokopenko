using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class BatMovement : MonoBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] private float speed = 3f;
    [SerializeField] private float travelDistance = 5f;
    [SerializeField] private bool startMovingRight = true;

    [Header("Bounce & Defeat")]
    [SerializeField] private float playerBounceForce = 10f;
    [SerializeField] private GameObject deathEffect;

    private Rigidbody2D rb;
    private Collider2D col;
    private Vector2 startPos;
    private bool movingRight;
    private bool isDead = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void Start()
    {
        startPos = transform.position;
        movingRight = startMovingRight;
        UpdateFacingDirection();
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        float currentSpeed = movingRight ? speed : -speed;
        rb.linearVelocity = new Vector2(currentSpeed, 0f);

        if (movingRight && transform.position.x >= startPos.x + travelDistance)
        {
            movingRight = false;
            UpdateFacingDirection();
        }
        else if (!movingRight && transform.position.x <= startPos.x - travelDistance)
        {
            movingRight = true;
            UpdateFacingDirection();
        }
    }

    private void UpdateFacingDirection()
    {
        Vector3 scale = transform.localScale;
        scale.x = movingRight ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            Vector2 contactNormal = collision.GetContact(0).normal;

            if (contactNormal.y < -0.4f)
            {
                var player = collision.gameObject.GetComponent<PlayerMovement>();
                if (player != null)
                {
                    player.Bounce(playerBounceForce);
                }
                Die();
            }
            else
            {
                var playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    Vector2 knockback = (collision.transform.position - transform.position).normalized;
                    playerHealth.TakeDamage(1, knockback);
                }
            }
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        col.enabled = false;
        rb.linearVelocity = Vector2.zero;

        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        var spawnable = GetComponent<SpawnableEnemy>();
        if (spawnable != null)
        {
            spawnable.NotifyKilled();
        }

        Destroy(gameObject);
    }
}