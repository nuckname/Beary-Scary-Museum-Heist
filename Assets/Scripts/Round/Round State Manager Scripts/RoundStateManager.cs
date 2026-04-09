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

    public static int AmountOfTimesPlayerSpottedByGuards = 0;
    public string currentStateName;
    
    // temp guards to hold in memory.
    [HideInInspector] public int amountOfArtifactsToCompleteLevel = 3;
    [HideInInspector] public int currentArtifacts = 0;

    private List<GameObject> guards = new List<GameObject>(); 
    private List<GameObject> spawnedArtifacts = new List<GameObject>(); 

    [Header("Round Progression")]
    public List<RoundConfigurationSO> roundConfigurations;
    public int currentRoundIndex = 0;
    
    [Header("Scene References")]
    public Transform playerTransform;
    public List<Transform> pathHolders;
    public List<Transform> artifactSpawnPoints;
    
    [Header("Dynamic Scene Objects")]
    [Tooltip("Master list of objects (doors, walls, cover) that can be removed depending on the round.")]
    public List<GameObject> removableObjects;
    
    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }

        artifactValueText = GameObject.FindGameObjectWithTag("artifactValueText").GetComponent<TextMeshProUGUI>();
    }

    void Start()
    {
        SwitchState(AboutToStartState);
        LoadRound(currentRoundIndex);
        AmountOfTimesPlayerSpottedByGuards = 0;
    }
    
    public void LoadRound(int roundIndex)
    {
        // Clean Up from previous round before loading new one:
        foreach (GameObject guard in guards) { if (guard != null) Destroy(guard); }
        guards.Clear();

        foreach (GameObject artifact in spawnedArtifacts) { if (artifact != null) Destroy(artifact); }
        spawnedArtifacts.Clear();

        // Turn everything back on first, so we start with a clean slate
        foreach (GameObject obj in removableObjects)
        {
            if (obj != null) obj.SetActive(true);
        }

        if (roundIndex > roundConfigurations.Count)
        {
            Debug.LogWarning("No more rounds available! You beat the game.");
            return;
        }

        // Load SO Data
        RoundConfigurationSO currentRound = roundConfigurations[roundIndex];
        amountOfArtifactsToCompleteLevel = currentRound.amountOfArtifactsToCompleteLevel;
        currentArtifacts = 0;

        // disable specific scene objects
        foreach (int index in currentRound.objectsToRemoveIndices)
        {
            // Make sure the index actually exists in our list to prevent errors
            if (index >= 0 && index < removableObjects.Count)
            {
                if (removableObjects[index] != null)
                {
                    removableObjects[index].SetActive(false);
                }
            }
            else
            {
                Debug.LogWarning($"Trying to remove object at index {index}, but it doesn't exist in the removableObjects list!");
            }
        }

        // Spawn Guards
        foreach (GuardSpawnData data in currentRound.guardsToSpawn)
        {
            Transform assignedPath = pathHolders[data.pathHolderIndex];
            Transform spawnPoint = assignedPath.GetChild(0);
            
            GameObject newGuard = Instantiate(data.guardPrefab, spawnPoint.position, spawnPoint.rotation);
            EnemyStateManager enemyStateManager = newGuard.GetComponent<EnemyStateManager>();

            enemyStateManager.SetupGuardFromRoundManager(assignedPath, playerTransform, data);
            guards.Add(newGuard);
        }

        // Spawn Artifacts
        foreach (ArtifactSpawnData artData in currentRound.artifactsToSpawn)
        {
            Transform spawnPoint = artifactSpawnPoints[artData.spawnPointIndex];
            
            GameObject newArtifact = Instantiate(artData.artifactPrefab, spawnPoint.position, spawnPoint.rotation);
            spawnedArtifacts.Add(newArtifact);
        }

        UpdateUI();
    }
    
    public void ResetArtifacts()
    {
        currentArtifacts = 0;
        UpdateUI();
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

    void Update() { CurrentState?.UpdateState(this); }

    public void UpdateUI()
    {
        UIManager.Instance.UpdateArtifactDisplay(currentArtifacts, amountOfArtifactsToCompleteLevel);
    }

    public void ResetUI()
    {
        UIManager.Instance.UpdateArtifactDisplay(0, amountOfArtifactsToCompleteLevel);
    }

    public void NotifyCollisionEnter(Collision collision) { CurrentState?.OnCollisionEnter(this, collision); }
    public void NotifyCollisionExit(Collision collision) { CurrentState?.OnCollisionExit(this, collision); }
    public void NotifyTriggerEnter(Collider other) { CurrentState?.OnTriggerEnter(this, other); }
    public void NotifyTriggerExit(Collider other) { CurrentState?.OnTriggerExit(this, other); }

    public void SwitchState(RoundBaseState state)
    {
        CurrentState = state;
        if (CurrentState != null)
        {
            currentStateName = CurrentState.GetType().Name;
            CurrentState.EnterState(this);
        }
    }
    
    
    
    // AI
    // https://gemini.google.com/share/1a7a03f377a9
    public void SpawnEldenRingPopup(string customMessage)
    {
        // 1. Find an existing Canvas, or create a dynamic one on the fly
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("DynamicPopupCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100; // Force it over everything else
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }

        // 2. Create the Text GameObject
        GameObject popupObj = new GameObject("EldenRingRoundPopup");
        popupObj.transform.SetParent(canvas.transform, false);

        // 3. Add and configure TextMeshProUGUI
        TextMeshProUGUI tmpText = popupObj.AddComponent<TextMeshProUGUI>();
        tmpText.text = customMessage; // Inject the custom string here
        tmpText.fontSize = 140;
        tmpText.alignment = TextAlignmentOptions.Center;
        tmpText.fontStyle = FontStyles.Bold;
        
        // Classic faint golden Elden Ring color (Starting alpha at 0 for the fade)
        tmpText.color = new Color(0.85f, 0.75f, 0.45f, 0f);

        // 4. Center it perfectly on the screen via RectTransform
        RectTransform rect = tmpText.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(1200, 300);
        rect.anchoredPosition = Vector2.zero;

        // 5. Attach the animator and fire it
        EldenRingTextFader fader = popupObj.AddComponent<EldenRingTextFader>();
        fader.StartSequence();
    }
}