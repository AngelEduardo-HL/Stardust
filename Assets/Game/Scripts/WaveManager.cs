using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class WaveManager : MonoBehaviour
{
    [System.Serializable]
    private class WaveDefinition
    {
        public int corvettes;
        public int cruisers;
        public int dreadnoughts;
    }

    [Header("Spawn")]
    [SerializeField] private EnemySpawnManager enemySpawnManager;

    [Header("Prefabs")]
    [SerializeField] private EnemyShipAI corvettePrefab;
    [SerializeField] private EnemyShipAI cruiserPrefab;
    [SerializeField] private EnemyShipAI dreadnoughtPrefab;

    [Header("Oleadas")]
    [SerializeField] private WaveDefinition[] waves;

    [Header("Tiempos")]
    [SerializeField, Min(0.1f)] private float spawnInterval = 2f;
    [SerializeField, Min(0f)] private float timeBetweenWaves = 10f;
    [SerializeField, Min(0f)] private float firstWaveDelay = 5f;

    [Header("Sistemas")]
    [SerializeField] private ObjectiveManager objectiveManager;
    [SerializeField] private WaveCountdownHUD countdownHUD;

    [Header("Debug")]
    [SerializeField] private int currentWave;
    [SerializeField] private int enemiesAlive;
    [SerializeField] private bool waveRunning;

    private readonly List<HealthController> trackedEnemies = new();

    public int CurrentWave => currentWave;
    public int TotalWaves => waves != null ? waves.Length : 0;
    public int EnemiesAlive => enemiesAlive;

    private void Start()
    {
        StartCoroutine(GameLoop());
    }

    private IEnumerator GameLoop()
    {
        if (firstWaveDelay > 0f)
            yield return StartCoroutine(RunIntermission(firstWaveDelay, 1));

        for (int i = 0; i < waves.Length; i++)
        {
            currentWave = i + 1;
            waveRunning = true;

            countdownHUD?.HideCountdown();

            Debug.Log($"OLEADA {currentWave}/{waves.Length}");

            List<EnemyShipAI> spawnList = BuildSpawnList(waves[i]);
            Shuffle(spawnList);

            foreach (EnemyShipAI prefab in spawnList)
            {
                SpawnTrackedEnemy(prefab);
                yield return new WaitForSeconds(spawnInterval);
            }

            while (enemiesAlive > 0)
                yield return null;

            waveRunning = false;

            Debug.Log($"Oleada {currentWave} completada.");

            if (currentWave >= waves.Length)
            {
                countdownHUD?.HideCountdown();

                if (objectiveManager != null)
                    objectiveManager.TriggerVictory();
                else
                    Debug.LogError("WaveManager: falta ObjectiveManager.", this);

                yield break;
            }

            yield return StartCoroutine(RunIntermission(timeBetweenWaves, currentWave + 1));
        }
    }

    private IEnumerator RunIntermission(float duration, int nextWave)
    {
        float remaining = duration;

        while (remaining > 0f)
        {
            countdownHUD?.ShowCountdown(nextWave, remaining);

            remaining -= Time.deltaTime;
            yield return null;
        }

        countdownHUD?.HideCountdown();
    }

    private List<EnemyShipAI> BuildSpawnList(WaveDefinition wave)
    {
        List<EnemyShipAI> result = new();

        for (int i = 0; i < wave.corvettes; i++)
            result.Add(corvettePrefab);

        for (int i = 0; i < wave.cruisers; i++)
            result.Add(cruiserPrefab);

        for (int i = 0; i < wave.dreadnoughts; i++)
            result.Add(dreadnoughtPrefab);

        return result;
    }

    private void SpawnTrackedEnemy(EnemyShipAI prefab)
    {
        EnemyShipAI enemy = enemySpawnManager.SpawnEnemy(prefab);
        if (enemy == null) return;

        HealthController health = enemy.GetComponent<HealthController>();

        if (health == null)
        {
            Debug.LogError($"{enemy.name} no tiene HealthController.", enemy);
            return;
        }

        trackedEnemies.Add(health);
        enemiesAlive++;

        health.Died += OnEnemyDied;
    }

    private void OnEnemyDied(HealthController health)
    {
        health.Died -= OnEnemyDied;
        trackedEnemies.Remove(health);

        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
    }

    private void Shuffle(List<EnemyShipAI> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);

            EnemyShipAI temporary = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temporary;
        }
    }

    private void OnDestroy()
    {
        foreach (HealthController health in trackedEnemies)
        {
            if (health != null)
                health.Died -= OnEnemyDied;
        }
    }
}