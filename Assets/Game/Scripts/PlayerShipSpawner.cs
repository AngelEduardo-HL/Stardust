using UnityEngine;

public sealed class PlayerShipSpawner : MonoBehaviour
{
    [Header("Naves disponibles")]

    [SerializeField]
    private PlayerShipReferences[] shipPrefabs;


    [Header("Spawn")]

    [SerializeField]
    private Transform spawnPoint;


    [Header("Sistemas de escena")]

    [SerializeField]
    private Camera mainCamera;

    [SerializeField]
    private ShipCameraFollow cameraFollow;

    [SerializeField]
    private ShipSpeedHUD speedHUD;


    [Header("UI")]

    [SerializeField]
    private GameObject shipSelectionPanel;


    private PlayerShipReferences currentShip;


    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera =
                Camera.main;
        }


        if (cameraFollow == null &&
            mainCamera != null)
        {
            cameraFollow =
                mainCamera.GetComponent
                <ShipCameraFollow>();
        }
    }


    public void SpawnShip(
        int shipIndex
    )
    {
        if (shipIndex < 0 ||
            shipIndex >=
            shipPrefabs.Length)
        {
            Debug.LogError(
                $"Indice de nave invalido: " +
                $"{shipIndex}",
                this
            );

            return;
        }


        if (shipPrefabs[shipIndex] ==
            null)
        {
            Debug.LogError(
                $"No hay prefab asignado " +
                $"en Ship {shipIndex}.",
                this
            );

            return;
        }


        RemoveCurrentShip();


        Vector3 spawnPosition =
            spawnPoint != null
                ? spawnPoint.position
                : Vector3.zero;


        Quaternion spawnRotation =
            spawnPoint != null
                ? spawnPoint.rotation
                : Quaternion.identity;


        currentShip =
            Instantiate(
                shipPrefabs[shipIndex],
                spawnPosition,
                spawnRotation
            );


        ConfigureNewShip(
            currentShip
        );


        if (shipSelectionPanel != null)
        {
            shipSelectionPanel.SetActive(
                false
            );
        }
    }


    private void ConfigureNewShip(
        PlayerShipReferences ship
    )
    {
        if (ship == null)
        {
            return;
        }


        // ---------------------
        // CAMARA
        // ---------------------

        if (cameraFollow != null &&
            ship.CameraPivot != null)
        {
            cameraFollow.SetCameraPivot(
                ship.CameraPivot
            );
        }


        // ---------------------
        // VELOCIMETRO
        // ---------------------

        if (speedHUD != null &&
            ship.ShipRigidbody != null)
        {
            speedHUD.SetShip(
                ship.ShipRigidbody
            );
        }


        // ---------------------
        // TORRETAS
        // ---------------------

        TurretAimController[] turrets =
            ship.GetComponentsInChildren
            <TurretAimController>(
                true
            );


        foreach (
            TurretAimController turret
            in turrets
        )
        {
            turret.SetAimCamera(
                mainCamera
            );
        }


        Debug.Log(
            $"Nave creada: {ship.name}. " +
            $"Torretas configuradas: " +
            $"{turrets.Length}",
            ship
        );
    }


    private void RemoveCurrentShip()
    {
        if (currentShip == null)
        {
            return;
        }

        currentShip.gameObject.SetActive(
            false
        );


        Destroy(
            currentShip.gameObject
        );


        currentShip = null;


        if (speedHUD != null)
        {
            speedHUD.ClearShip();
        }
    }
}