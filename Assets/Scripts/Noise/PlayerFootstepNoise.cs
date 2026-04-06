using UnityEngine;

[RequireComponent(typeof(NoiseEmitter))]
public class PlayerFootstepNoise : MonoBehaviour
{
    private NoiseEmitter noiseEmitter;
    [SerializeField] private Rigidbody rb;

    [Header("Footstep Settings")]
    [Tooltip("How often a footstep sound triggers when moving (in seconds).")]
    public float stepInterval = 0.5f;
    
    [Tooltip("Base radius of the footstep noise.")]
    public float baseNoiseRadius = 2f;
    
    [Tooltip("How much the players speed multiplies the noise. Walking vs Sprinting.")]
    public float speedMultiplier = 1f;

    [Header("Weight Settings")]
    [Tooltip("How much each unit of weight adds to the step duration interval (in seconds).")]
    public float weightToDurationMultiplier = 0.05f;

    [Tooltip("How much each unit of weight adds to the footstep noise radius.")]
    public float weightToRadiusMultiplier = 0.5f;

    private float stepTimer = 0f;
    
    private float currentHeldWeight = 0f; 

    private readonly float maxSpeed = 10f;
    
    private void Start()
    {
        noiseEmitter = GetComponent<NoiseEmitter>();
    }

    public void SetWeightModifier(float weight)
    {
        currentHeldWeight = weight;
    }

    private void Update()
    {
        float currentSpeed = rb.linearVelocity.magnitude;
        
        if (currentSpeed > 0.1f && currentSpeed < maxSpeed)
        {
            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0f)
            {
                TriggerFootstep(currentSpeed);
                
                // Calculate and add the extra duration
                stepTimer = stepInterval + (currentHeldWeight * weightToDurationMultiplier); 
            }
        }
    }

    private void TriggerFootstep(float speed)
    {
        float calculatedRadius = baseNoiseRadius + (speed * speedMultiplier) + (currentHeldWeight * weightToRadiusMultiplier);

        noiseEmitter.EmitNoise(calculatedRadius, NoiseType.Player);
    }
}