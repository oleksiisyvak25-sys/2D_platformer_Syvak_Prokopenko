using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Horizontal Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float crawlSpeed = 2.8f;
    [SerializeField] private float acceleration = 45f;
    [SerializeField] private float deceleration = 55f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 9.5f;
    [SerializeField] private float doubleJumpForce = 8.5f;
    [SerializeField] private int maxAirJumps = 1;
    [SerializeField] private float fallMultiplier = 2.4f;
    [SerializeField] private float lowJumpMultiplier = 2.0f;
    [SerializeField] private float coyoteTime = 0.12f;
    [SerializeField] private float jumpBufferTime = 0.12f;

    [Header("Fatigue Mechanics")]
    [SerializeField] private float consecutiveJumpPenalty = 0.75f;
    [SerializeField] private float fatigueRecoveryTime = 0.35f;
    private float currentSpeedMultiplier = 1f;
    private float fatigueTimer;

    [Header("Environment Checks")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.5f, 0.15f);
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask oneWayPlatformLayer;

    [SerializeField] private Transform ceilingCheck;
    [SerializeField] private Vector2 ceilingCheckSize = new Vector2(0.5f, 0.15f);

    [SerializeField] private Transform ladderCheck;
    [SerializeField] private Vector2 ladderCheckSize = new Vector2(0.5f, 0.7f);
    [SerializeField] private LayerMask ladderLayer;

    [Header("Collider Sizing")]
    [SerializeField] private Vector2 standColliderSize = new Vector2(0.6f, 0.8f);
    [SerializeField] private Vector2 standColliderOffset = new Vector2(0f, 0f);
    [SerializeField] private Vector2 duckColliderSize = new Vector2(0.6f, 0.4f);
    [SerializeField] private Vector2 duckColliderOffset = new Vector2(0f, -0.2f);

    [Header("Ladder Settings")]
    [SerializeField] private float climbSpeed = 4f;
    [SerializeField] private float ladderDismountForce = 6.5f;

    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private Animator animator;

    private float horizontalInput;
    private float verticalInput;
    private bool isGrounded;
    private bool isDucking;
    private bool isClimbing;
    private bool isTouchingLadder;
    private float defaultGravity;

    private float coyoteCounter;
    private float jumpBufferCounter;
    private int airJumpsRemaining;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int IsDuckingHash = Animator.StringToHash("IsDucking");
    private static readonly int IsClimbingHash = Animator.StringToHash("IsClimbing");
    private static readonly int YVelocityHash = Animator.StringToHash("YVelocity");

    public bool IsGrounded => isGrounded;
    public bool IsDucking => isDucking;
    public bool IsClimbing => isClimbing;
    public float HorizontalVelocity => Mathf.Abs(rb.linearVelocity.x);
    public float VerticalVelocity => rb.linearVelocity.y;

    public float SpeedMultiplier { get; set; } = 1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        defaultGravity = rb.gravityScale;
    }

    private void Update()
    {
        GatherInputs();
        CheckEnvironment();
        HandleJumpAndDrop();
        HandleCrouch();
        HandleClimb();
        UpdateFatigue();
        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        ApplyMovement();
        ApplyJumpArc();
    }

    private void GatherInputs()
    {
        horizontalInput = 0f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) horizontalInput += 1f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) horizontalInput -= 1f;

        verticalInput = 0f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) verticalInput += 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) verticalInput -= 1f;

        if (Input.GetKeyDown(KeyCode.Space))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        if (horizontalInput > 0.01f)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1f);
        else if (horizontalInput < -0.01f)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, 1f);
    }

    private void CheckEnvironment()
    {
        if (rb.linearVelocity.y > 0.05f)
        {
            isGrounded = false;
        }
        else if (groundCheck != null)
        {
            LayerMask combinedMask = groundLayer | oneWayPlatformLayer;
            isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, combinedMask);
        }

        if (isGrounded)
        {
            airJumpsRemaining = maxAirJumps;
        }

        Vector2 ladderPos = ladderCheck != null ? (Vector2)ladderCheck.position : (Vector2)transform.position;
        isTouchingLadder = Physics2D.OverlapBox(ladderPos, ladderCheckSize, 0f, ladderLayer);

        if (!isTouchingLadder && isClimbing)
        {
            isClimbing = false;
            rb.gravityScale = defaultGravity;
        }
    }

    private void HandleJumpAndDrop()
    {
        if (isGrounded)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.deltaTime;

        if (jumpBufferCounter > 0f && verticalInput < -0.2f)
        {
            if (TryDropThroughPlatform())
            {
                jumpBufferCounter = 0f;
                coyoteCounter = 0f;
                return;
            }
        }

        if (isClimbing && jumpBufferCounter > 0f)
        {
            isClimbing = false;
            rb.gravityScale = defaultGravity;
            rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, ladderDismountForce);
            jumpBufferCounter = 0f;
            airJumpsRemaining = maxAirJumps;
            return;
        }

        if (jumpBufferCounter > 0f && coyoteCounter > 0f && !isDucking && !isClimbing && verticalInput >= -0.2f)
        {
            ExecuteJump(jumpForce);
            coyoteCounter = -1f;
        }
        else if (jumpBufferCounter > 0f && airJumpsRemaining > 0 && !isGrounded && !isClimbing)
        {
            airJumpsRemaining--;
            ExecuteJump(doubleJumpForce);
        }
    }

    private void ExecuteJump(float force)
    {
        currentSpeedMultiplier = consecutiveJumpPenalty;
        fatigueTimer = fatigueRecoveryTime;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, force);
        jumpBufferCounter = 0f;
        isGrounded = false;
    }

    private bool TryDropThroughPlatform()
    {
        Vector2 checkPos = groundCheck != null ? (Vector2)groundCheck.position : (Vector2)transform.position;
        Collider2D hit = Physics2D.OverlapBox(checkPos, new Vector2(groundCheckSize.x * 1.5f, 0.4f), 0f, oneWayPlatformLayer);

        if (hit != null)
        {
            PlatformEffector2D effector = hit.GetComponent<PlatformEffector2D>();
            if (effector == null && hit.transform.parent != null)
            {
                effector = hit.transform.parent.GetComponent<PlatformEffector2D>();
            }

            if (effector != null)
            {
                StartCoroutine(FlipEffectorRoutine(effector));
                return true;
            }

            Collider2D[] colliders = hit.transform.parent != null
                ? hit.transform.parent.GetComponentsInChildren<Collider2D>()
                : hit.GetComponents<Collider2D>();

            StartCoroutine(DisableCollisionsRoutine(colliders));
            return true;
        }

        return false;
    }

    private IEnumerator FlipEffectorRoutine(PlatformEffector2D effector)
    {
        effector.rotationalOffset = 180f;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, -3.5f);

        yield return new WaitForSeconds(0.4f);

        if (effector != null)
        {
            effector.rotationalOffset = 0f;
        }
    }

    private IEnumerator DisableCollisionsRoutine(Collider2D[] platformColliders)
    {
        foreach (var col in platformColliders)
        {
            if (col != null) Physics2D.IgnoreCollision(playerCollider, col, true);
        }

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, -3.5f);

        yield return new WaitForSeconds(0.4f);

        foreach (var col in platformColliders)
        {
            if (col != null && playerCollider != null)
                Physics2D.IgnoreCollision(playerCollider, col, false);
        }
    }

    private void HandleClimb()
    {
        if (isTouchingLadder)
        {
            if (verticalInput > 0.1f)
            {
                isClimbing = true;
            }
            else if (verticalInput < -0.1f && !isGrounded)
            {
                isClimbing = true;
            }
        }

        if (isClimbing && isGrounded && verticalInput < -0.1f)
        {
            isClimbing = false;
            rb.gravityScale = defaultGravity;
        }

        if (isClimbing)
        {
            airJumpsRemaining = maxAirJumps;
            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(horizontalInput * (moveSpeed * 0.4f), verticalInput * climbSpeed);
        }
    }

    private void HandleCrouch()
    {
        if (isClimbing)
        {
            SetCrouchState(false);
            return;
        }

        bool wantsDuck = verticalInput < -0.1f;
        bool roofAbove = false;

        if (ceilingCheck != null)
        {
            roofAbove = Physics2D.OverlapBox(ceilingCheck.position, ceilingCheckSize, 0f, groundLayer);
        }

        if (isGrounded && (wantsDuck || roofAbove))
        {
            SetCrouchState(true);
        }
        else if (!roofAbove)
        {
            SetCrouchState(false);
        }
    }

    private void SetCrouchState(bool duck)
    {
        if (isDucking == duck) return;
        isDucking = duck;

        if (playerCollider is BoxCollider2D box)
        {
            box.size = duck ? duckColliderSize : standColliderSize;
            box.offset = duck ? duckColliderOffset : standColliderOffset;
        }
        else if (playerCollider is CapsuleCollider2D capsule)
        {
            capsule.size = duck ? duckColliderSize : standColliderSize;
            capsule.offset = duck ? duckColliderOffset : standColliderOffset;
        }
    }

    private void ApplyMovement()
    {
        if (isClimbing) return;

        float target = (isDucking ? crawlSpeed : moveSpeed) * SpeedMultiplier * currentSpeedMultiplier * horizontalInput;
        float diff = target - rb.linearVelocity.x;
        float rate = (Mathf.Abs(target) > 0.01f) ? acceleration : deceleration;
        float force = diff * rate * Time.fixedDeltaTime;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x + force, rb.linearVelocity.y);
    }

    private void ApplyJumpArc()
    {
        if (isClimbing) return;

        if (rb.linearVelocity.y < 0)
            rb.gravityScale = defaultGravity * fallMultiplier;
        else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space))
            rb.gravityScale = defaultGravity * lowJumpMultiplier;
        else
            rb.gravityScale = defaultGravity;
    }

    private void UpdateFatigue()
    {
        if (fatigueTimer > 0f)
            fatigueTimer -= Time.deltaTime;
        else
            currentSpeedMultiplier = Mathf.MoveTowards(currentSpeedMultiplier, 1f, Time.deltaTime * 2.5f);
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;

        animator.SetFloat(SpeedHash, Mathf.Abs(horizontalInput));
        animator.SetBool(IsGroundedHash, isGrounded);
        animator.SetBool(IsDuckingHash, isDucking);
        animator.SetBool(IsClimbingHash, isClimbing);
        animator.SetFloat(YVelocityHash, rb.linearVelocity.y);
    }

    public void Bounce(float force)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, force);
        coyoteCounter = 0f;
        airJumpsRemaining = maxAirJumps;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube((Vector2)groundCheck.position + Vector2.down * 0.15f, new Vector2(groundCheckSize.x * 1.5f, 0.4f));
        }
    }
}