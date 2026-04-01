using UnityEngine;
using TMPro;

// AI https://gemini.google.com/share/658d481e591d
public class TempScoreboard : MonoBehaviour
{
    [Header("References")]
    public Timer timer;
    public TextMeshProUGUI totalScoreTMPro; // Drag your TMPro element here in the inspector

    [Header("Balancing Settings")]
    [Tooltip("The score a player gets if they finish in 0 seconds.")]
    public int maxPossibleTimeScore = 10000; 
    [Tooltip("How many points to lose per second.")]
    public int pointsLostPerSecond = 100;
    public int penaltyPerSpot = 50;

    [HideInInspector]
    public bool isGameOver = false;

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
        float width = 500;
        float height = 180;
        float x = (Screen.width - width) / 2f;
        float y = (Screen.height - height) / 2f;

        GUI.Box(new Rect(x, y, width, height), "DEBUG SCOREBOARD");

        GUIStyle normalStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
        GUIStyle redStyle = new GUIStyle(normalStyle) { normal = { textColor = Color.red } };
        GUIStyle boldStyle = new GUIStyle(normalStyle) { fontStyle = FontStyle.Bold, fontSize = 16 };

        // Row 1: How the time score was calculated
        GUI.Label(new Rect(x, y + 30, width, 30), 
            $"Base ({maxPossibleTimeScore}) - Time Penalty ({timer._currentTime:F1}s * {pointsLostPerSecond}) = {timeScore}", normalStyle);
        
        // Row 2: Stealth Penalty
        GUI.Label(new Rect(x, y + 70, width, 30), 
            $"Stealth Penalty ({RoundStateManager.AmountOfTimesPlayerSpottedByGuards} spots * {penaltyPerSpot}) = -{penaltyScore}", redStyle);
        
        // Row 3: Final Result
        GUI.Label(new Rect(x, y + 110, width, 40), $"FINAL SCORE: {totalScore}", boldStyle);
    }
}