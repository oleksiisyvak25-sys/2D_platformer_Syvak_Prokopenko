using System;
using System.Collections;
using UnityEngine;

public class BeeMovement : MonoBehaviour
{
    [SerializeField]
    private float _speed;

    [SerializeField]
    private GameObject _bullet;

    [SerializeField]
    private float _timeToDie;

    private Transform _player;
    private Rigidbody2D _beeBody;
    private Vector3 _startPosition;
    private PlayerAwarness _playerAwarness;
    private BulletSpikeScript _bulletSpike;
    private bool _isAvailable = true;
    private bool _isPreparingAttack;
    private Collider2D _beeCol;

    private void Awake()
    {
        _beeBody = GetComponent<Rigidbody2D>();
        _player = FindAnyObjectByType<PlayerMovement>().transform;
        _startPosition = transform.position;
        _playerAwarness = GetComponent<PlayerAwarness>();
        _bulletSpike = _bullet.GetComponent<BulletSpikeScript>();
        _beeCol = GetComponent<Collider2D>();
    }

    private void FixedUpdate()
    {
        UpdateTargetMovement();
    }

    private void Return()
    {
        float distance = Vector2.Distance(transform.position, _startPosition);
        if (distance <= _speed * Time.fixedDeltaTime)
        {
            _beeBody.linearVelocity = Vector2.zero;
        }
        else {
            _beeBody.linearVelocity = (_startPosition - transform.position).normalized * _speed;
        }
    }

    private void Attack()
    {
        if (_isAvailable && (_player.position - transform.position).magnitude <= _bulletSpike.ShootingZone)
        {
            StartCoroutine(PreparingAttack());
        }
        else
        {
            _beeBody.linearVelocity = (_player.position - transform.position).normalized * _speed;
        }
    }

    private void UpdateTargetMovement()
    {
        if (_isPreparingAttack)
        {
            return;
        }
        if (_playerAwarness.AwareOfPlayer)
        {
            Attack();
            Flip(_player.position);
            Debug.Log("Player is in Scope!");
        }
        else
        {
            Return();
            Flip(_startPosition);
            Debug.Log("Player if out of scope!");
        }
    }

    private void Flip(Vector3 point)
    {
        if (transform.position.x >= point.x)
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
        }
        else
        {
            transform.eulerAngles = new Vector3(0, 180, 0);
        }
    }

    private IEnumerator PreparingAttack()
    {
        _isPreparingAttack = true;

        _beeBody.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(1.7f);
        Debug.Log("Preparing is over");

        CreateAndShootBullet();
        DieByTime();

        yield return new WaitForSeconds(1f);
        _isPreparingAttack = false;
        Debug.Log("Ready for pushing");
    }

    private void CreateAndShootBullet()
    {
        GameObject spike = Instantiate(_bullet, transform.position, transform.rotation);
        Physics2D.IgnoreCollision(spike.GetComponent<Collider2D>(), _beeCol);
        BulletSpikeScript spikeScript = spike.GetComponent<BulletSpikeScript>();
        spikeScript.Shoot();
        _isAvailable = false;
    }

    private void DieByTime()
    {
        Destroy(gameObject, _timeToDie);
    }
}
