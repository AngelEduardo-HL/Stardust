using UnityEngine;
using UnityEngine.InputSystem;

public sealed class AllySpawnManager : MonoBehaviour
{
    [System.Serializable]
    private class SpawnLane
    {
        public Transform spawnPoint;
        public Transform arrivalPoint;
    }

    [Header("Naves aliadas")]
    [SerializeField] private EnemyShipAI corvettePrefab;
    [SerializeField] private EnemyShipAI cruiserPrefab;
    [SerializeField] private EnemyShipAI dreadnoughtPrefab;

    [Header("Carriles de entrada")]
    [SerializeField] private SpawnLane[] spawnLanes;

    [Header("Estacion")]
    [SerializeField] private Transform spaceStation;

    [Header("Zona aliada")]
    [SerializeField, Min(0f)] private float minimumApproachRadius = 40f;
    [SerializeField, Min(0f)] private float maximumApproachRadius = 70f;

    private void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
            SpawnAlly(corvettePrefab);

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
            SpawnAlly(cruiserPrefab);

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
            SpawnAlly(dreadnoughtPrefab);
    }

    public EnemyShipAI SpawnAlly(EnemyShipAI allyPrefab)
    {
        if (allyPrefab == null)
        {
            Debug.LogError("AllySpawnManager: prefab aliado no asignado.", this);
            return null;
        }

        if (spawnLanes == null || spawnLanes.Length == 0)
        {
            Debug.LogError("AllySpawnManager: no existen Spawn Lanes.", this);
            return null;
        }

        if (spaceStation == null)
        {
            Debug.LogError("AllySpawnManager: no hay estación asignada.", this);
            return null;
        }

        SpawnLane lane = spawnLanes[Random.Range(0, spawnLanes.Length)];

        if (lane.spawnPoint == null || lane.arrivalPoint == null)
        {
            Debug.LogError("AllySpawnManager: uno de los carriles está incompleto.", this);
            return null;
        }

        Vector3 spawnPosition = lane.spawnPoint.position;

        Vector3 direction = lane.arrivalPoint.position - spawnPosition;
        direction.y = 0f;

        Quaternion spawnRotation = direction.sqrMagnitude > 0.001f
            ? Quaternion.LookRotation(direction.normalized, Vector3.up)
            : lane.spawnPoint.rotation;

        EnemyShipAI ally = Instantiate(allyPrefab, spawnPosition, spawnRotation);

        ally.Initialize(
            lane.arrivalPoint.position,
            spaceStation,
            minimumApproachRadius,
            maximumApproachRadius
        );

        Debug.Log($"Aliado desplegado: {ally.name}", ally);

        return ally;
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