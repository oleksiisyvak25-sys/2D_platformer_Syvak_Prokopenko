using System.Collections;
using System.Linq.Expressions;
using UnityEngine;

public class MouseMovement : MonoBehaviour
{
    [SerializeField]
    private float _speed;

    [SerializeField]
    private float _distance;

    [SerializeField]
    private LayerMask _mask;

    [SerializeField]
    private Transform _wallChecker;

    [SerializeField]
    private Transform  _groundChecker;

    [SerializeField]
    private float _stoppingTime;

    private Rigidbody2D _mouseBody;

    private RaycastHit2D _isWall;

    private RaycastHit2D _isGround;

    private bool _isStopped;

    private void Awake()
    {
        _mouseBody = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        Patrolling();
    }

    private void Patrolling()
    {
        if (_isStopped)
        {
            return;
        }

        _isWall = Physics2D.Raycast(_wallChecker.position, -_wallChecker.right, _distance, _mask);
        _isGround = Physics2D.Raycast(_groundChecker.position, -_groundChecker.up, _distance, _mask);
        if (_isWall || !_isGround)
        {
            StartCoroutine(WaitChangeRotation());
        }

        _mouseBody.linearVelocity = (-transform.right) * _speed;
    }

    private IEnumerator WaitChangeRotation()
    {
        _isStopped = true;
        _mouseBody.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(_stoppingTime);
        transform.eulerAngles += new Vector3(0, 180, 0);
        _isStopped = false;
    }
}
