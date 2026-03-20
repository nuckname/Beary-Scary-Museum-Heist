using UnityEngine;

[RequireComponent(typeof(NoiseEmitter))]
public class PlayerFootstepNoise : MonoBehaviour
{
    private NoiseEmitter noiseEmitter;
    [SerializeField] private CharacterController controller;

    [Header("Footstep Settings")]
    [Tooltip("How often a footstep sound triggers when moving (in seconds).")]
    public float stepInterval = 0.5f;
    
    [Tooltip("Base radius of the footstep noise.")]
    public float baseNoiseRadius = 2f;
    
    [Tooltip("How much the players speed multiplies the noise. Walking vs Sprinting.")]
    public float speedMultiplier = 1f;

    private float stepTimer = 0f;

    private void Start()
    {
        noiseEmitter = GetComponent<NoiseEmitter>();
    }

    private void Update()
    {
        float currentSpeed = controller.velocity.magnitude;
        
        if (currentSpeed > 0.1f)
        {
            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0f)
            {
                TriggerFootstep(currentSpeed);
                
                // Reset timer
                stepTimer = stepInterval; 
            }
        }
        else
        {
            // Reset timer so the first step happens immediately when starting to move again
            stepTimer = 0f; 
        }
    }

    private void TriggerFootstep(float speed)
    {
        // Calculate dynamic radius based on velocity
        float calculatedRadius = baseNoiseRadius + (speed * speedMultiplier);

        noiseEmitter.EmitNoise(calculatedRadius);
    }
}