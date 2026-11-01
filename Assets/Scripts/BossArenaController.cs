using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class BossArenaController : MonoBehaviour
{
    public enum BossState { Patrolling, Charging, Shooting, Cooldown, Dead }

    [SerializeField] private int maxHealth = 10;
    [SerializeField] private int currentHealth;

    [SerializeField] private float flightSpeed = 3f;
    [SerializeField] private float minY = -2f;
    [SerializeField] private float maxY = 6f;
    private bool movingUp = true;

    [SerializeField] private Transform playerTransform;
    [SerializeField] private float lineOfSightTolerance = 0.5f;

    [SerializeField] private float chargeDuration = 1.5f;
    [SerializeField] private float attackCooldown = 2.0f;
    private float lastAttackTime = -10f;

    [SerializeField] private GameObject fireProjectilePrefab;
    [SerializeField] private Transform firePoint;

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;

    [SerializeField] private float mouthOffsetY = 0.2f;

    public BossState CurrentState { get; private set; } = BossState.Patrolling;

    private void Start()
    {
        currentHealth = maxHealth;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (playerTransform == null)
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTransform = playerObj.transform;
        }
    }

    private void Update()
    {
        if (CurrentState == BossState.Dead) return;

        FacePlayer();

        switch (CurrentState)
        {
            case BossState.Patrolling:
                FlyUpDown();
                CheckAlignmentWithPlayer();
                break;

            case BossState.Charging:
            case BossState.Shooting:
            case BossState.Cooldown:
                break;
        }
    }

    private void FacePlayer()
    {
        if (playerTransform == null) return;

        bool shouldFlip = playerTransform.position.x > transform.position.x;
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = shouldFlip;
        }
    }

    private void FlyUpDown()
    {
        float dir = movingUp ? 1f : -1f;
        transform.position += new Vector3(0f, dir * flightSpeed * Time.deltaTime, 0f);

        if (transform.position.y >= maxY)
        {
            movingUp = false;
        }
        else if (transform.position.y <= minY)
        {
            movingUp = true;
        }
    }

    private void CheckAlignmentWithPlayer()
    {
        if (playerTransform == null) return;
        if (Time.time < lastAttackTime + attackCooldown) return;

        float currentMouthY = firePoint != null ? firePoint.position.y : (transform.position.y + mouthOffsetY);

        float targetY = playerTransform.position.y;

        float heightDifference = Mathf.Abs(currentMouthY - targetY);

        if (heightDifference <= lineOfSightTolerance)
        {
            StartCoroutine(PerformAttackRoutine(targetY));
        }
    }

    private IEnumerator PerformAttackRoutine(float targetHeight)
    {
        CurrentState = BossState.Charging;

        float mouthOffset = (firePoint != null ? (firePoint.position.y - transform.position.y) : mouthOffsetY);
        transform.position = new Vector3(transform.position.x, targetHeight - mouthOffset, transform.position.z);

        if (animator != null)
        {
            animator.SetBool("IsCharging", true);
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(1f, 0.4f, 0.4f);
        }

        yield return new WaitForSeconds(chargeDuration);

        CurrentState = BossState.Shooting;
        if (animator != null)
        {
            animator.SetBool("IsCharging", false);
            animator.SetTrigger("Shoot");
        }

        ShootFireball();

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }

        yield return new WaitForSeconds(0.4f);

        CurrentState = BossState.Patrolling;
        lastAttackTime = Time.time;
    }
    

    private void ShootFireball()
    {
        if (fireProjectilePrefab == null) return;

        Vector2 spawnPos = firePoint != null ? firePoint.position : transform.position;

        float shootDirectionX = (playerTransform != null && playerTransform.position.x < transform.position.x) ? -1f : 1f;
        Vector2 shootDirection = new Vector2(shootDirectionX, 0f);

        var projectileObj = Instantiate(fireProjectilePrefab, spawnPos, Quaternion.identity);
        var projectile = projectileObj.GetComponent<BossProjectile>();
        if (projectile != null)
        {
            projectile.Launch(shootDirection);
        }
    }

    public void TakeBombDamage(int damage)
    {
        if (CurrentState == BossState.Dead) return;

        currentHealth -= damage;

        if (animator != null)
        {
            animator.SetTrigger("TakeDamage");
        }

        StartCoroutine(FlashWhiteRoutine());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator FlashWhiteRoutine()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.15f);
            spriteRenderer.color = Color.white;
        }
    }

    private void Die()
    {
        CurrentState = BossState.Dead;
        StopAllCoroutines();

        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 2f;
        }

        Destroy(gameObject, 3f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 top = new Vector3(transform.position.x, maxY, 0f);
        Vector3 bottom = new Vector3(transform.position.x, minY, 0f);
        Gizmos.DrawLine(bottom, top);
    }

    public void SetDifficulty(float speed, float chargeTime, float cooldown)
    {
        flightSpeed = speed;
        chargeDuration = chargeTime;
        attackCooldown = cooldown;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            var playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                Vector2 knockbackDir = (collision.transform.position - transform.position).normalized;
                
                if (knockbackDir == Vector2.zero)
                    knockbackDir = Vector2.down;

                playerHealth.TakeDamage(1, knockbackDir);
            }
        }
    }
}