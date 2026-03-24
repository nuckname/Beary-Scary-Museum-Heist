using TMPro;
using UnityEngine;

public class EnemyStateManager : MonoBehaviour
{
    [Header("Pathfinding")]
    public Transform pathHolder;
    public float turnSpeed = 90f;
    public float waitTime = 2f;
    [HideInInspector] public Vector3[] waypoints;

    [Header("Investigation")]
    [HideInInspector] public Vector3 investigateTargetPosition;

    [Header("Guard Speed")]
    public float currentWalkSpeed;
    public float chaseSpeed = 4f;
    public float normalWalkSpeed = 2f;
    
    public EnemyBaseState EnemyCurrentState;
    public EnemyFollowPathState EnemyFollowPathState;
    public EnemyInvestigateALocationState EnemyInvestigateState;
    public EnemyLostPlayerState EnemyLostPlayerState;
    
    public EnemyChasePlayerState EnemyChasePlayerState = new EnemyChasePlayerState();

    public Transform playerTransform;

    
    public TMP_Text stateText;
    
    // Can make this an enum later
    [Header("Debug")]
    public string currentStateName;
    
    void Awake()
    {
        EnemyLostPlayerState = new EnemyLostPlayerState();
        EnemyFollowPathState = new EnemyFollowPathState();
        EnemyInvestigateState = new EnemyInvestigateALocationState();
    }

    void Start()
    {
        if (pathHolder == null)
        {
            Debug.LogError("Path Holder is not assigned on " + gameObject.name);
            return;
        }
        
        currentWalkSpeed = normalWalkSpeed;

        waypoints = new Vector3[pathHolder.childCount];
        for (int i = 0; i < pathHolder.childCount; i++)
        {
            waypoints[i] = pathHolder.GetChild(i).position;    
            waypoints[i] = new Vector3(waypoints[i].x, transform.position.y, waypoints[i].z);
        }

        SwitchState(EnemyFollowPathState);
    }
    

    public void StartChasing(Transform target)
    {
        playerTransform = target;

        // Only switch state if we aren't ALREADY chasing them
        if (EnemyCurrentState != EnemyChasePlayerState)
        {
            SwitchState(EnemyChasePlayerState);
        }
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

    public void TriggerInvestigation(Vector3 targetLocation)
    {
        // Dont trigger if we are already investigating this exact spot, prevents spamming
        if (EnemyCurrentState == EnemyInvestigateState && investigateTargetPosition == targetLocation) return;

        investigateTargetPosition = targetLocation;
        SwitchState(EnemyInvestigateState);
    }
}