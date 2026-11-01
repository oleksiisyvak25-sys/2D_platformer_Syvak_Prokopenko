using System;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance { get; private set; }

    public static event Action<int> CheckpointChanged;

    [Header("Spawn Points")]
    [SerializeField] private Transform outdoorSpawnPoint;
    [SerializeField] private Transform bossArenaSpawnPoint;

    [Header("Progress")]
    [SerializeField] private int bossArenaOrder = 100;

    private const int NoCheckpoint = int.MinValue;

    private int _activeOrder = NoCheckpoint;
    private Vector3 _activePosition;
    private bool _bossArenaReached;

    public int ActiveOrder => _activeOrder;

    public static bool HasEnteredCastle
    {
        get { return Instance != null && Instance._bossArenaReached; }
        set
        {
            if (Instance == null) return;

            if (value) Instance.ReachBossArena();
            else Instance.ResetProgress();
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Instance = null;
        CheckpointChanged = null;
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public Vector3 GetRespawnPosition()
    {
        if (_activeOrder != NoCheckpoint) return _activePosition;

        return outdoorSpawnPoint != null ? outdoorSpawnPoint.position : Vector3.zero;
    }

    public bool TryActivateCheckpoint(int order, Vector3 position)
    {
        if (order <= _activeOrder) return false;

        _activeOrder = order;
        _activePosition = position;
        CheckpointChanged?.Invoke(_activeOrder);
        return true;
    }

    private void ReachBossArena()
    {
        _bossArenaReached = true;

        if (bossArenaSpawnPoint != null)
        {
            TryActivateCheckpoint(bossArenaOrder, bossArenaSpawnPoint.position);
        }
    }

    public void ResetProgress()
    {
        _activeOrder = NoCheckpoint;
        _bossArenaReached = false;
        CheckpointChanged?.Invoke(_activeOrder);
    }
}