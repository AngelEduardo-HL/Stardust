using UnityEngine;

public sealed class EnemySpawnManager : MonoBehaviour
{
    [System.Serializable]
    private class SpawnLane
    {
        public Transform spawnPoint;
        public Transform arrivalPoint;
    }

    [Header("Naves enemigas")]
    [SerializeField] private EnemyShipAI[] enemyPrefabs;

    [Header("Carriles de entrada")]
    [SerializeField] private SpawnLane[] spawnLanes;

    [Header("Estacion")]
    [SerializeField] private Transform spaceStation;

    [Header("Zona de aproximacion")]
    [SerializeField, Min(0f)] private float minimumApproachRadius = 60f;
    [SerializeField, Min(0f)] private float maximumApproachRadius = 110f;

    [Header("Prueba")]
    [SerializeField] private bool spawnEnemyOnStart = true;

    private void Start()
    {
        if (spawnEnemyOnStart)
            SpawnRandomEnemy();
    }

    public EnemyShipAI SpawnRandomEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogError("EnemySpawnManager: no hay prefabs enemigos.", this);
            return null;
        }

        EnemyShipAI prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

        return SpawnEnemy(prefab);
    }

    public EnemyShipAI SpawnEnemy(EnemyShipAI enemyPrefab)
    {
        if (enemyPrefab == null) return null;

        if (spawnLanes == null || spawnLanes.Length == 0)
        {
            Debug.LogError("EnemySpawnManager: no existen Spawn Lanes.", this);
            return null;
        }

        if (spaceStation == null)
        {
            Debug.LogError("EnemySpawnManager: no hay estacion asignada.", this);
            return null;
        }

        SpawnLane lane = spawnLanes[Random.Range(0, spawnLanes.Length)];

        if (lane.spawnPoint == null || lane.arrivalPoint == null)
        {
            Debug.LogError("EnemySpawnManager: un carril esta incompleto.", this);
            return null;
        }

        Vector3 spawnPosition = lane.spawnPoint.position;

        Vector3 direction = lane.arrivalPoint.position - spawnPosition;
        direction.y = 0f;

        Quaternion spawnRotation = direction.sqrMagnitude > 0.001f
            ? Quaternion.LookRotation(direction.normalized, Vector3.up)
            : lane.spawnPoint.rotation;

        EnemyShipAI enemy = Instantiate(
            enemyPrefab,
            spawnPosition,
            spawnRotation
        );

        Vector3 randomApproachPoint = GetRandomApproachPoint();

        enemy.Initialize(
            lane.arrivalPoint.position,
            randomApproachPoint
        );

        return enemy;
    }

    private Vector3 GetRandomApproachPoint()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float radius = Random.Range(minimumApproachRadius, maximumApproachRadius);

        Vector3 offset = new Vector3(
            Mathf.Cos(angle) * radius,
            0f,
            Mathf.Sin(angle) * radius
        );

        Vector3 result = spaceStation.position + offset;
        result.y = spaceStation.position.y;

        return result;
    }

    private void OnDrawGizmosSelected()
    {
        if (spaceStation == null) return;

        DrawCircle(spaceStation.position, minimumApproachRadius);
        DrawCircle(spaceStation.position, maximumApproachRadius);
    }

    private void DrawCircle(Vector3 center, float radius)
    {
        const int segments = 48;

        Vector3 previous = center + new Vector3(radius, 0f, 0f);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i / (float)segments * Mathf.PI * 2f;

            Vector3 next = center + new Vector3(
                Mathf.Cos(angle) * radius,
                0f,
                Mathf.Sin(angle) * radius
            );

            Gizmos.DrawLine(previous, next);
            previous = next;
        }
    }
}