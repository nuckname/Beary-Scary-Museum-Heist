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
        
        // Fixes a bug where if enemy hits an object on the ground we don't want to make a noise or the enemy gets perma stunned as it keeps hitting the enemy.
        if (collision.gameObject.CompareTag("Enemy")) return;

        float finalMultiplier = dropSoundMultiplier;

        if (useVelocityScaling)
        {
            float impactVelocity = collision.relativeVelocity.magnitude;
            
            if (impactVelocity >= minVelocityThreshold)
            {
                finalMultiplier = dropSoundMultiplier * (1f + (impactVelocity * velocityScale));
            }
        }
        
        // Spawn impact image
        Vector3 spawnPosition = collision.contacts[0].point + (Vector3.up * Random.Range(2f, 3f));
            
        if (ImagePopUpSpawnerManager.Instance != null)
        {
            ImagePopUpSpawnerManager.Instance.SpawnRandomImpact(spawnPosition);
        }

        float noiseRadius = rb.mass * finalMultiplier;
        
        audioSource.maxDistance = noiseRadius;
        audioSource.Play();
        
        noiseEmitter.EmitNoise(noiseRadius, NoiseType.Item);
    }
}