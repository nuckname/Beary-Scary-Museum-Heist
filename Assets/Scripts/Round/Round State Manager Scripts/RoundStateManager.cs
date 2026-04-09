using System.Collections.Generic;
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
    
   // [HideInInspector] public Timer timer; 

    public static int AmountOfTimesPlayerSpottedByGuards = 0;
    
    public string currentStateName;
    // To complete the level
    public int amountOfArtifactsToCompleteLevel = 3;
    public int currentArtifacts = 0;

    // temp guards to hold in memory.
    private List<GameObject> guards = new List<GameObject>(); 

    [Header("Round Progression")]
    public List<RoundConfigurationSO> roundConfigurations;
    public int currentRoundIndex = 0;
    
    [Space(20)]
    public Transform playerTransform;
    public List<Transform> pathHolders;
    
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

        // need a round counter -> increase with next round state
        LoadRound(0);

        UpdateUI();

        AmountOfTimesPlayerSpottedByGuards = 0;
    }
    
    public void LoadRound(int roundIndex)
    {
        // Clean up existing guards if we are loading a new round mid-game
        foreach (GameObject guard in guards)
        {
            if (guard != null) Destroy(guard);
        }
        guards.Clear();

        // Load SO Data
        if (roundIndex >= roundConfigurations.Count)
        {
            Debug.LogWarning("No more rounds available! You beat the game.");
            return;
        }

        RoundConfigurationSO currentRound = roundConfigurations[roundIndex];
        amountOfArtifactsToCompleteLevel = currentRound.amountOfArtifactsToCompleteLevel;
        currentArtifacts = 0;

        // Spawn and Configure Guards
        foreach (GuardSpawnData data in currentRound.guardsToSpawn)
        {
            Transform assignedPath = pathHolders[data.pathHolderIndex];
            
            Transform spawnPoint = assignedPath.GetChild(0);
            
            // Spawn the guard
            GameObject newGuard = Instantiate(data.guardPrefab, spawnPoint.position, spawnPoint.rotation);
            EnemyStateManager enemyStateManager = newGuard.GetComponent<EnemyStateManager>();

            // Inject scene references and custom SO settings
            enemyStateManager.SetupGuardFromRoundManager(
                pathHolders[data.pathHolderIndex], 
                playerTransform, 
                data
            );

            guards.Add(newGuard);
        }

        UpdateUI();
    }

    private void SetUpGuards()
    {
        guards.AddRange(GameObject.FindGameObjectsWithTag("Enemy"));

    }
    
    public void MakeGuardsStartMoving()
    {
        foreach (GameObject guard in guards)
        {
            EnemyStateManager enemyStateManager = guard.GetComponent<EnemyStateManager>();
            if(enemyStateManager != null)
            {
                enemyStateManager.InitialiseGuardStartMoving();
            }
        }
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