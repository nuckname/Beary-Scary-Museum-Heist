using UnityEngine;

[RequireComponent(typeof(NoiseEmitter), typeof(Rigidbody))]
public class CollisionNoiseTrigger : MonoBehaviour
{
    [Header("Collision Sound Settings")]
    [SerializeField] private AudioClip impactClip; 
    [SerializeField] private float dropSoundMultiplier = 1f;
    [SerializeField] private bool useVelocityScaling = true;
    [SerializeField] private float velocityScale = 0.5f; 
    [SerializeField] private float minVelocityThreshold = 2f;

    [Header("Pitch Scaling Settings")]
    [SerializeField] private bool usePitchScaling = true;
    [Tooltip("The pitch when the noise radius is very small (High pitch)")]
    [SerializeField] private float maxPitch = 1.2f; 
    [Tooltip("The pitch when the noise radius is massive (Deep pitch)")]
    [SerializeField] private float minPitch = 0.4f; 
    [Tooltip("The noise radius size required to hit the absolute lowest/deepest pitch")]
    [SerializeField] private float maxExpectedRadius = 30f; 

    private NoiseEmitter noiseEmitter;
    private Rigidbody rb;

    private void Awake()
    {
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
            finalMultiplier = GetVelocity(collision, finalMultiplier);
        }
        
        SpawnFloatingCartoonImage(collision);

        float noiseRadius = rb.mass * finalMultiplier;
        
        // --- NEW: Calculate the pitch, then send it to the AudioManager ---
        float targetPitch = 1f;

        if (usePitchScaling)
        {
            float normalizedRadius = Mathf.Clamp01(noiseRadius / maxExpectedRadius);
            targetPitch = Mathf.Lerp(maxPitch, minPitch, normalizedRadius);
        }

        // Send the clip and the specific pitch to the global manager
        if (AudioManager.instance != null && impactClip != null)
        {
            AudioManager.instance.PlaySFXWithExactPitch(impactClip, targetPitch);
        }
        // ------------------------------------------------------------------
        
        noiseEmitter.EmitNoise(noiseRadius, NoiseType.Item);
    }

    private float GetVelocity(Collision collision, float finalMultiplier)
    {
        float impactVelocity = collision.relativeVelocity.magnitude;

        if (impactVelocity >= minVelocityThreshold)
        {
            finalMultiplier = dropSoundMultiplier * (1f + (impactVelocity * velocityScale));
        }

        return finalMultiplier;
    }

    private void SpawnFloatingCartoonImage(Collision collision)
    {
        float horizontalSpread = 1.5f; 

        float randomX = Random.Range(-horizontalSpread, horizontalSpread);
        float randomZ = Random.Range(-horizontalSpread, horizontalSpread); 
        float randomY = Random.Range(2f, 3f);

        Vector3 randomOffset = new Vector3(randomX, randomY, randomZ);
        Vector3 spawnPosition = collision.contacts[0].point + randomOffset;

        if (ImagePopUpSpawnerManager.Instance != null)
        {
            ImagePopUpSpawnerManager.Instance.SpawnRandomImpact(spawnPosition);
        }
    }
}