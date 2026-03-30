using UnityEngine;

[RequireComponent(typeof(NoiseEmitter), typeof(AudioSource))]
public class CollisionNoiseTrigger : PickupItem
{
    private NoiseEmitter noiseEmitter;
    private AudioSource audioSource;
    
    [Header("Collision Settings")]
    [SerializeField] private float dropSoundMultiplier = 1f;

    public bool canOnlyStunWhenAirBorne = true; 
    public bool objectIsAirborne = true;
    
    [Header("Velocity Settings")]
    [SerializeField] private bool useVelocityScaling = true;
    [SerializeField] private float velocityScale = 0.5f; 
    [SerializeField] private float minVelocityThreshold = 2f;

    
    private int groundLayerIndex;
    private int obstacleLayerIndex;
    
    protected override void Awake()
    {
        base.Awake(); 

        audioSource = GetComponent<AudioSource>();
        noiseEmitter = GetComponent<NoiseEmitter>();

        // Important Cache 
        groundLayerIndex = LayerMask.NameToLayer("Ground");
        obstacleLayerIndex = LayerMask.NameToLayer("Obstacle");
    }
    
    public override void OnPickedUp()
    {
        base.OnPickedUp();
        objectIsAirborne = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Ignore collisions with the player
        if (collision.gameObject.CompareTag("Player")) return;

        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (collision.gameObject.TryGetComponent(out EnemyStateManager enemy))
            {
                if (canOnlyStunWhenAirBorne && objectIsAirborne)
                {
                    enemy.SwitchState(enemy.EnemyStunnedState);
                }
            }
        }
        
        Debug.Log($"Item hit: {collision.gameObject.name} | " +
                  $"It is on Layer: {LayerMask.LayerToName(collision.gameObject.layer)}");
        
        if (collision.gameObject.layer == groundLayerIndex || collision.gameObject.layer == obstacleLayerIndex)
        {
            objectIsAirborne = false;
        }
        
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