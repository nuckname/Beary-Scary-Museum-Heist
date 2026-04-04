using TMPro;
using UnityEngine;

public class RoundStateManager : MonoBehaviour
{
    // Singleton Instance
    public static RoundStateManager Instance { get; private set; }

    [HideInInspector] public RoundBaseState CurrentState; 
    [HideInInspector] public RoundAboutToStartState AboutToStartState = new RoundAboutToStartState();
    [HideInInspector] public RoundInProgressState InProgressState = new RoundInProgressState();
    [HideInInspector] public RoundGameOverState GameOverState = new RoundGameOverState();

    private TextMeshProUGUI artifactValueText;
    
    // dragged in from the inspector
    [HideInInspector] public Timer timer; 

    public static int AmountOfTimesPlayerSpottedByGuards = 0;
    
    public string currentStateName;
    // To complete the level
    public int amountOfArtifactsToCompleteLevel = 3;
    public int currentArtifacts = 0;

    private void Awake()
    {
        // Standard Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        artifactValueText = GameObject.FindGameObjectWithTag("artifactValueText").GetComponent<TextMeshProUGUI>();
    }

    void Start()
    {
        SwitchState(AboutToStartState);

        UpdateUI();
    }
    
    void Update()
    {
        CurrentState?.UpdateState(this);
    }
    
    public void UpdateUI()
    {
        UIManager.Instance.UpdateArtifactDisplay(currentArtifacts, amountOfArtifactsToCompleteLevel);
    }

    // call this to have collisions on another script. 
    public void NotifyCollisionEnter(Collision collision)
    {
        CurrentState?.OnCollisionEnter(this, collision);
    }

    public void NotifyCollisionExit(Collision collision)
    {
        CurrentState?.OnCollisionExit(this, collision);
    }

    public void NotifyTriggerEnter(Collider other)
    {
        CurrentState?.OnTriggerEnter(this, other);
    }

    public void NotifyTriggerExit(Collider other)
    {
        CurrentState?.OnTriggerExit(this, other);
    }

    public void SwitchState(RoundBaseState state)
    {
        CurrentState = state;
        
        if (CurrentState != null)
        {
            currentStateName = CurrentState.GetType().Name;
            CurrentState.EnterState(this);
        }
    }
}