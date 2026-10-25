using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class SlimeEnemy : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 2.2f;
    [SerializeField] private bool movingRight = false;

    [Header("Edge & Wall Detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private float wallCheckDistance = 0.35f;
    [SerializeField] private float groundCheckDistance = 0.4f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;

    [Header("Bounce & Defeat")]
    [SerializeField] private float playerBounceForce = 9.5f;
    [SerializeField] private GameObject deathEffect;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private Rigidbody2D rb;
    private Collider2D col;
    private bool isDead = false;
    private float flipCooldown = 0f;

    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    private static readonly int DieTriggerHash = Animator.StringToHash("Die");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        if (animator == null)
            animator = GetComponent<Animator>();

        Physics2D.queriesStartInColliders = false;
    }

    private void Start()
    {
        UpdateFacingDirection();
    }

    private void Update()
    {
        if (flipCooldown > 0f)
        {
            flipCooldown -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        Patrol();
    }

    private void Patrol()
    {
        float currentSpeed = movingRight ? speed : -speed;
        rb.linearVelocity = new Vector2(currentSpeed, rb.linearVelocity.y);

        if (animator != null)
        {
            animator.SetBool(IsMovingHash, Mathf.Abs(rb.linearVelocity.x) > 0.05f);
        }

        if (flipCooldown > 0f) return;

        Vector2 moveDirection = movingRight ? Vector2.right : Vector2.left;

        Vector2 wallOrigin;
        if (wallCheck != null)
        {
            float forwardOffset = Mathf.Abs(wallCheck.localPosition.x);
            wallOrigin = new Vector2(transform.position.x + (movingRight ? forwardOffset : -forwardOffset), wallCheck.position.y);
        }
        else
        {
            wallOrigin = transform.position;
        }

        RaycastHit2D wallHit = Physics2D.Raycast(wallOrigin, moveDirection, wallCheckDistance, wallLayer);

        if (wallHit.collider != null)
        {
            Flip();
        }
    }

    private void Flip()
    {
        movingRight = !movingRight;
        flipCooldown = 0.25f;
        UpdateFacingDirection();
    }

    private void UpdateFacingDirection()
    {
        Vector3 scale = transform.localScale;
        scale.x = movingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
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

    private void Die()
    {
        isDead = true;
        col.enabled = false;
        rb.linearVelocity = Vector2.zero;

        var spawnable = GetComponent<SpawnableEnemy>();
        if (spawnable != null)
        {
            spawnable.NotifyKilled();
        }

        if (animator != null)
        {
            animator.SetTrigger(DieTriggerHash);
        }

        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        StartCoroutine(SquishAndDestroy());
    }

    private IEnumerator SquishAndDestroy()
    {
        transform.localScale = new Vector3(transform.localScale.x * 1.25f, transform.localScale.y * 0.4f, 1f);
        yield return new WaitForSeconds(0.18f);
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (wallCheck != null)
        {
            float forwardOffset = Mathf.Abs(wallCheck.localPosition.x);
            Vector2 wallOrigin = new Vector2(transform.position.x + (movingRight ? forwardOffset : -forwardOffset), wallCheck.position.y);
            Vector2 dir = movingRight ? Vector2.right : Vector2.left;
            Gizmos.DrawRay(wallOrigin, dir * wallCheckDistance);
        }

        Gizmos.color = Color.green;
        if (groundCheck != null)
        {
            float groundOffset = Mathf.Abs(groundCheck.localPosition.x);
            Vector2 groundOrigin = new Vector2(transform.position.x + (movingRight ? groundOffset : -groundOffset), groundCheck.position.y);
            Gizmos.DrawRay(groundOrigin, Vector2.down * groundCheckDistance);
        }
    }
}