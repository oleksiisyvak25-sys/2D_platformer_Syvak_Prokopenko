using UnityEngine;

public class SpawnableEnemy : MonoBehaviour
{
    private EnemySpawnPoint parentSpawner;

    public void Init(EnemySpawnPoint spawner)
    {
        parentSpawner = spawner;
    }

    public void NotifyKilled()
    {
        if (parentSpawner != null)
        {
            parentSpawner.OnEnemyDied();
        }
    }

    private void OnDestroy()
    {
        NotifyKilled();
    }
}