using UnityEngine;

[RequireComponent(typeof(NoiseEmitter), typeof(AudioSource))]
public class CollisionNoiseTrigger : PickupItem
{
    private NoiseEmitter noiseEmitter;
    private AudioSource audioSource;
    
    [Header("Collision Settings")]
    [SerializeField] private float dropSoundMultiplier = 1f;
    
    [Header("Velocity Settings")]
    [SerializeField] private bool useVelocityScaling = true;
    [SerializeField] private float velocityScale = 0.5f; 
    [SerializeField] private float minVelocityThreshold = 2f;

    protected override void Awake()
    {
        base.Awake(); 

        audioSource = GetComponent<AudioSource>();
        noiseEmitter = GetComponent<NoiseEmitter>();
    }


    private void OnCollisionEnter(Collision collision)
    {
        // Ignore collisions with the player
        if (collision.gameObject.CompareTag("Player")) return;

        float finalMultiplier = dropSoundMultiplier;

        if (useVelocityScaling)
        {
            float impactVelocity = collision.relativeVelocity.magnitude;
            
            if (impactVelocity >= minVelocityThreshold)
            {
                finalMultiplier = dropSoundMultiplier * (1f + (impactVelocity * velocityScale));
            }
        }

        float noiseRadius = rb.mass * finalMultiplier;
        
        audioSource.maxDistance = noiseRadius;
        audioSource.Play();
        
        noiseEmitter.EmitNoise(noiseRadius, NoiseType.Item);
    }
}