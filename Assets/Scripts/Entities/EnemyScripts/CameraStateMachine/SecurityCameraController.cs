using UnityEngine;

public class SecurityCameraController : MonoBehaviour
{
    [Header("References")]
    public Transform cameraHead;
    public FieldOfView fieldOfView;
    [HideInInspector] public NoiseEmitter noiseEmitter;
    [HideInInspector] public Transform playerTransform;

    [Header("Rotation Settings")]
    public float leftAngle = -45f;
    public float rightAngle = 45f;
    public float rotationSpeed = 20f;
    public float pauseTimeAtEdges = 1.5f;

    [Header("Alarm Settings")]
    public float alarmNoiseRadius = 15f;
    public float alarmBeepInterval = 3f;
    
    public bool startTurningRight = true;
    
    // State Machine Instances
    [HideInInspector] public CameraBaseState currentState;
    public readonly CameraSweepState SweepState = new CameraSweepState();
    public readonly CameraAlarmState AlarmState = new CameraAlarmState();

    public AlarmComponent alarmComponent;
    
    [Header("Debug")]
    public string currentStateName;

    private void Awake()
    {
        noiseEmitter = GetComponent<NoiseEmitter>();
        if (cameraHead == null) cameraHead = this.transform;
    }

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

    public void StartCamaeraMovement()
    {
        SwitchState(SweepState);
    }

    private void Update()
    {
        currentState?.UpdateState(this);
    }

    public void SwitchState(CameraBaseState newState)
    {
        currentState?.ExitState(this); 
        
        currentState = newState;
        currentStateName = currentState.GetType().Name; // For the inspector
        currentState.EnterState(this);
    }

    private void HandlePlayerSpotted(Transform target)
    {
        playerTransform = target;
        
        // Only switch if we aren't already sounding the alarm
        if (currentState != AlarmState)
        {
            SwitchState(AlarmState);
        }
    }

    private void HandlePlayerLost(Vector3 lastKnownPosition)
    {
        if (currentState == AlarmState)
        {
            SwitchState(SweepState);
        }
    }
}