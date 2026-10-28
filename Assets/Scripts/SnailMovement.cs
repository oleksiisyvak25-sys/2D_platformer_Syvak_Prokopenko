using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class SnailMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _speed = 1.5f;
    [SerializeField] private LayerMask _whatIsGround;

    [Header("Visual")]
    [Tooltip("Mirror the sprite")]
    [SerializeField] private bool _flipSprite = true;

    [Header("Surface following")]
    [Tooltip("Ground check")]
    [SerializeField] private float _groundCheckDistance = 0.15f;
    [Tooltip("Wall check")]
    [SerializeField] private float _wallCheckDistance = 0.08f;
    [Tooltip("Small gape between snail and ground/wall")]
    [SerializeField] private float _surfaceSkin = 0.02f;
    [Tooltip("Align along the slopes")]
    [SerializeField] private bool _alignToSlopes = true;

    private const float ProbeRise = 0.1f;
    private const float WrapReach = 0.3f;
    private const float WrapDepth = 0.05f;

    private Rigidbody2D _rb;
    private BoxCollider2D _col;
    private SpriteRenderer _sprite;

    private Vector2 _up;
    private float _belly;
    private float _back;
    private float _halfLength;
    private float _centerX;
    private float _centerY;

    private readonly RaycastHit2D[] _hits = new RaycastHit2D[8];
    private ContactFilter2D _filter;

    private Vector2 Right => new Vector2(_up.y, -_up.x);

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<BoxCollider2D>();
        _sprite = GetComponentInChildren<SpriteRenderer>();

        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;

        if (_sprite != null) _sprite.flipX = _flipSprite;

        _filter = new ContactFilter2D();
        _filter.SetLayerMask(_whatIsGround);
        _filter.useTriggers = false;

        Vector2 scale = new Vector2(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y));
        Vector2 half = Vector2.Scale(_col.size, scale) * 0.5f;
        Vector2 offset = Vector2.Scale(_col.offset, scale);

        _belly = half.y - offset.y;
        _back = half.x - offset.x;
        _halfLength = half.x;
        _centerX = offset.x;
        _centerY = offset.y;

        _up = transform.up;
    }

    private void FixedUpdate()
    {
        Vector2 pos = _rb.position;
        Vector2 right = Right;
        float speed = Mathf.Abs(_speed);
        float step = speed * Time.fixedDeltaTime;

        Vector2 center = pos + right * _centerX + _up * _centerY;
        if (Cast(center, right, _halfLength + _wallCheckDistance + step, out RaycastHit2D wall))
        {
            Vector2 n = wall.normal;
            Vector2 newRight = new Vector2(n.y, -n.x);

            float shift = Mathf.Max(0f, _back - _belly + _surfaceSkin);

            pos = wall.point + n * (_belly + _surfaceSkin) + newRight * (shift - _centerY);
            _up = n;
        }
        else
        {
            Vector2 groundOrigin = pos + right * _centerX - _up * _belly + _up * ProbeRise;
            float groundRay = ProbeRise + _surfaceSkin + _groundCheckDistance;

            if (Cast(groundOrigin, -_up, groundRay, out RaycastHit2D ground))
            {
                pos += _up * (ProbeRise + _surfaceSkin - ground.distance);

                if (_alignToSlopes && Vector2.Angle(_up, ground.normal) < 50f)
                    _up = ground.normal;
            }
            else
            {
                Vector2 wrapOrigin = pos + right * (_centerX + WrapReach) - _up * (_belly + WrapDepth);

                if (Cast(wrapOrigin, -right, WrapReach + 0.5f, out RaycastHit2D face))
                {
                    Vector2 n = face.normal;
                    Vector2 newRight = new Vector2(n.y, -n.x);

                    pos = face.point + n * (_belly + _surfaceSkin) - newRight * _centerX;
                    _up = n;
                }
            }
        }

        _up.Normalize();

        _rb.position = pos;
        _rb.rotation = Mathf.Atan2(-_up.x, _up.y) * Mathf.Rad2Deg;
        _rb.linearVelocity = Right * speed;
    }

    private bool Cast(Vector2 origin, Vector2 direction, float distance, out RaycastHit2D best)
    {
        best = default;
        int count = Physics2D.Raycast(origin, direction, _filter, _hits, distance);

        bool found = false;
        float min = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            RaycastHit2D h = _hits[i];
            if (h.collider == null || h.collider.isTrigger || h.rigidbody == _rb) continue;

            if (h.distance < min)
            {
                min = h.distance;
                best = h;
                found = true;
            }
        }

        return found;
    }
}