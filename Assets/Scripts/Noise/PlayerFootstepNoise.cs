using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NoiseEmitter))]
public class PlayerFootstepNoise : MonoBehaviour
{
    private NoiseEmitter noiseEmitter;
    [SerializeField] private Rigidbody rb;
    
    private PlayerStealthController stealthController; 

    [Header("Footstep Settings")]
    [Tooltip("How often a footstep sound triggers when moving (in seconds).")]
    public float stepInterval = 0.5f;
    
    [Tooltip("Base radius of the footstep noise.")]
    public float baseNoiseRadius = 2f;
    
    [Tooltip("How much the players speed multiplies the noise. Walking vs Sprinting.")]
    public float speedMultiplier = 1f;

    [Tooltip("Multiplier applied to the noise radius when sneaking (e.g., 0.2 means 20% normal volume).")]
    public float sneakNoiseMultiplier = 0.2f;

    [Header("Weight Settings")]
    [Tooltip("How much each unit of weight adds to the step duration interval (in seconds).")]
    public float weightToDurationMultiplier = 0.05f;

    [Tooltip("How much each unit of weight adds to the footstep noise radius.")]
    public float weightToRadiusMultiplier = 0.5f;

    [Header("Delay Settings")]
    [Tooltip("Custom delay in seconds before the footstep noise is actually emitted at the previous location.")]
    public float footstepDelay = 1.5f;

    private float stepTimer = 0f;
    private float currentHeldWeight = 0f; 
    private readonly float maxSpeed = 10f;
    
    // Tracks the 3 steps coming out of a sneak
    private int recoveryStepsRemaining = 0;
    
    private void Start()
    {
        noiseEmitter = GetComponent<NoiseEmitter>();
        stealthController = GetComponentInParent<PlayerStealthController>(); 
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

        if (stealthController != null && stealthController.IsSneaking)
        {
            calculatedRadius *= sneakNoiseMultiplier;
            
            // Reset the recovery counter so it's ready for when we stop sneaking
            recoveryStepsRemaining = 3; 
        }
        else if (recoveryStepsRemaining > 0)
        {
            // Player just came out of sneaking, step up the volume gradually 
            if (recoveryStepsRemaining == 3) calculatedRadius *= 0.25f; // 25% etc     
            else if (recoveryStepsRemaining == 2) calculatedRadius *= 0.50f; 
            else if (recoveryStepsRemaining == 1) calculatedRadius *= 0.75f; 

            recoveryStepsRemaining--;
        }

        // Capture the exact position where the step happened
        Vector3 stepPosition = transform.position;

        StartCoroutine(EmitDelayedNoise(stepPosition, calculatedRadius));
    }

    private IEnumerator EmitDelayedNoise(Vector3 position, float radius)
    {
        // Wait for the custom delay
        yield return new WaitForSeconds(footstepDelay);

        noiseEmitter.EmitNoise(radius, NoiseType.Player, position);
    }
}