using UnityEngine;

[RequireComponent(typeof(NoiseEmitter))]
[RequireComponent(typeof(Rigidbody))]
public class NoiseTimerMoveForwardItem : MonoBehaviour, IThrowableItem
{
    private NoiseEmitter noiseEmitter;
    private Rigidbody rb;

    [Header("Movement Settings")]
    [Tooltip("How fast the item moves forward.")]
    public float moveSpeed = 3f;
    
    [Header("Noise Settings")]
    [Tooltip("How often the noise triggers (in seconds).")]
    public float noiseInterval = 0.5f;
    
    [Tooltip("The radius of the emitted noise to alert Guards /ai.")]
    public float noiseRadius = 4f;

    private float timer = 0f;
    
    // IThrowableItem variables
    private bool isArmed = false;
    private bool isActivated = false;

    private void Start()
    {
        noiseEmitter = GetComponent<NoiseEmitter>();
        rb = GetComponent<Rigidbody>();
    }

    // Called by the PlayerThrowController via the interface
    public void OnThrown()
    {
        isArmed = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // If it was thrown and hits anything but the player, activate it!
        if (isArmed && !collision.gameObject.CompareTag("Player"))
        {
            isArmed = false;
            isActivated = true;
            timer = 0f; // Trigger the first noise immediately on impact

            if (rb != null)
            {
                rb.isKinematic = true;
            }

            // Flatten the rotation so it drives nicely along the floor sort of parallel to the ground
            Vector3 flatEuler = transform.eulerAngles;
            flatEuler.x = 0;
            flatEuler.z = 0;
            transform.eulerAngles = flatEuler;
        }
    }

    private void Update()
    {
        if (!isActivated) return;

        transform.Translate(Vector3.forward * (moveSpeed * Time.deltaTime));

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            TriggerNoise();
            timer = noiseInterval; 
        }
    }

    private void TriggerNoise()
    {
        noiseEmitter.EmitNoise(noiseRadius, transform.position);
        
        // Call audio here
    }
}