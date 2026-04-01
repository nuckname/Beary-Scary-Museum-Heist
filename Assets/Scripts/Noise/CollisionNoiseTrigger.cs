using UnityEngine;

[RequireComponent(typeof(NoiseEmitter), typeof(AudioSource))]
public class CollisionNoiseTrigger : PickupItem
{
    private NoiseEmitter noiseEmitter;
    private AudioSource audioSource;
    
    [Header("Collision Settings")]
    [SerializeField] private float dropSoundMultiplier = 1f;

    public bool canOnlyStunWhenAirBorne = true; 
    public bool objectCanStunGuard = true;
    
    [Header("Velocity Settings")]
    [SerializeField] private bool useVelocityScaling = true;
    [SerializeField] private float velocityScale = 0.5f; 
    [SerializeField] private float minVelocityThreshold = 2f;

    [SerializeField] private LayerMask whatIsGround;
    
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
        objectCanStunGuard = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Ignore collisions with the player
        if (collision.gameObject.CompareTag("Player")) return;

        // Stun Logic -> might need to refactor to a new script 
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (collision.gameObject.TryGetComponent(out EnemyStateManager enemy))
            {
                if (canOnlyStunWhenAirBorne && objectCanStunGuard)
                {
                    enemy.SwitchState(enemy.EnemyStunnedState);
                }
            }
        }

        print(collision.gameObject.layer + " " + groundLayerIndex);

        // Bitwise check: Is the collision layer included in the mask?
        // Layer Index and a Layer Mask are two different math values we cant compare them, need this instead
        if ((whatIsGround.value & (1 << collision.gameObject.layer)) != 0)
        {
            objectCanStunGuard = false;
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