using System;
using UnityEngine;
using UnityEngine.AI;

public class EnemyStateManager : MonoBehaviour, ISoundListener
{
    [Header("Pathfinding")]
    public Transform pathHolder;
    public float turnSpeed = 90f;
    public float waitTime = 2f;
    [HideInInspector] public Vector3[] waypoints;

    [Header("Path Visuals")] 
    public float guardPatrollSpeed = 3f;
    public float guardChaseSpeed = 3f;
    
    [Header("Chase Escalation Settings")]
    public float maxChaseSpeed = 7f;
    public float chaseAcceleration = 1.5f;
    private float currentChaseSpeed;
    private bool isCurrentlySeeingPlayer = false;

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
    public int amountOfTimesTheGuardTurns = 2;
    public int turnAngle = 40;

    [Header("Patrol Turning Vision Reduction")]
    [Tooltip("Percentage to reduce vision radius when turning. 0.5 = 50% reduction.")]
    [Range(0f, 1f)]
    public float turnVisionRadiusReductionPercentage = 0.5f; 
    public float fovTurnAngleThreshold = 25f;
    
    [Header("References")]
    public FieldOfView[] fieldOfViews;
    public Transform playerTransform;
    [SerializeField] private NoiseEmitter noiseEmitter; 
    
    [Header("Hearing Settings")]
    public FieldOfView hearingFOV;
    
    [Header("Guard Icons")]
    [SerializeField] private SpriteRenderer stateSpriteRenderer;
    [SerializeField] private Sprite heardASoundIcon;
    [SerializeField] private Sprite chasingPlayerIcon;
    [SerializeField] private Sprite lookingAroundConfusedIcon;
    [SerializeField] private Sprite guardIsStunnedIcon;
    [SerializeField] private Sprite guardHasFinishedLookingAroundAndDidntFindAnythingSoBackToPatrolling;

    [Header("Noise")]
    [SerializeField] private NoiseType whatTypeOfNoiseTheGuardHeard = NoiseType.Nothing;
    private bool hasShoutedAtPlayer = false; // Moved from FieldOfView
    
    [Header("Movement Settings")]
    [Tooltip("If true, the guard will only rotate clockwise. If false, they will take the shortest rotation path.")]
    public bool alwaysTurnRight = false;
    
    // Add this to remember the guard's specific inspector setting
    [HideInInspector] public bool defaultAlwaysTurnRight;
    
    // State Instances
    [HideInInspector] public EnemyBaseState EnemyCurrentState; 
    [HideInInspector] public EnemyFollowPathState EnemyFollowPathState = new EnemyFollowPathState();
    [HideInInspector] public EnemyInvestigateALocationState EnemyInvestigateState = new EnemyInvestigateALocationState();
    [HideInInspector] public EnemyConfusedState enemyConfusedState = new EnemyConfusedState();
    [HideInInspector] public EnemyStunnedState EnemyStunnedState = new EnemyStunnedState();
    [HideInInspector] public EnemyChasePlayerState EnemyChasePlayerState = new EnemyChasePlayerState();
    
    // This is currently keeping track of what waypoint to turn towards when are we are a state.
    [HideInInspector] public int currentWaypointIndex = 0;
    
    // Before we use EnemeyTurnState, we must allow tell it what state to go to afterwards using stateToSwitchToAfterTurning
    [HideInInspector] public EnemyTurnToPointState EnemyTurnToPointState = new EnemyTurnToPointState();
    [HideInInspector] public Vector3 turnTargetPosition;
    public EnemyBaseState stateToSwitchToAfterTurning;
    
    [HideInInspector] public NavMeshAgent agent;
    
    [SerializeField] private Quaternion startingRotation;
    
    [Header("Path Visuals Setup")] 
    public bool showGuardPaths = false;
    [SerializeField] private Color pathColor = Color.red; 
    [SerializeField] private float pathLineWidth = 0.15f;
    [HideInInspector] public LineRenderer lineRenderer;

    public Animator animator;
    public bool playerLeftTheGuardsFovOnRightSide = false;
    // Can make this an enum later
    [Header("Debug")]
    public string currentStateName;

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

    private void Awake()
    {
        defaultAlwaysTurnRight = alwaysTurnRight;
        
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        if (pathHolder == null)
        {
            Debug.LogError("Path Holder is not assigned on " + gameObject.name);
            return;
        }

        SetupPatrolRoute();

        if (showGuardPaths)
        {
            SetUpGuardPathingLines();
        }

        SwitchState(EnemyFollowPathState);

        enemyConfusedState.lookAngle = turnAngle;

        playerTransform = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();

        //ResetAnimations();
        
        // Freeze guards until we want them to start moving called in RoundStateManager
        agent.isStopped = true;
        agent.speed = 0;
    }

    public void ResetAnimations()
    {
        animator.SetBool("IsWalking", true);
        animator.SetBool("IsChasing", false);
        animator.SetBool("IsCollideWithPlayer", false);
        animator.SetBool("IsStunned", false);
        animator.SetBool("IsAlerted", false);
    }
    
