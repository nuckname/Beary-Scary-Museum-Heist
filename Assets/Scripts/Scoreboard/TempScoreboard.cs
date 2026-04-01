using UnityEngine;
using TMPro;

// AI https://gemini.google.com/share/658d481e591d
public class TempScoreboard : MonoBehaviour
{
    [Header("References")]
    public Timer timer;
    public TextMeshProUGUI totalScoreTMPro; // Drag your TMPro element here in the inspector

    [HideInInspector]
    public bool isGameOver = false;

    void OnGUI()
    {
        // Don't draw the UI if the game isn't over or the timer is missing
        if (!isGameOver || timer == null) return;

        // --- 1. Calculate Scores ---
        // Using Mathf.FloorToInt to keep the scores as clean whole numbers
        int timeScore = Mathf.FloorToInt(timer._currentTime * 100f);
        int penaltyScore = RoundStateManager.AmountOfTimesPlayerSpottedByGuards * 10;
        int totalScore = timeScore - penaltyScore;

        // --- 2. Update the TMPro UI (Row 3 Requirement) ---
        if (totalScoreTMPro != null)
        {
            totalScoreTMPro.text = $"Total Score: {totalScore}";
        }

        // --- 3. Draw Centered OnGUI Debug Board ---
        float width = 450;
        float height = 150;
        float x = (Screen.width - width) / 2f;
        float y = (Screen.height - height) / 2f;

        // Draw the background box
        GUI.Box(new Rect(x, y, width, height), "DEBUG SCOREBOARD");

        // Set up text styles
        GUIStyle normalStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
        GUIStyle redStyle = new GUIStyle(normalStyle);
        redStyle.normal.textColor = Color.red;
        GUIStyle boldStyle = new GUIStyle(normalStyle) { fontStyle = FontStyle.Bold };

        // Row 1: Time Score
        GUI.Label(new Rect(x, y + 30, width, 30), $"Total Time Score ({timer._currentTime:F1}s * 100) = {timeScore}", normalStyle);
        
        // Row 2: Enemy Spotted Penalty (in Red)
        GUI.Label(new Rect(x, y + 60, width, 30), $"Enemy Spotted Score ({RoundStateManager.AmountOfTimesPlayerSpottedByGuards} * 10) = -{penaltyScore}", redStyle);
        
        // Row 3: Total Score
        GUI.Label(new Rect(x, y + 90, width, 30), $"Total Score = {totalScore}", boldStyle);
    }
}