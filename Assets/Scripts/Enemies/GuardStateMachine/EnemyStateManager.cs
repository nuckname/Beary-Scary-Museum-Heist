using UnityEngine;

public class EnemyStateManager : MonoBehaviour, ISoundListener
{
    [Header("Pathfinding")]
    public Transform pathHolder;
    public float turnSpeed = 90f;
    public float waitTime = 2f;
    [HideInInspector] public Vector3[] waypoints;

    [Header("Investigation")]
    [HideInInspector] public Vector3 investigateTargetPosition;

    [Header("Behaviour Settings")]
    [Tooltip("If true, when the guards lose sight of the player, they will go to the player's last known location and look around. If false, they will instantly look around and then patrol.")]
    [InspectorName("Guards Go To Last Player Location When They Lose Sight")]
    public bool makeGuardsInvestiageLastPlayerLocationWhenTheyLoseSight = true;

    [Tooltip("if true, guards will make noise when they see the player, which can alert other guards. If false, guards will be silent when they see the player and just chase them.")]
    [InspectorName("Guards Make Noise When They First The Player")]
    public bool makeGuardsCreateNoiseWhenTheySeeThePlayer = true;
    public float guardNoiseRadiusWhenTheySeeThePlayer = 5f;

    [Header("Guard Turning")]
    [Tooltip("The amount of turns the guard does when they lose sight of the player and look around. " +
             "A turn is either looking left or right. So if this is 2, they will look left, then right, then go back to patrolling. " +
             "If this is 4, they will look left, right, left, right before going back to patrolling.")]
    public int minAmountOfTurnsTheGuardDoes = 2;
    public int maxAmountOfTurnsTheGuardDoes = 4;
    
    
    [Header("Guard Speed")]
    public float currentWalkSpeed;
    public float chaseSpeed = 4f;
    public float normalWalkSpeed = 2f;
    
    [Header("References")]
    public FieldOfView fieldOfView;
    public Transform playerTransform;
    [SerializeField] private NoiseEmitter noiseEmitter; 
    
    [Header("Guard Icons")]
    [SerializeField] private SpriteRenderer stateSpriteRenderer;
    [SerializeField] private Sprite heardIcon;
    [SerializeField] private Sprite seenIcon;
    [SerializeField] private Sprite patrollingIcon;
    [SerializeField] private Sprite confusedIcon;

    [Header("Noise")]
    [SerializeField] private NoiseType whatTypeOfNoiseTheGuardHeard = NoiseType.Nothing;
    private bool hasShoutedAtPlayer = false; // Moved from FieldOfView
    
    // State Instances
    [HideInInspector] public EnemyBaseState EnemyCurrentState; 
    [HideInInspector] public EnemyFollowPathState EnemyFollowPathState = new EnemyFollowPathState();
    [HideInInspector] public EnemyInvestigateALocationState EnemyInvestigateState = new EnemyInvestigateALocationState();
    [HideInInspector] public EnemyConfusedState enemyConfusedState = new EnemyConfusedState();
    [HideInInspector] public EnemyStunnedState EnemyStunnedState = new EnemyStunnedState();
    [HideInInspector] public EnemyChasePlayerState EnemyChasePlayerState = new EnemyChasePlayerState();

    [SerializeField] private Quaternion startingRotation;
    
    // Can make this an enum later
    [Header("Debug")]
    public string currentStateName;

    private void OnEnable()
    {
        if (fieldOfView != null)
        {
            fieldOfView.OnPlayerSpotted += HandlePlayerSpotted;
            fieldOfView.OnPlayerLost += HandlePlayerLost;
        }
    }

    private void OnDisable()
    {
        if (fieldOfView != null)
        {
            fieldOfView.OnPlayerSpotted -= HandlePlayerSpotted;
            fieldOfView.OnPlayerLost -= HandlePlayerLost;
        }
    }

    void Start()
    {
        if (pathHolder == null)
        {
            Debug.LogError("Path Holder is not assigned on " + gameObject.name);
            return;
        }
        
        startingRotation = transform.rotation;
        
        currentWalkSpeed = normalWalkSpeed;

        waypoints = new Vector3[pathHolder.childCount];
        for (int i = 0; i < pathHolder.childCount; i++)
        {
            waypoints[i] = pathHolder.GetChild(i).position;    
            waypoints[i] = new Vector3(waypoints[i].x, transform.position.y, waypoints[i].z);
        }

        SwitchState(EnemyFollowPathState);
    }
    
