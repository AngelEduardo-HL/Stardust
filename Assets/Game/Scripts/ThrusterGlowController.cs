using UnityEngine;

public sealed class ThrusterGlowController : MonoBehaviour
{
    [Header("Referencias")]

    [SerializeField]
    private Rigidbody shipRigidbody;

    [Tooltip("Renderers que utilizan el material de los thrusters.")]
    [SerializeField]
    private Renderer[] thrusterRenderers;


    [Header("Velocidad")]

    [Tooltip("Velocidad que representa el brillo máximo.")]
    [SerializeField, Min(0.01f)]
    private float maximumSpeed = 10f;


    [Header("Emission")]

    [ColorUsage(false, true)]
    [SerializeField]
    private Color emissionColor = Color.cyan;

    [Tooltip("Intensidad cuando la nave está detenida.")]
    [SerializeField, Min(0f)]
    private float minimumIntensity = 0.2f;

    [Tooltip("Intensidad cuando alcanza la velocidad máxima.")]
    [SerializeField, Min(0f)]
    private float maximumIntensity = 8f;


    [Header("Suavizado")]

    [SerializeField, Min(0f)]
    private float intensityChangeSpeed = 4f;


    private static readonly int EmissionColorID =
        Shader.PropertyToID("_EmissionColor");

    private MaterialPropertyBlock propertyBlock;

    private float currentIntensity;


    private void Awake()
    {
        if (shipRigidbody == null)
        {
            shipRigidbody =
                GetComponentInParent<Rigidbody>();
        }

        propertyBlock =
            new MaterialPropertyBlock();

        currentIntensity =
            minimumIntensity;
    }


    private void Update()
    {
        if (shipRigidbody == null)
        {
            return;
        }


        UpdateThrusterIntensity();
    }


    private void UpdateThrusterIntensity()
    {
        // Velocidad total actual de la nave.
        float currentSpeed =
            shipRigidbody.linearVelocity.magnitude;


        // Convierte la velocidad a un valor
        // entre 0 y 1.
        float speedPercentage =
            Mathf.Clamp01(
                currentSpeed /
                maximumSpeed
            );


        // Calcula el brillo objetivo.
        float targetIntensity =
            Mathf.Lerp(
                minimumIntensity,
                maximumIntensity,
                speedPercentage
            );


        // Hace que el cambio de intensidad
        // sea suave.
        currentIntensity =
            Mathf.MoveTowards(
                currentIntensity,
                targetIntensity,
                intensityChangeSpeed *
                Time.deltaTime
            );


        Color finalEmission =
            emissionColor *
            currentIntensity;


        ApplyEmission(
            finalEmission
        );
    }


    private void ApplyEmission(
        Color finalColor
    )
    {
        foreach (Renderer thrusterRenderer
                 in thrusterRenderers)
        {
            if (thrusterRenderer == null)
            {
                continue;
            }


            thrusterRenderer.GetPropertyBlock(
                propertyBlock
            );


            propertyBlock.SetColor(
                EmissionColorID,
                finalColor
            );


            thrusterRenderer.SetPropertyBlock(
                propertyBlock
            );
        }
    }
}