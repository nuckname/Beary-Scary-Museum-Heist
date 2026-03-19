using UnityEngine;

public class NoiseTimerMoveForwardItem : MonoBehaviour
{
    private NoiseEmitter noiseEmitter;

    [Header("Movement Settings")]
    [Tooltip("How fast the item moves forward.")]
    public float moveSpeed = 3f;
    
    [Header("Noise Settings")]
    [Tooltip("How often the noise triggers (in seconds).")]
    public float noiseInterval = 0.5f;
    
    [Tooltip("The radius of the emitted noise to alert Guards /ai.")]
    public float noiseRadius = 4f;

    private float timer = 0f;

    private void Start()
    {
        noiseEmitter = GetComponent<NoiseEmitter>();
        
        //timer = 0f; 
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * (moveSpeed * Time.deltaTime));

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            TriggerNoise();
            
            // Reset the timer
            timer = noiseInterval; 
        }
    }

    private void TriggerNoise()
    {
        noiseEmitter.EmitNoise(noiseRadius, transform.position);
        
        // Call audio here
    }
}
