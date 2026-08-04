using UnityEngine;

public sealed class ShipCameraFollow : MonoBehaviour
{
    [Header("Objetivo")]

    [SerializeField]
    private Transform target;

    [Header("Posición")]

    [SerializeField]
    private Vector3 localOffset = new Vector3(0f, 6f, -15f);

    [SerializeField, Min(0.01f)]
    private float positionSmoothTime = 0.15f;

    [Header("Rotación")]

    [SerializeField, Min(0f)]
    private float rotationSpeed = 8f;

    [SerializeField, Min(0f)]
    private float lookAheadDistance = 10f;

    private Vector3 positionVelocity;

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        FollowTarget();
        LookAtTarget();
    }

    private void FollowTarget()
    {
        Vector3 desiredPosition =
            target.TransformPoint(localOffset);

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref positionVelocity,
            positionSmoothTime
        );
    }

    private void LookAtTarget()
    {
        Vector3 lookTarget =
            target.position +
            target.forward * lookAheadDistance;

        Vector3 direction =
            lookTarget - transform.position;

        if (direction.sqrMagnitude <= 0.001f)
        {
            return;
        }

        Quaternion desiredRotation = Quaternion.LookRotation(
            direction,
            target.up
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            desiredRotation,
            rotationSpeed * Time.deltaTime
        );
    }
}