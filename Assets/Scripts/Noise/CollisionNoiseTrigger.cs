using UnityEngine;

[RequireComponent(typeof(NoiseEmitter), typeof(AudioSource), typeof(Rigidbody))]
public class CollisionNoiseTrigger : MonoBehaviour
{
    [Header("Collision Sound Settings")]
    [SerializeField] private float dropSoundMultiplier = 1f;
    [SerializeField] private bool useVelocityScaling = true;
    [SerializeField] private float velocityScale = 0.5f; 
    [SerializeField] private float minVelocityThreshold = 2f;

    private NoiseEmitter noiseEmitter;
    private AudioSource audioSource;
    private Rigidbody rb;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        noiseEmitter = GetComponent<NoiseEmitter>();
        rb = GetComponent<Rigidbody>();
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

        // We use the Rigidbody mass, which is naturally set by your actual PickupItem/Gun script!
        float noiseRadius = rb.mass * finalMultiplier;
        
        audioSource.maxDistance = noiseRadius;
        audioSource.Play();
        
        noiseEmitter.EmitNoise(noiseRadius, NoiseType.Item);
    }
}