    void Update()
    {
        EnemyCurrentState?.UpdateState(this);
    }

    // Use this to switch states
    public void SwitchState(EnemyBaseState state)
    {
        EnemyCurrentState = state;
        
        if (EnemyCurrentState != null)
        {
            currentStateName = EnemyCurrentState.GetType().Name;
            EnemyCurrentState.EnterState(this);
        }
    }

    // Event Handlers
    // Called from FieldOfView.cs
    private void HandlePlayerSpotted(Transform target)
    {
        // Shouting logic 
        if (!hasShoutedAtPlayer && makeGuardsCreateNoiseWhenTheySeeThePlayer)
        {
            noiseEmitter.EmitNoise(guardNoiseRadiusWhenTheySeeThePlayer, NoiseType.Nothing);
            hasShoutedAtPlayer = true;
        }
        
        StartChasing(target);
    }

    // Called from FieldOfView.cs
    private void HandlePlayerLost(Vector3 lastKnownPosition)
    {
        // Investigation logic
        if (makeGuardsInvestiageLastPlayerLocationWhenTheyLoseSight)
        {
            investigateTargetPosition = lastKnownPosition;
            SwitchState(EnemyInvestigateState);
        }
        else
        {
            SwitchState(enemyConfusedState);
        }
        
        hasShoutedAtPlayer = false;
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        EnemyCurrentState?.OnCollisionEnter(this, collision);
    }
    public void StartChasing(Transform target)
    {
        playerTransform = target;

        // Only switch state if we aren't ALREADY chasing them and NOT stunned
        if (EnemyCurrentState != EnemyChasePlayerState && EnemyCurrentState != EnemyStunnedState)
        {
            SwitchState(EnemyChasePlayerState);
        }
    }

    // This is the method the NoiseEmitter calls when it hears something
    public void OnSoundHeard(Vector3 targetLocation, Transform sourceTransform, NoiseType noiseType)
    {
        Debug.Log("OnSoundHeard");
        // VISUAL PRIORITY: If we are actively chasing the player or stunned, ignore ALL other noises. Vision overrides hearing.
        if (EnemyCurrentState == EnemyChasePlayerState || EnemyCurrentState == EnemyStunnedState) 
            return;

        // SPAM PREVENTION: Don't trigger if we are already investigating this exact spot
        if (EnemyCurrentState == EnemyInvestigateState && investigateTargetPosition == targetLocation) 
            return;

        // NOISE PRIORITY: If we are already investigating a Player noise, ignore Item noises.
        if (EnemyCurrentState == EnemyInvestigateState)
        {
            if (whatTypeOfNoiseTheGuardHeard == NoiseType.Player && noiseType == NoiseType.Item)
            {
                // We heard an item, but we are already looking for a player noise. Ignore the item.
                return; 
            }
        }

        // If we made it this far, either we weren't investigating anything, 
        // OR the new noise is higher/equal priority to the old noise, therefore update our targets.
        whatTypeOfNoiseTheGuardHeard = noiseType;
        investigateTargetPosition = targetLocation;
        SwitchState(EnemyInvestigateState);
    }
    
    public void SetStateIcon(EnemyStateIcon iconType)
    {
        switch (iconType)
        {
            case EnemyStateIcon.Heard:
                stateSpriteRenderer.sprite = heardIcon;
                stateSpriteRenderer.enabled = true;
                break;
            case EnemyStateIcon.Seen:
                stateSpriteRenderer.sprite = seenIcon;
                stateSpriteRenderer.enabled = true;
                break;
            case EnemyStateIcon.Patrolling:
                stateSpriteRenderer.sprite = patrollingIcon;
                stateSpriteRenderer.enabled = true;
                break;
            case EnemyStateIcon.Confused:
                stateSpriteRenderer.sprite = confusedIcon;
                stateSpriteRenderer.enabled = true;
                break;
            case EnemyStateIcon.Hide:
                stateSpriteRenderer.sprite = null;
                stateSpriteRenderer.enabled = false;
                break;
        }
    }
}