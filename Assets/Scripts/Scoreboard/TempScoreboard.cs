using System;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; 

// AI https://gemini.google.com/share/121a014f27ff
// AI https://gemini.google.com/share/658d481e591d
// AI next level button https://gemini.google.com/share/7c70e371649a
public class TempScoreboard : MonoBehaviour
{
    [Header("References")]
    private Timer timer;
    private TextMeshProUGUI totalScoreTMPro;

    [Header("Star Time Thresholds (In Seconds)")]
    [Tooltip("Time required to get 3 base stars.")]
    public float threeStarTime = 40f; 
    [Tooltip("Time required to get 2 base stars.")]
    public float twoStarTime = 60f;   
    [Tooltip("Time required to get 1 base star. Any time above this gets 0 base stars.")]
    public float oneStarTime = 80f;   

    [HideInInspector]
    public bool isGameOver = false;

    private void Awake()
    {
        timer = GetComponent<Timer>();
        GameObject scoreBoardObj = GameObject.FindGameObjectWithTag("ScoreBoard");
        if (scoreBoardObj != null)
        {
            totalScoreTMPro = scoreBoardObj.GetComponent<TextMeshProUGUI>();
        }
    }

    void OnGUI()
    {
        // Don't draw the UI if the game isn't over or the timer is missing
        if (!isGameOver || timer == null) return;

        // --- 1. Calculate Base Stars (Time) ---
        float timeTaken = timer._currentTime;
        float baseStars = 0f;

        if (timeTaken <= threeStarTime) baseStars = 3f;
        else if (timeTaken <= twoStarTime) baseStars = 2f;
        else if (timeTaken <= oneStarTime) baseStars = 1f;
        else baseStars = 0f;

        // --- 2. Calculate Stealth Penalty ---
        int spots = RoundStateManager.AmountOfTimesPlayerSpottedByGuards;
        float spotPenalty = 0f;

        if (spots > 0)
        {
            // First spot is -0.5. After that, -0.5 for every 2 spots.
            // spots = 1 -> 0.5 penalty
            // spots = 2 -> 0.5 penalty
            // spots = 3 -> 1.0 penalty
            // spots = 4 -> 1.0 penalty
            spotPenalty = 0.5f + Mathf.Floor((spots - 1) / 2f) * 0.5f;
        }

        // --- 3. Calculate Final Stars ---
        // Clamp between 0 and 3 so it doesn't go negative or somehow above 3
        float finalStars = Mathf.Clamp(baseStars - spotPenalty, 0f, 3f);

        if (totalScoreTMPro != null)
        {
            // Optional: Update canvas UI if you decide to use it later
            // totalScoreTMPro.text = $"Stars: {finalStars:F1} / 3.0";
        }

        // --- 4. Draw Centered OnGUI Debug Board ---
        float width = 700;
        float height = 350;
        float x = (Screen.width - width) / 2f;
        float y = (Screen.height - height) / 2f;

        GUIStyle boxStyle = new GUIStyle(GUI.skin.box) { fontSize = 20, fontStyle = FontStyle.Bold };
        GUI.Box(new Rect(x, y, width, height), "LEVEL COMPLETE", boxStyle);

        GUIStyle normalStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 18 };
        GUIStyle redStyle = new GUIStyle(normalStyle) { normal = { textColor = Color.red } };
        GUIStyle boldStyle = new GUIStyle(normalStyle) { fontStyle = FontStyle.Bold, fontSize = 26 };

        // Row 1: Time and Base Stars
        GUI.Label(new Rect(x, y + 60, width, 40), 
            $"Time: {timeTaken:F1}s  =>  Base Stars: {baseStars}", normalStyle);
        
        // Row 2: Stealth Penalty
        GUI.Label(new Rect(x, y + 110, width, 40), 
            $"Spotted: {spots} times  =>  Penalty: -{spotPenalty:F1} Stars", spotPenalty > 0 ? redStyle : normalStyle);
        
        // Row 3: Final Result
        GUI.Label(new Rect(x, y + 160, width, 50), $"FINAL RATING: {finalStars:F1} / 3.0 STARS", boldStyle);

        // --- 5. Navigation Buttons ---
        float buttonWidth = 250;
        float buttonHeight = 60;
        float buttonY = y + 250; 
        
        bool canProceed = finalStars >= 1.0f;

        // If they can proceed, show both buttons side by side
        if (canProceed)
        {
            float spacing = 50f; 
            float restartButtonX = x + (width / 2f) - buttonWidth - (spacing / 2f);
            float nextButtonX = x + (width / 2f) + (spacing / 2f);

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 22, fontStyle = FontStyle.Bold };

            if (GUI.Button(new Rect(restartButtonX, buttonY, buttonWidth, buttonHeight), "Restart Level", buttonStyle))
            {
                RestartLevel();
            }

            if (GUI.Button(new Rect(nextButtonX, buttonY, buttonWidth, buttonHeight), "Next Level", buttonStyle))
            {
                LoadNextLevel();
            }
        }
        else // If they failed to get at least 1 star, only show the restart button centered, with a warning
        {
            GUIStyle warningStyle = new GUIStyle(normalStyle) { normal = { textColor = Color.yellow }, fontSize = 20, fontStyle = FontStyle.Bold };
            GUI.Label(new Rect(x, y + 210, width, 30), "You need at least 1 star to advance!", warningStyle);

            float restartButtonX = x + (width / 2f) - (buttonWidth / 2f);
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 22, fontStyle = FontStyle.Bold };

            if (GUI.Button(new Rect(restartButtonX, buttonY, buttonWidth, buttonHeight), "Try Again", buttonStyle))
            {
                RestartLevel();
            }
        }
    }

    private void RestartLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }

    private void LoadNextLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogWarning("No more levels found! Make sure your next scene is added to File -> Build Settings.");
        }
    }
}