    private void SetupPatrolRoute()
    {
        waypoints = new Vector3[pathHolder.childCount];
        for (int i = 0; i < pathHolder.childCount; i++)
        {
            waypoints[i] = pathHolder.GetChild(i).position;    
            waypoints[i] = new Vector3(waypoints[i].x, transform.position.y, waypoints[i].z);
        }
    }

    public void InitialiseGuardStartMoving()
    {
        agent.speed = guardPatrollSpeed;
        agent.isStopped = false;
    }

    public void GuardStopMoving()
    {
        agent.isStopped = true;
    }
    
    public void GuardStartMoving()
    {
        agent.isStopped = false;
    }

    public void SetUpGuardPathingLines()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        
        // Colour
        lineRenderer.startColor = pathColor;
        lineRenderer.endColor = pathColor;
        
        // Width
        lineRenderer.startWidth = pathLineWidth;
        lineRenderer.endWidth = pathLineWidth;

        // Make the lines on the ground
        Vector3[] linePositions = new Vector3[waypoints.Length];
        for (int i = 0; i < waypoints.Length; i++)
        {
            linePositions[i] = new Vector3(waypoints[i].x, 0f, waypoints[i].z);
        }
        
        lineRenderer.loop = true;
        
        lineRenderer.positionCount = linePositions.Length;
        lineRenderer.SetPositions(linePositions);
    }

    void Update()
    {
        EnemyCurrentState?.UpdateState(this);

        IncreaseGuardSpeedOverTime();
    }

    private void IncreaseGuardSpeedOverTime()
    {
        if (isCurrentlySeeingPlayer && EnemyCurrentState == EnemyChasePlayerState)
        {
            if (currentChaseSpeed < maxChaseSpeed)
            {
                currentChaseSpeed += chaseAcceleration * Time.deltaTime;
                agent.speed = Mathf.Min(currentChaseSpeed, maxChaseSpeed);
                //Debug.Log("Enemy Speed: " + agent.speed);
            }
        }
    }

    // Use this to switch states
    public void SwitchState(EnemyBaseState state)
    {
        Debug.Log($"Switching State TO: {state.GetType().Name}");
        
        EnemyCurrentState = state;
        
        if (fieldOfViews != null)
        {
            foreach (var fov in fieldOfViews)
            {
                if (fov != null)
                {
                    fov.RestoreFOVRadius();
                }
            }
        }

        // Always turn false but then override this for something in a specific state. 
        alwaysTurnRight = false;
        
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
        isCurrentlySeeingPlayer = true;

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
            currentChaseSpeed = guardChaseSpeed;
            agent.speed = currentChaseSpeed;
            SwitchState(EnemyChasePlayerState);
        }
    }

    // This is the method the NoiseEmitter calls when it hears something
    public void OnSoundHeard(Vector3 targetLocation, Transform sourceTransform, NoiseType noiseType)
    {
        print("Guard " + gameObject.name + " heard a noise of type " + noiseType + " at location " + targetLocation);
        
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
        
        SetStateIcon(EnemyStateIcon.HeardASound);

        // If we made it this far, either we weren't investigating anything, 
        // OR the new noise is higher/equal priority to the old noise, therefore update our targets.
        whatTypeOfNoiseTheGuardHeard = noiseType;
        investigateTargetPosition = targetLocation;
        
        turnTargetPosition = targetLocation;
        stateToSwitchToAfterTurning = EnemyInvestigateState;
        
        SwitchState(EnemyTurnToPointState);
    }
    
    public void SetStateIcon(EnemyStateIcon iconType)
    {
        switch (iconType)
        {
            case EnemyStateIcon.HeardASound:
                stateSpriteRenderer.sprite = heardASoundIcon;
                stateSpriteRenderer.enabled = true;
                break;
            case EnemyStateIcon.ChasingPlayer:
                stateSpriteRenderer.sprite = chasingPlayerIcon;
                stateSpriteRenderer.enabled = true;
                break;
            case EnemyStateIcon.LookingAroundConfused:
                stateSpriteRenderer.sprite = lookingAroundConfusedIcon;
                stateSpriteRenderer.enabled = true;
                break;
            case EnemyStateIcon.HideIcon:
                stateSpriteRenderer.sprite = null;
                stateSpriteRenderer.enabled = false;
                break;
            case EnemyStateIcon.FinishedLookingAroundAndDidntFindAnythingSoBackToPatrolling:
                stateSpriteRenderer.sprite = guardHasFinishedLookingAroundAndDidntFindAnythingSoBackToPatrolling;
                stateSpriteRenderer.enabled = true;
                break;
        }
    }
    
    private void OnDrawGizmos()
    {
        if (pathHolder == null || !showGuardPaths) return;

        Gizmos.color = pathColor;

        for (int i = 0; i < pathHolder.childCount - 1; i++)
        {
            Vector3 startPos = pathHolder.GetChild(i).position;
            Vector3 endPos = pathHolder.GetChild(i + 1).position;

            Gizmos.DrawLine(startPos, endPos);
        }

        // Connect last waypoint back to first
        if (pathHolder.childCount > 1)
        {
            Vector3 lastPos = pathHolder.GetChild(pathHolder.childCount - 1).position;
            Vector3 firstPos = pathHolder.GetChild(0).position;

            Gizmos.DrawLine(lastPos, firstPos);
        }
    }
}