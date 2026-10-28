using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BeeMovement : MonoBehaviour
{
    private enum State { Patrol, Chase, Return, Recoil }

    [Header("Speed")]
    [SerializeField] private float _speed = 4f;
    [SerializeField] private float _patrolSpeed = 2f;
    [SerializeField] private float _acceleration = 15f;

    [Header("Patrol path")]
    [SerializeField] private Transform[] _waypoints;
    [SerializeField] private bool _loopPath = false;
    [SerializeField] private Vector2 _patrolDirection = Vector2.right;
    [SerializeField] private float _patrolDistance = 3f;

    [Header("Vision")]
    [SerializeField] private float _detectionRadius = 6f;
    [SerializeField] private LayerMask _obstacleMask;
    [SerializeField] private float _forgetTime = 0.8f;

    [Header("Attack")]
    [SerializeField] private float _recoilTime = 0.7f;

    private const float ArriveDistance = 0.1f;

    private Rigidbody2D _rb;
    private Transform _player;
    private Collider2D _playerCol;
    private bool _debugSees;
    private string _blockedBy = "";
    private string _lastLoggedBlock = "";
    private ContactFilter2D _visionFilter;
    private readonly RaycastHit2D[] _visionHits = new RaycastHit2D[8];

    private Vector2[] _path;
    private int _targetIndex;
    private int _pingDir = 1;
    private Vector2 _returnPoint;

    private State _state;
    private Vector2 _lastSeenPos;
    private float _lostSightTimer;

    private Vector2 _recoilDir;
    private float _recoilTimer;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;

        var playerMovement = FindAnyObjectByType<PlayerMovement>();
        if (playerMovement != null)
        {
            _player = playerMovement.transform;
            _playerCol = playerMovement.GetComponent<Collider2D>();
        }

        _path = BuildPath(transform.position);
        BeginReturn();
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        bool sees = CanSeePlayer();
        if (sees)
        {
            _lastSeenPos = _player.position;
            _lostSightTimer = _forgetTime;
        }
        else
        {
            _lostSightTimer -= dt;
        }

        Vector2 pos = _rb.position;
        Vector2 desired = Vector2.zero;
        bool instant = false;

        if (_state == State.Recoil)
        {
            _recoilTimer -= dt;
            desired = _recoilDir * _speed;
            instant = true;

            if (_recoilTimer <= 0f)
            {
                if (sees) _state = State.Chase;
                else BeginReturn();
            }
        }
        else
        {
            if (sees)
            {
                _state = State.Chase;
            }
            else if (_state == State.Chase && _lostSightTimer <= 0f)
            {
                BeginReturn();
            }

            switch (_state)
            {
                case State.Chase:
                {
                    Vector2 target = sees ? (Vector2)_player.position : _lastSeenPos;
                    desired = VelocityTowards(pos, target, _speed, dt);
                    break;
                }
                case State.Return:
                {
                    desired = VelocityTowards(pos, _returnPoint, _speed, dt);
                    if (Vector2.Distance(pos, _returnPoint) <= ArriveDistance)
                    {
                        _state = State.Patrol;
                    }
                    break;
                }
                case State.Patrol:
                {
                    Vector2 waypoint = _path[_targetIndex];
                    if (Vector2.Distance(pos, waypoint) <= ArriveDistance)
                    {
                        AdvanceTarget();
                        waypoint = _path[_targetIndex];
                    }
                    desired = VelocityTowards(pos, waypoint, _patrolSpeed, dt);
                    break;
                }
            }
        }

        _rb.linearVelocity = instant
            ? desired
            : Vector2.MoveTowards(_rb.linearVelocity, desired, _acceleration * dt);

        Face(_rb.linearVelocity.x);
    }


    private bool CanSeePlayer()
    {
        if (_player == null)
        {
            var playerMovement = FindAnyObjectByType<PlayerMovement>();
            if (playerMovement == null) return false;
            _player = playerMovement.transform;
            _playerCol = playerMovement.GetComponent<Collider2D>();
        }

        _debugSees = false;
        _blockedBy = "";

        Vector2 from = _rb.position;
        Vector2 center = _player.position;

        if ((center - from).sqrMagnitude > _detectionRadius * _detectionRadius) return false;

        if (_playerCol != null)
        {
            Bounds bounds = _playerCol.bounds;
            _debugSees = IsClear(from, bounds.center)
                      || IsClear(from, new Vector2(bounds.center.x, bounds.max.y - 0.05f))
                      || IsClear(from, new Vector2(bounds.center.x, bounds.min.y + 0.05f));
        }
        else
        {
            _debugSees = IsClear(from, center);
        }

        if (!_debugSees && _blockedBy != _lastLoggedBlock)
        {
            Debug.Log("Bee: гравця закриває " + _blockedBy, this);
            _lastLoggedBlock = _blockedBy;
        }
        else if (_debugSees)
        {
            _lastLoggedBlock = "";
        }

        return _debugSees;
    }

    private bool IsClear(Vector2 from, Vector2 to)
    {
        _visionFilter.SetLayerMask(_obstacleMask);
        _visionFilter.useTriggers = false;

        int count = Physics2D.Linecast(from, to, _visionFilter, _visionHits);

        for (int i = 0; i < count; i++)
        {
            Collider2D hit = _visionHits[i].collider;

            if (hit == null || hit.isTrigger) continue;
            if (hit.attachedRigidbody == _rb) continue;
            if (_player != null && hit.transform.IsChildOf(_player)) continue;

            if (_blockedBy.Length == 0)
            {
                _blockedBy = hit.name + " (шар " + LayerMask.LayerToName(hit.gameObject.layer) + ")";
            }
            return false;
        }

        return true;
    }


    private Vector2[] BuildPath(Vector2 origin)
    {
        var points = new List<Vector2>();

        if (_waypoints != null)
        {
            foreach (var waypoint in _waypoints)
            {
                if (waypoint != null) points.Add(waypoint.position);
            }
        }

        if (points.Count < 2)
        {
            points.Clear();
            Vector2 dir = _patrolDirection.sqrMagnitude > 0.0001f ? _patrolDirection.normalized : Vector2.right;
            points.Add(origin - dir * _patrolDistance);
            points.Add(origin + dir * _patrolDistance);
        }

        return points.ToArray();
    }

    private void BeginReturn()
    {
        Vector2 pos = _rb.position;
        int segmentCount = _loopPath ? _path.Length : _path.Length - 1;
        float best = float.MaxValue;

        for (int s = 0; s < segmentCount; s++)
        {
            Vector2 a = _path[s];
            Vector2 b = _path[(s + 1) % _path.Length];
            Vector2 ab = b - a;

            float t = ab.sqrMagnitude > 0.0001f
                ? Mathf.Clamp01(Vector2.Dot(pos - a, ab) / ab.sqrMagnitude)
                : 0f;

            Vector2 closest = a + ab * t;
            float sqrDistance = (pos - closest).sqrMagnitude;

            if (sqrDistance < best)
            {
                best = sqrDistance;
                _returnPoint = closest;
                _targetIndex = (s + 1) % _path.Length;
            }
        }

        _pingDir = 1;
        _state = State.Return;
    }

    private void AdvanceTarget()
    {
        if (_loopPath)
        {
            _targetIndex = (_targetIndex + 1) % _path.Length;
            return;
        }

        int next = _targetIndex + _pingDir;
        if (next >= _path.Length || next < 0)
        {
            _pingDir = -_pingDir;
            next = _targetIndex + _pingDir;
        }
        _targetIndex = next;
    }


    private static Vector2 VelocityTowards(Vector2 from, Vector2 to, float speed, float dt)
    {
        Vector2 delta = to - from;
        float distance = delta.magnitude;
        if (distance < 0.001f) return Vector2.zero;

        return delta / distance * Mathf.Min(speed, distance / dt);
    }

    private void Face(float velocityX)
    {
        if (velocityX > 0.05f)
        {
            transform.eulerAngles = new Vector3(0, 180, 0);
        }
        else if (velocityX < -0.05f)
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
        }
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        Vector2 away = (Vector2)transform.position - (Vector2)collision.transform.position;
        away = away.sqrMagnitude > 0.0001f ? away.normalized : Vector2.up;

        _recoilDir = new Vector2(away.x, Mathf.Max(away.y, 0.3f)).normalized;
        _recoilTimer = _recoilTime;
        _state = State.Recoil;
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _detectionRadius);

        if (Application.isPlaying && _player != null)
        {
            Gizmos.color = _debugSees ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, _player.position);

#if UNITY_EDITOR
            if (!_debugSees && _blockedBy.Length > 0)
            {
                UnityEditor.Handles.Label(transform.position + Vector3.up * 0.8f, "Закрито: " + _blockedBy);
            }
#endif
        }

        Vector2[] path = (Application.isPlaying && _path != null) ? _path : BuildPath(transform.position);

        Gizmos.color = Color.cyan;
        for (int i = 0; i < path.Length; i++)
        {
            Gizmos.DrawWireSphere(path[i], 0.15f);

            if (i < path.Length - 1)
            {
                Gizmos.DrawLine(path[i], path[i + 1]);
            }
            else if (_loopPath && path.Length > 2)
            {
                Gizmos.DrawLine(path[i], path[0]);
            }
        }
    }
}