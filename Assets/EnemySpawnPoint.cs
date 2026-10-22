using UnityEngine;

public class EnemySpawnPoint : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float respawnCooldown = 3f;
    [SerializeField] private float offscreenBuffer = 1.5f;

    private GameObject currentEnemyInstance;
    private Camera cam;
    private bool isPendingRespawn = false;
    private float timer = 0f;

    private void Awake()
    {
        cam = Camera.main;
        if (cam == null)
        {
            cam = FindFirstObjectByType<Camera>();
        }
    }

    private void Start()
    {
        SpawnEnemy();
    }

    private void Update()
    {
        if (currentEnemyInstance == null && !isPendingRespawn)
        {
            isPendingRespawn = true;
            timer = 0f;
        }

        if (!isPendingRespawn) return;

        timer += Time.deltaTime;

        if (timer >= respawnCooldown)
        {
            if (IsOutOfCameraView())
            {
                SpawnEnemy();
            }
        }
    }

    public void OnEnemyDied()
    {
        isPendingRespawn = true;
        timer = 0f;
    }

    private void SpawnEnemy()
    {
        if (BossFightManager.IsBossDefeated) return;
        if (enemyPrefab == null) return;

        currentEnemyInstance = Instantiate(enemyPrefab, transform.position, Quaternion.identity);

        var spawnable = currentEnemyInstance.GetComponent<SpawnableEnemy>();
        if (spawnable == null)
        {
            spawnable = currentEnemyInstance.AddComponent<SpawnableEnemy>();
        }
        spawnable.Init(this);

        isPendingRespawn = false;
        timer = 0f;
    }

    public void DestroySpawnedEnemyOnBossDeath()
    {
        isPendingRespawn = false;
        if (currentEnemyInstance != null)
        {
            Destroy(currentEnemyInstance);
        }
    }

    private bool IsOutOfCameraView()
    {
        if (cam == null)
        {
            cam = Camera.main;
            if (cam == null) return true;
        }

        float cameraHeight = cam.orthographicSize * 2f;
        float cameraWidth = cameraHeight * cam.aspect;

        Vector2 camPos = cam.transform.position;
        Vector2 myPos = transform.position;

        float horizontalBound = (cameraWidth / 2f) + offscreenBuffer;
        float verticalBound = (cameraHeight / 2f) + offscreenBuffer;

        bool isOutsideX = Mathf.Abs(myPos.x - camPos.x) > horizontalBound;
        bool isOutsideY = Mathf.Abs(myPos.y - camPos.y) > verticalBound;

        return isOutsideX || isOutsideY;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.4f);
    }
}