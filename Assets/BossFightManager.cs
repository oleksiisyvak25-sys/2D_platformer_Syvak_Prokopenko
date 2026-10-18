using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossFightManager : MonoBehaviour
{
    [Header("Waves Setup")]
    [SerializeField] private int totalWaves = 5;
    [SerializeField] private int currentWave = 1;

    [Header("References")]
    [SerializeField] private BossArenaController bossController;
    [SerializeField] private List<BossButton> allButtons = new List<BossButton>();
    [SerializeField] private GameObject heavyBombPrefab;
    [SerializeField] private Transform[] bombSpawnPoints;

    private List<BossButton> currentWaveButtons = new List<BossButton>();
    private int pressedButtonsCount = 0;
    private GameObject activeBomb;

    private void Start()
    {
        if (bossController == null)
        {
            bossController = FindFirstObjectByType<BossArenaController>();
        }

        foreach (var btn in allButtons)
        {
            if (btn != null) btn.Initialize(this);
        }

        if (currentWave < 1) currentWave = 1;
        StartWave(currentWave);
    }

    public void StartWave(int waveIndex)
    {
        currentWave = waveIndex;
        pressedButtonsCount = 0;

        ApplyWaveDifficulty(currentWave);

        foreach (var btn in allButtons)
        {
            if (btn != null) btn.SetState(false, false);
        }
        currentWaveButtons.Clear();

        int buttonsToActivateCount = Mathf.Clamp(currentWave, 1, allButtons.Count);

        List<BossButton> pool = new List<BossButton>(allButtons);
        for (int i = 0; i < buttonsToActivateCount; i++)
        {
            if (pool.Count == 0) break;

            int randomIndex = Random.Range(0, pool.Count);
            BossButton selectedBtn = pool[randomIndex];

            selectedBtn.SetState(true, false);
            currentWaveButtons.Add(selectedBtn);

            pool.RemoveAt(randomIndex);
        }

        Debug.Log($"[Хвиля {currentWave}] Активовано кнопок: {currentWaveButtons.Count}. Потрібно натиснути всі!");
    }

    private void ApplyWaveDifficulty(int wave)
    {
        if (bossController == null) return;

        switch (wave)
        {
            case 1:
                bossController.SetDifficulty(speed: 3.0f, chargeTime: 1.5f, cooldown: 2.0f);
                break;
            case 2:
                bossController.SetDifficulty(speed: 3.8f, chargeTime: 1.3f, cooldown: 1.7f);
                break;
            case 3:
                bossController.SetDifficulty(speed: 4.6f, chargeTime: 1.1f, cooldown: 1.4f);
                break;
            case 4:
                bossController.SetDifficulty(speed: 5.4f, chargeTime: 0.9f, cooldown: 1.1f);
                break;
            case 5:
                bossController.SetDifficulty(speed: 6.2f, chargeTime: 0.7f, cooldown: 0.8f);
                break;
        }
    }

    public void OnButtonPressed(BossButton button)
    {
        if (currentWaveButtons.Contains(button))
        {
            pressedButtonsCount++;
            Debug.Log($"Натиснуто кнопок: {pressedButtonsCount} / {currentWaveButtons.Count}");

            if (pressedButtonsCount >= currentWaveButtons.Count)
            {
                Debug.Log("Всі кнопки хвилі натиснуто! Скидаємо бомбу!");
                SpawnSingleBomb();
            }
        }
    }

    private void SpawnSingleBomb()
    {
        if (heavyBombPrefab == null || bombSpawnPoints == null || bombSpawnPoints.Length == 0) return;

        if (activeBomb != null)
        {
            Destroy(activeBomb);
        }

        Transform spawnPoint = bombSpawnPoints[Random.Range(0, bombSpawnPoints.Length)];
        activeBomb = Instantiate(heavyBombPrefab, spawnPoint.position, Quaternion.identity);

        var bomb = activeBomb.GetComponent<HeavyBomb>();
        if (bomb != null)
        {
            bomb.Initialize(this);
        }
    }

    public void OnBombMissed()
    {
        Debug.Log("Промах! Хвиля перезапускається: потрібно натиснути всі кнопки заново.");
        StartWave(currentWave);
    }

    public void OnBossHitByBomb()
    {
        currentWave++;

        if (currentWave <= totalWaves)
        {
            StartCoroutine(NextWaveRoutine());
        }
        else
        {
            Debug.Log("Усі 5 хвиль пройдено! Бос повалений!");
        }
    }

    private IEnumerator NextWaveRoutine()
    {
        yield return new WaitForSeconds(1.5f);
        StartWave(currentWave);
    }
}