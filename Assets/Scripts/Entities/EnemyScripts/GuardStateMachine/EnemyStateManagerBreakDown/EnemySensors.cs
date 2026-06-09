using UnityEngine;

/// <summary>
/// Processes all environmental stimuli for the enemy, including vision and hearing. 
/// Evaluates noise priorities and visual detection to trigger appropriate state changes, while also emitting local alert noises.
/// </summary>
public class EnemySensors : MonoBehaviour, ISoundListener
{
    private EnemyStateManager manager;
    private EnemyVisuals visuals;

    [Header("References")]
    public FieldOfView[] fieldOfViews;
    public Transform playerTransform;
    [SerializeField] private NoiseEmitter noiseEmitter; 

    [Header("Hearing Settings")]
    public FieldOfView hearingFOV;
    
    [Header("Noise")]
    [SerializeField] private NoiseType whatTypeOfNoiseTheGuardHeard = NoiseType.Nothing;
    private bool hasShoutedAtPlayer = false;

    [Tooltip("if true, guards will make noise when they see the player, which can alert other guards. If false, guards will be silent when they see the player and just chase them.")]
    [InspectorName("Guards Make Noise When They First The Player")]
    public bool makeGuardsCreateNoiseWhenTheySeeThePlayer = true;
    public float guardNoiseRadiusWhenTheySeeThePlayer = 5f;

    [Header("Patrol Turning Vision Reduction")]
    [Tooltip("Percentage to reduce vision radius when turning. 0.5 = 50% reduction.")]
    [Range(0f, 1f)]
    public float turnVisionRadiusReductionPercentage = 0.5f; 
    public float fovTurnAngleThreshold = 25f;

    [HideInInspector] public bool isCurrentlySeeingPlayer = false;

    private void Awake()
    {
        manager = GetComponent<EnemyStateManager>();
        visuals = GetComponent<EnemyVisuals>();
    }

    private void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
    }

    private void OnEnable()
    {
        if (fieldOfViews != null)
        {
            foreach (var fov in fieldOfViews)
            {
                if (fov != null)
                {
                    fov.OnPlayerSpotted += HandlePlayerSpotted;
                    fov.OnPlayerLost += HandlePlayerLost;
                }
            }
        }
    }

    private void OnDisable()
    {
        if (fieldOfViews != null)
        {
            foreach (var fov in fieldOfViews)
            {
                if (fov != null)
                {
                    fov.OnPlayerSpotted -= HandlePlayerSpotted;
                    fov.OnPlayerLost -= HandlePlayerLost;
                }
            }
        }
    }

    // Called from FieldOfView.cs
    private void HandlePlayerSpotted(Transform target)
    {
        isCurrentlySeeingPlayer = true;

        // Shouting logic 
        if (!hasShoutedAtPlayer && makeGuardsCreateNoiseWhenTheySeeThePlayer)
        {
            noiseEmitter.EmitNoise(guardNoiseRadiusWhenTheySeeThePlayer, NoiseType.Nothing);
            hasShoutedAtPlayer = true;
        }
        
        visuals.animator.SetBool("isAlert", true);
        manager.StartChasing(target);
    }

    // Called from FieldOfView.cs
    private void HandlePlayerLost(Vector3 lastKnownPosition)
    {
        bool isPlayerStillSeenByAnotherFOV = false;
        if (fieldOfViews != null)
        {
            foreach (var fov in fieldOfViews)
            {
                if (fov != null)
                {
                    foreach (Transform visibleTarget in fov.visibleTargets)
                    {
                        if (visibleTarget != null && visibleTarget.CompareTag("Player"))
                        {
                            isPlayerStillSeenByAnotherFOV = true;
                            break;
                        }
                    }
                }
                if (isPlayerStillSeenByAnotherFOV) break;
            }
        }

        if (isPlayerStillSeenByAnotherFOV) return;

        isCurrentlySeeingPlayer = false;

        // Investigation logic
        if (manager.makeGuardsInvestiageLastPlayerLocationWhenTheyLoseSight)
        {
            manager.investigateTargetPosition = lastKnownPosition;
            manager.SwitchState(manager.EnemyInvestigateState);
        }
        else
        {
            manager.SwitchState(manager.enemyConfusedState);
        }
        
        hasShoutedAtPlayer = false;
    }

    // This is the method the NoiseEmitter calls when it hears something
    public void OnSoundHeard(Vector3 targetLocation, Transform sourceTransform, NoiseType noiseType)
    {
        print("Guard " + gameObject.name + " heard a noise of type " + noiseType + " at location " + targetLocation);
        
        // VISUAL PRIORITY: If we are actively chasing the player or stunned, ignore ALL other noises. Vision overrides hearing.
        if (manager.EnemyCurrentState == manager.EnemyChasePlayerState || manager.EnemyCurrentState == manager.EnemyStunnedState) 
            return;

        // SPAM PREVENTION: Don't trigger if we are already investigating this exact spot
        if (manager.EnemyCurrentState == manager.EnemyInvestigateState && manager.investigateTargetPosition == targetLocation) 
            return;

        // NOISE PRIORITY: If we are already investigating a Player noise, ignore Item noises.
        if (manager.EnemyCurrentState == manager.EnemyInvestigateState)
        {
            if (whatTypeOfNoiseTheGuardHeard == NoiseType.Player && noiseType == NoiseType.Item)
            {
                // We heard an item, but we are already looking for a player noise. Ignore the item.
                return; 
            }
        }
        
        visuals.SetStateIcon(EnemyStateIcon.HeardASound);

        // If we made it this far, either we weren't investigating anything, 
        // OR the new noise is higher/equal priority to the old noise, therefore update our targets.
        whatTypeOfNoiseTheGuardHeard = noiseType;
        manager.investigateTargetPosition = targetLocation;
        
        manager.turnTargetPosition = targetLocation;
        manager.stateToSwitchToAfterTurning = manager.EnemyInvestigateState;
        
        manager.SwitchState(manager.EnemyTurnToPointState);
    }

    public void RestoreAllFOVRadiuses()
    {
        if (fieldOfViews != null)
        {
            foreach (var fov in fieldOfViews)
            {
                if (fov != null) fov.RestoreFOVRadius();
            }
        }
    }
}