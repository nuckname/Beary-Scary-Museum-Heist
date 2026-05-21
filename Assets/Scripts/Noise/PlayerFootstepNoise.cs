using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NoiseEmitter))]
public class PlayerFootstepNoise : MonoBehaviour
{
    private NoiseEmitter noiseEmitter;
    [SerializeField] private Rigidbody rb;
    
    private PlayerStealthController stealthController; 

    [Header("Footstep Settings")]
    [Tooltip("Base radius of the footstep noise.")]
    public float baseNoiseRadius = 2f;
    
    [Tooltip("How much the players speed multiplies the noise. Walking vs Sprinting.")]
    public float speedMultiplier = 1f;

    [Tooltip("Multiplier applied to the noise radius when sneaking (e.g., 0.2 means 20% normal volume).")]
    public float sneakNoiseMultiplier = 0.2f;

    [Header("Weight Settings")]
    public float weightToRadiusMultiplier = 0f;

    [Header("Delay Settings")]
    public float footstepDelay = 0f;

    private float currentHeldWeight = 0f; 
    
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

    // Called by animation event in player walk animation
    public void OnFootstepEvent()
    {
        float currentSpeed = rb.linearVelocity.magnitude;
        
        if (currentSpeed > 0.1f)
        {
            TriggerFootstep(currentSpeed);
        }
    }

    private void TriggerFootstep(float speed)
    {
        float calculatedRadius = baseNoiseRadius + (speed * speedMultiplier) + (currentHeldWeight * weightToRadiusMultiplier);

        if (stealthController != null && stealthController.IsSneaking)
        {
            calculatedRadius *= sneakNoiseMultiplier;
            recoveryStepsRemaining = 3; 
        }
        else if (recoveryStepsRemaining > 0)
        {
            if (recoveryStepsRemaining == 3) calculatedRadius *= 0.25f;     
            else if (recoveryStepsRemaining == 2) calculatedRadius *= 0.50f; 
            else if (recoveryStepsRemaining == 1) calculatedRadius *= 0.75f; 

            recoveryStepsRemaining--;
        }

        Vector3 stepPosition = transform.position;

        if (footstepDelay > 0f)
        {
            StartCoroutine(EmitDelayedNoise(stepPosition, calculatedRadius));
        }
        else
        {
            // Emit instantly for 1:1 animation sync
            noiseEmitter.EmitNoise(calculatedRadius, NoiseType.Player, stepPosition);
        }
    }

    private IEnumerator EmitDelayedNoise(Vector3 position, float radius)
    {
        yield return new WaitForSeconds(footstepDelay);
        noiseEmitter.EmitNoise(radius, NoiseType.Player, position);
    }
}