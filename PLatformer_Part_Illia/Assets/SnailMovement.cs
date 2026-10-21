using System;
using UnityEngine;
using UnityEngineInternal;

public class SnailMovement : MonoBehaviour
{
    [SerializeField]
    private float _speed;

    [SerializeField]

    private float distance;
    private RaycastHit2D _isWall;
    private RaycastHit2D _isGround;
    [SerializeField]
    private Transform _groundPostition;

    [SerializeField]
    private Transform _wallPosition;

    private float _groundRaySize;

    private float _wallRaySize;

    [SerializeField]
    private LayerMask _whatIsGround;

    private Rigidbody2D _snailBody;
    private BoxCollider2D _snailCollider;

    private bool _hasTurned;

    private float _rotation;

    private void Awake()
    {
        _snailBody = GetComponent<Rigidbody2D>();
        _snailCollider = GetComponent<BoxCollider2D>();
        _groundRaySize = 0.2f;
        _wallRaySize = 0.08f;
    }

    void Start()
    {

    }

    void FixedUpdate()
    {
        CheckGroundOrWall();
        Move();
    }

    private void CheckGroundOrWall()
    {
        _isGround = Physics2D.Raycast(_groundPostition.position, -transform.up, _groundRaySize, _whatIsGround);
        _isWall = Physics2D.Raycast(_wallPosition.position, transform.right, _wallRaySize, _whatIsGround);

        if (!_isGround && !_hasTurned)
        {
            _rotation -= 90;
            _hasTurned = true;
            Debug.Log("Snail Rotated -90");
        }
        else if (_isWall && _isGround)
        {
            _snailBody.position = GetNewPositoin(_isWall, _snailCollider, transform.position);
            _rotation += 90;
            _hasTurned = true;
            Debug.Log("Snail Rotated +90");
        }

        if (_isGround)
        {

            _hasTurned = false;
        }

        transform.eulerAngles = new Vector3(0, 0, _rotation);

    }

    private void Move()
    {
        _snailBody.linearVelocity = transform.right * _speed;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(_groundPostition.position, new Vector2(_groundPostition.position.x, _groundPostition.position.y - _groundRaySize));
        Gizmos.DrawLine(_wallPosition.position, new Vector2(_wallPosition.position.x + _wallRaySize, _wallPosition.position.y));
    }

    private Vector3 GetNewPositoin(RaycastHit2D hit, BoxCollider2D col, Vector2 pos)
    {
        float projection = Vector2.Dot((hit.point - pos), hit.normal);
        float d = projection + distance;
        Vector3 newPostion = pos - hit.normal*d;
        return newPostion;
    }
}