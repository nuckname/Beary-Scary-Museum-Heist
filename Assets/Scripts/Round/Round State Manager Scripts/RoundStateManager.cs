using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    
    [SerializeField] private bool isTutorialLevel = false;
    
    public string currentStateName;
    // To complete the level
    public int amountOfArtifactsToCompleteLevel = 3;
    public int currentArtifacts = 0;

    public List<GameObject> enemies = new List<GameObject>(); 
    public List<GameObject> cameras = new List<GameObject>();

    public GameObject scoreboardPrefab;
    public ScoreCalculator scoreCalculator;
    
    
    
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
    }

    void Start()
    {
        GameObject textObject = GameObject.FindGameObjectWithTag("artifactValueText");
        if (textObject != null)
        {
            artifactValueText = textObject.GetComponent<TextMeshProUGUI>();
        }
        
        if (isTutorialLevel)
        {
            SwitchState(InProgressState);            
        }
        else
        {
            SwitchState(AboutToStartState);
        }

        UIManager.Instance.TurnOnInGameUi(true);
        
        GetAllEnemies();

        UpdateUI();

        AmountOfTimesPlayerSpottedByGuards = 0;
    }

    private void GetAllEnemies()
    {
        enemies.AddRange(GameObject.FindGameObjectsWithTag("Enemy"));

    }    
    
    public void MakeGuardsStartMoving()
    {
        foreach (GameObject enemy in enemies)
        {
            // set up guards
            EnemyStateManager enemyStateManager = enemy.GetComponent<EnemyStateManager>();
            
            if(enemyStateManager != null)
            {
                enemyStateManager.InitialiseGuardStartMoving();
            }
            
            // Set up cameras
            SecurityCameraController securityCameraController = enemy.GetComponent<SecurityCameraController>();

            if (securityCameraController != null)
            {
                securityCameraController.StartCamaeraMovement();   
            }
        }
    }


    public void MakeCamerassStartMoving()
    {
        foreach (GameObject guard in enemies)
        {
            EnemyStateManager enemyStateManager = guard.GetComponent<EnemyStateManager>();
            if(enemyStateManager != null)
            {
                enemyStateManager.InitialiseGuardStartMoving();
            }
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void SetUpScoreboard()
    {
        UIManager.Instance.TurnOnInGameUi(false);
        
        GameObject scoreboard = Instantiate(scoreboardPrefab, Vector3.zero, Quaternion.identity);
        ScoreboardUi scoreboardUi = scoreboard.GetComponent<ScoreboardUi>();
        
        float targetTime = timer._currentTime;
        
        int penalties = AmountOfTimesPlayerSpottedByGuards;
        
        // Get data from our updated ScoreCalculator
        int score = scoreCalculator.CalculateRawScore();
        float starsEarned = scoreCalculator.CalculateFinalStars();

        scoreboardUi.PopulateScoreboard(targetTime, penalties, score, starsEarned);
   
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