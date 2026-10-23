using UnityEngine;

public class PlayerAwarness : MonoBehaviour
{

    [SerializeField]
    private LayerMask _layer;
    private bool _awareOfPlayer;

    [SerializeField]
    private float _playerAwarenessZone;

    private Transform _player;


    public float PlayerAwarenessZone => _playerAwarenessZone;
    public bool AwareOfPlayer => _awareOfPlayer;

    private void Awake()
    {
        _player = FindAnyObjectByType<PlayerMovement>().transform;
    }

    void Update()
    {
        SetAwarness();
    }

    private void SetAwarness()
    {
        Vector2 EnemyToPlayerPos = _player.position - transform.position;

        if (EnemyToPlayerPos.magnitude <= _playerAwarenessZone)
        {
            _awareOfPlayer = true;

        }
        else
        {
            _awareOfPlayer = false;
        }
    }
}
