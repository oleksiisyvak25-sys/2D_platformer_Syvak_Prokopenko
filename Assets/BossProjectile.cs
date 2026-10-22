using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BossProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 4f;
    [SerializeField] private int damage = 1;

    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        rb.gravityScale = 0f;
    }

    public void Launch(Vector2 direction)
    {
        rb.linearVelocity = direction.normalized * speed;

        if (sr != null)
        {
            sr.flipX = (direction.x > 0);
        }

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Boss")) return;

        if (collision.CompareTag("Player"))
        {
            var health = collision.GetComponent<PlayerHealth>();
            if (health != null)
            {
                Vector2 knockbackDir = (collision.transform.position - transform.position).normalized;
                health.TakeDamage(damage, knockbackDir);
            }
            Destroy(gameObject);
            return;
        }

        if (collision.GetComponent<BossButton>() != null)
        {
            Destroy(gameObject);
            return;
        }

        if (collision.CompareTag("Ground") || collision.CompareTag("Wall") || !collision.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}