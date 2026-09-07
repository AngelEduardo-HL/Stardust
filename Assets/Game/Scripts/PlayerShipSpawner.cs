using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PlayerShipSpawner : MonoBehaviour
{
    [Header("Naves")]
    [SerializeField] private PlayerShipReferences[] shipPrefabs;

    [Header("Costos")]
    [SerializeField, Min(0)] private int corvetteCost = 0;
    [SerializeField, Min(0)] private int cruiserCost = 4600;
    [SerializeField, Min(0)] private int dreadnoughtCost = 8000;

    [Header("Spawn")]
    [SerializeField] private Transform spawnPoint;

    [Header("Sistemas")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private ShipCameraFollow cameraFollow;
    [SerializeField] private ShipSpeedHUD speedHUD;
    [SerializeField] private PlayerHealthHUD healthHUD;
    [SerializeField] private AllySpawnManager allySpawnManager;

    [Header("UI")]
    [SerializeField] private GameObject shipSelectionPanel;
    [SerializeField] private GameObject allyPurchasePanel;

    private PlayerShipReferences currentShip;
    private HealthController currentHealth;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (cameraFollow == null && mainCamera != null)
            cameraFollow = mainCamera.GetComponent<ShipCameraFollow>();
    }

    private void Start()
    {
        SetShipSelectionVisible(true);
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.mKey.wasPressedThisFrame)
        {
            if (currentShip == null)
            {
                SetShipSelectionVisible(true);
                return;
            }

            bool show = shipSelectionPanel != null && !shipSelectionPanel.activeSelf;
            SetShipSelectionVisible(show);
        }
    }

    public void SpawnShip(int shipIndex)
    {
        if (shipIndex < 0 || shipIndex >= shipPrefabs.Length)
        {
            Debug.LogError($"Indice de nave invalido: {shipIndex}", this);
            return;
        }

        if (shipPrefabs[shipIndex] == null)
        {
            Debug.LogError($"No existe prefab en Ship {shipIndex}.", this);
            return;
        }

        int cost = GetShipCost(shipIndex);

        if (cost > 0)
        {
            if (CreditManager.Instance == null)
            {
                Debug.LogError("No existe CreditManager.", this);
                return;
            }

            if (!CreditManager.Instance.TrySpendCredits(cost))
            {
                Debug.Log($"Creditos insuficientes. La nave cuesta {cost}.", this);
                return;
            }
        }

        RemoveCurrentShip();

        Vector3 position = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        Quaternion rotation = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

        currentShip = Instantiate(shipPrefabs[shipIndex], position, rotation);

        ConfigureNewShip(currentShip);
        SetShipSelectionVisible(false);

        Debug.Log($"Nave seleccionada: {currentShip.name}. Costo: {cost}", currentShip);
    }

    private int GetShipCost(int shipIndex)
    {
        return shipIndex switch
        {
            0 => corvetteCost,
            1 => cruiserCost,
            2 => dreadnoughtCost,
            _ => 0
        };
    }

    private void ConfigureNewShip(PlayerShipReferences ship)
    {
        if (ship == null) return;

        if (cameraFollow != null && ship.CameraPivot != null)
            cameraFollow.SetCameraPivot(ship.CameraPivot);

        if (speedHUD != null && ship.ShipRigidbody != null)
            speedHUD.SetShip(ship.ShipRigidbody);

        currentHealth = ship.GetComponent<HealthController>();

        if (currentHealth == null)
            currentHealth = ship.GetComponentInChildren<HealthController>();

        if (currentHealth != null)
        {
            currentHealth.Died += OnCurrentShipDied;
            healthHUD?.SetShip(currentHealth);
        }
        else
        {
            Debug.LogWarning($"{ship.name} no tiene HealthController.", ship);
        }

        TurretAimController[] turrets = ship.GetComponentsInChildren<TurretAimController>(true);

        foreach (TurretAimController turret in turrets)
        {
            if (turret != null)
                turret.SetAimCamera(mainCamera);
        }
    }

    private void OnCurrentShipDied(HealthController health)
    {
        if (health != currentHealth) return;

        currentHealth.Died -= OnCurrentShipDied;

        currentHealth = null;
        currentShip = null;

        speedHUD?.ClearShip();
        healthHUD?.ClearShip();

        if (cameraFollow != null)
            cameraFollow.SetCameraPivot(null);

        SetShipSelectionVisible(true);
    }

    private void SetShipSelectionVisible(bool visible)
    {
        if (shipSelectionPanel != null)
            shipSelectionPanel.SetActive(visible);

        if (allyPurchasePanel != null)
            allyPurchasePanel.SetActive(!visible);

        if (allySpawnManager != null)
            allySpawnManager.SetInputEnabled(!visible);
    }

    private void RemoveCurrentShip()
    {
        if (currentHealth != null)
        {
            currentHealth.Died -= OnCurrentShipDied;
            currentHealth = null;
        }

        healthHUD?.ClearShip();

        if (currentShip != null)
        {
            currentShip.gameObject.SetActive(false);
            Destroy(currentShip.gameObject);
            currentShip = null;
        }

        speedHUD?.ClearShip();
    }
}