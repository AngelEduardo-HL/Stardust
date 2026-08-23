using TMPro;
using UnityEngine;

public sealed class ShipSpeedHUD : MonoBehaviour
{
    [Header("Referencias")]

    [SerializeField]
    private Rigidbody shipRigidbody;

    [SerializeField]
    private TMP_Text speedText;


    [Header("Visualizacion")]

    [SerializeField]
    private bool showSignedSpeed = true;

    [SerializeField]
    private float speedMultiplier = 1f;

    [SerializeField]
    private string unitText = "";


    private void Update()
    {
        if (shipRigidbody == null ||
            speedText == null)
        {
            return;
        }


        float speed;


        if (showSignedSpeed)
        {
            speed =
                Vector3.Dot(
                    shipRigidbody.linearVelocity,
                    shipRigidbody.transform.forward
                );
        }
        else
        {
            speed =
                shipRigidbody.linearVelocity.magnitude;
        }


        speed *=
            speedMultiplier;


        if (string.IsNullOrWhiteSpace(
                unitText
            ))
        {
            speedText.text =
                $"KM: {speed:0.0}";
        }
        else
        {
            speedText.text =
                $"KM: {speed:0.0} {unitText}";
        }
    }

    public void SetShip(
    Rigidbody newShipRigidbody
)
    {
        shipRigidbody =
            newShipRigidbody;
    }

    public void ClearShip()
    {
        shipRigidbody = null;

        if (speedText != null)
        {
            speedText.text =
                "KM: 0.0";
        }
    }
}