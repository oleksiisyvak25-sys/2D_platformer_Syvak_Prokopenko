using UnityEngine;

public class BulletSpikeScript : MonoBehaviour
{
    [SerializeField]
    private float _speed;
    private Rigidbody2D _spikeBody;

    private Transform _player;


    private void Awake()
    {
        _spikeBody = GetComponent<Rigidbody2D>();
        _player = FindAnyObjectByType<PlayerMovement>().transform;
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
