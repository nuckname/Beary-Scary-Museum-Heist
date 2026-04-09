using System;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; // Required for loading the next level

// AI https://gemini.google.com/share/658d481e591d
// AI next level button https://gemini.google.com/share/7c70e371649a
public class TempScoreboard : MonoBehaviour
{
    [Header("References")]
    private Timer timer;
    private TextMeshProUGUI totalScoreTMPro;

    [Header("Balancing Settings")]
    [Tooltip("The score a player gets if they finish in 0 seconds.")]
    public int maxPossibleTimeScore = 10000; 
    [Tooltip("How many points to lose per second.")]
    public int pointsLostPerSecond = 100;
    public int penaltyPerSpot = 500;

    [HideInInspector]
    public bool isGameOver = false;

    private void Awake()
    {
        timer = GetComponent<Timer>();
        totalScoreTMPro = GameObject.FindGameObjectWithTag("ScoreBoard").GetComponent<TextMeshProUGUI>();
    }

    void OnGUI()
    {
        // Don't draw the UI if the game isn't over or the timer is missing
        if (!isGameOver || timer == null) return;

        // Start high, subtract time. Mathf.Max ensures score doesn't go below 0.
        int rawTimeScore = Mathf.FloorToInt(timer._currentTime * pointsLostPerSecond);
        int timeScore = Mathf.Max(0, maxPossibleTimeScore - rawTimeScore);
        
        int penaltyScore = RoundStateManager.AmountOfTimesPlayerSpottedByGuards * penaltyPerSpot;
        int totalScore = Mathf.Max(0, timeScore - penaltyScore);

        if (totalScoreTMPro != null)
        {
            totalScoreTMPro.text = $"Total Score: {totalScore}";
        }

        // --- 3. Draw Centered OnGUI Debug Board ---
        // INCREASED GUI SIZE
        float width = 700;
        float height = 350;
        float x = (Screen.width - width) / 2f;
        float y = (Screen.height - height) / 2f;

        // Apply a custom style to the box itself to make the title larger
        GUIStyle boxStyle = new GUIStyle(GUI.skin.box) { fontSize = 20, fontStyle = FontStyle.Bold };
        GUI.Box(new Rect(x, y, width, height), "DEBUG SCOREBOARD", boxStyle);

        // Increased font sizes for readability in the larger box
        GUIStyle normalStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 18 };
        GUIStyle redStyle = new GUIStyle(normalStyle) { normal = { textColor = Color.red } };
        GUIStyle boldStyle = new GUIStyle(normalStyle) { fontStyle = FontStyle.Bold, fontSize = 26 };

        // Row 1: How the time score was calculated
        GUI.Label(new Rect(x, y + 60, width, 40), 
            $"Base ({maxPossibleTimeScore}) - Time Penalty ({timer._currentTime:F1}s * {pointsLostPerSecond}) = {timeScore}", normalStyle);
        
        // Row 2: Stealth Penalty
        GUI.Label(new Rect(x, y + 110, width, 40), 
            $"Stealth Penalty ({RoundStateManager.AmountOfTimesPlayerSpottedByGuards} spots * {penaltyPerSpot}) = -{penaltyScore}", redStyle);
        
        // Row 3: Final Result
        GUI.Label(new Rect(x, y + 160, width, 50), $"FINAL SCORE: {totalScore}", boldStyle);

        // --- 4. Next Level Button ---
        float buttonWidth = 250;
        float buttonHeight = 60;
        float buttonX = x + (width - buttonWidth) / 2f; // Center horizontally
        float buttonY = y + 250; // Position near the bottom

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 22, fontStyle = FontStyle.Bold };

        if (GUI.Button(new Rect(buttonX, buttonY, buttonWidth, buttonHeight), "Next Level", buttonStyle))
        {
            LoadNextLevel();
        }
    }

    private void LoadNextLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        // Make sure there is actually a next scene in the Build Settings to prevent errors
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogWarning("No more levels found! Make sure your next scene is added to File -> Build Settings.");
            // Optional: Loop back to main menu or first level
            // SceneManager.LoadScene(0); 
        }
    }
}