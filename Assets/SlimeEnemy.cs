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
    [SerializeField] private float wallCheckDistance = 0.4f;
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

        Vector2 direction = movingRight ? Vector2.right : Vector2.left;
        Vector2 wallOrigin = wallCheck != null ? (Vector2)wallCheck.position : (Vector2)transform.position;

        LayerMask obstacleMask = wallLayer | groundLayer;

        RaycastHit2D wallHit = Physics2D.Raycast(wallOrigin, direction, wallCheckDistance, obstacleMask);

        bool groundAhead = true;
        if (groundCheck != null)
        {
            RaycastHit2D groundHit = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);
            groundAhead = groundHit.collider != null;
        }

        if (wallHit.collider != null || !groundAhead)
        {
            Flip();
        }
    }

    private void Flip()
    {
        movingRight = !movingRight;
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
        if (spawnable != null) spawnable.NotifyKilled();

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
            Vector2 dir = movingRight ? Vector2.right : Vector2.left;
            Gizmos.DrawRay(wallCheck.position, dir * wallCheckDistance);
        }

        Gizmos.color = Color.green;
        if (groundCheck != null)
        {
            Gizmos.DrawRay(groundCheck.position, Vector2.down * groundCheckDistance);
        }
    }
}