using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance { get; private set; }

    [SerializeField] private Transform outdoorSpawnPoint;
    [SerializeField] private Transform bossArenaSpawnPoint;

    public static bool HasEnteredCastle { get; set; } = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public Vector3 GetRespawnPosition()
    {
        if (HasEnteredCastle && bossArenaSpawnPoint != null)
        {
            return bossArenaSpawnPoint.position;
        }

        return outdoorSpawnPoint != null ? outdoorSpawnPoint.position : Vector3.zero;
    }
}