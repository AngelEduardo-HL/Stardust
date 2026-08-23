using UnityEngine;

public sealed class PlayerShipReferences : MonoBehaviour
{
    [Header("Referencias de la nave")]

    [SerializeField]
    private Rigidbody shipRigidbody;

    [SerializeField]
    private Transform cameraPivot;


    public Rigidbody ShipRigidbody =>
        shipRigidbody;

    public Transform CameraPivot =>
        cameraPivot;


    private void Awake()
    {
        if (shipRigidbody == null)
        {
            shipRigidbody =
                GetComponent<Rigidbody>();
        }
    }
}