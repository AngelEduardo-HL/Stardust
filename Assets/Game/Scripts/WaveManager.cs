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

    [Header("Prefabs enemigos")]
    [SerializeField] private EnemyShipAI corvettePrefab;
    [SerializeField] private EnemyShipAI cruiserPrefab;
    [SerializeField] private EnemyShipAI dreadnoughtPrefab;

    [Header("Oleadas")]
    [SerializeField] private WaveDefinition[] waves;

    [Header("Tiempos")]
    [SerializeField, Min(0.1f)] private float spawnInterval = 1.5f;
    [SerializeField, Min(0f)] private float timeBetweenWaves = 8f;

    [Header("Inicio")]
    [SerializeField] private bool startAutomatically = true;
    [SerializeField, Min(0f)] private float firstWaveDelay = 3f;

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
        if (startAutomatically)
            StartCoroutine(StartGame());
    }

    private IEnumerator StartGame()
    {
        yield return new WaitForSeconds(firstWaveDelay);

        currentWave = 0;
        StartCoroutine(RunNextWave());
    }

    private IEnumerator RunNextWave()
    {
        if (currentWave >= waves.Length)
        {
            Victory();
            yield break;
        }

        waveRunning = true;

        WaveDefinition wave = waves[currentWave];

        Debug.Log($"Iniciando oleada {currentWave + 1}/{waves.Length}");

        List<EnemyShipAI> spawnList = BuildSpawnList(wave);

        Shuffle(spawnList);

        foreach (EnemyShipAI prefab in spawnList)
        {
            SpawnTrackedEnemy(prefab);
            yield return new WaitForSeconds(spawnInterval);
        }

        while (enemiesAlive > 0)
            yield return null;

        waveRunning = false;

        Debug.Log($"Oleada {currentWave + 1} completada.");

        currentWave++;

        if (currentWave >= waves.Length)
        {
            Victory();
            yield break;
        }

        yield return new WaitForSeconds(timeBetweenWaves);

        StartCoroutine(RunNextWave());
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

    private void Victory()
    {
        waveRunning = false;

        Debug.Log("LAS 12 OLEADAS HAN SIDO COMPLETADAS.");
    }
}