using Unity.VisualScripting;
using UnityEngine;

public class BulletSpikeScript : MonoBehaviour
{
    [SerializeField]
    private float _speed;
    private Rigidbody2D _spikeBody;

    private Transform _player;
    private float _shootingZone;
    private PlayerAwarness _playerAwarness;
    public float ShootingZone => _shootingZone;


    private void Awake()
    {
        _spikeBody = GetComponent<Rigidbody2D>();
        _player = FindAnyObjectByType<PlayerMovement>().transform;
        _playerAwarness = FindAnyObjectByType<PlayerAwarness>();
        _shootingZone = _playerAwarness.PlayerAwarenessZone * (2f / 3f);
    }

    public void Shoot()
    {
        _spikeBody.linearVelocity = (_player.position - transform.position).normalized * _speed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(gameObject);    
    }
}
