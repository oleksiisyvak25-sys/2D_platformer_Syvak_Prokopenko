using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class BatMovement : MonoBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] private float speed = 3f;
    [SerializeField] private float travelDistance = 5f;
    [SerializeField] private bool startMovingRight = true;

    private Rigidbody2D rb;
    private Vector2 startPos;
    private bool movingRight;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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
}