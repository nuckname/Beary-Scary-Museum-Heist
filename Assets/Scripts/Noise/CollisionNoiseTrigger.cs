using System;
using UnityEngine;

[RequireComponent(typeof(NoiseEmitter))]
[RequireComponent(typeof(AudioSource))]
public class CollisionNoiseTrigger : MonoBehaviour
{
    [SerializeField] private NoiseEmitter noiseEmitter;
    [SerializeField] private AudioSource audioSource;

    private Rigidbody rb; 
    
    [Header("Collision Settings")]
    [SerializeField] private float dropSoundMultiplier = 1f;
    
    [Header("Velocity Settings")]
    [SerializeField] private bool useVelocityScaling = true;
    [SerializeField] private float velocityScale = 0.5f; 
    [SerializeField] private float minVelocityThreshold = 2f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        if (noiseEmitter == null) noiseEmitter = GetComponent<NoiseEmitter>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    private void OnCollisionEnter(Collision collision)
    {
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

        Vector3 hitPoint = collision.GetContact(0).point;
        noiseEmitter.EmitNoise(noiseRadius, hitPoint);
    }
}