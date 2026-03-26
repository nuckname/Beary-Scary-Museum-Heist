using TMPro;
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

    [Header("Guard Speed")]
    public float currentWalkSpeed;
    public float chaseSpeed = 4f;
    public float normalWalkSpeed = 2f;
    
    [Header("References")]
    public TMP_Text stateText;
    public FieldOfView fieldOfView;
    public Transform playerTransform;

    [SerializeField] private NoiseType whatTypeOfNoiseTheGuardHeard = NoiseType.Nothing;
    
    // State Instances
    [HideInInspector] public EnemyBaseState EnemyCurrentState; 
    [HideInInspector] public EnemyFollowPathState EnemyFollowPathState = new EnemyFollowPathState();
    [HideInInspector] public EnemyInvestigateALocationState EnemyInvestigateState = new EnemyInvestigateALocationState();
    [HideInInspector] public EnemyLostPlayerState EnemyLostPlayerState = new EnemyLostPlayerState();
    [HideInInspector] public EnemyStunnedState EnemyStunnedState = new EnemyStunnedState();
    [HideInInspector] public EnemyChasePlayerState EnemyChasePlayerState = new EnemyChasePlayerState();

    // Can make this an enum later
    [Header("Debug")]
    public string currentStateName;

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

    public void StartChasing(Transform target)
    {
        playerTransform = target;

        // Only switch state if we aren't ALREADY chasing them and NOT stunned
        if (EnemyCurrentState != EnemyChasePlayerState && EnemyCurrentState != EnemyStunnedState)
        {
            SwitchState(EnemyChasePlayerState);
        }
    }

    public void TriggerInvestigation(Vector3 targetLocation, NoiseType noiseType)
    {
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

    // This is the method the NoiseEmitter calls when it hears something
    public void OnSoundHeard(Vector3 originPosition, Transform sourceTransform, NoiseType noiseType)
    {
        TriggerInvestigation(originPosition, noiseType);
    }
    
    // Helper function to quickly update the enemies overhead text
    public void SetStateText(string message, Color textColor)
    {
        if (stateText != null)
        {
            stateText.text = message;
            stateText.color = textColor;
        }
    }

}