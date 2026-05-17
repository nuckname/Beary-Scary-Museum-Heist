using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ScoreThreshold
{
    [Tooltip("The maximum time (in seconds) allowed to earn this threshold.")]
    public float maxTimeAllowed;
    
    [Tooltip("The maximum number of times spotted allowed to earn this threshold.")]
    public int maxSpottedAllowed;
    
    [Tooltip("Stars awarded for meeting both of these conditions (e.g., 3, 2, 1, 0).")]
    public float starsAwarded;
}

public class ScoreCalculator : MonoBehaviour
{
    [Header("Level Configuration")]
    [Tooltip("Order these from Best (3 stars) to Worst (0 stars).")]
    public List<ScoreThreshold> scoreThresholds;

    public Timer timer;

    [Header("Numerical Scoring Rules")]
    [Tooltip("The maximum points a player can get if they finish in 0 seconds.")]
    public int maxScore = 10000;
    
    [Tooltip("The time in seconds when the time-score completely drops to 0.")]
    public float maxTimeLimit = 100f;
    
    [Tooltip("How many points are lost per spot.")]
    public int penaltyPerSpot = 750;

    /// <summary>
    /// Calculates the raw numerical score based on time and spots.
    /// </summary>
    public int CalculateRawScore()
    {
        float finalTime = timer._currentTime;
        int penalties = RoundStateManager.AmountOfTimesPlayerSpottedByGuards;

        // Calculate how much time has passed as a percentage (0.0 to 1.0)
        // Clamp01 ensures this value never goes above 1 (100%)
        float timeFraction = Mathf.Clamp01(finalTime / maxTimeLimit);

        // Invert the fraction (so 0 seconds = 1.0 multiplier, maxTimeLimit = 0.0 multiplier)
        float scoreMultiplier = 1f - timeFraction;

        int timeScore = Mathf.RoundToInt(maxScore * scoreMultiplier);
        
        // Deduct spot penalties from the time score
        int finalScore = timeScore - (penalties * penaltyPerSpot);

        return Mathf.Max(0, finalScore);
    }

    /// <summary>
    /// Calculates the awarded stars based on the time and spotted thresholds.
    /// </summary>
    public float CalculateFinalStars()
    {
        float finalTime = timer._currentTime;
        int timesSpotted = RoundStateManager.AmountOfTimesPlayerSpottedByGuards;

        // Iterate through thresholds (must be ordered from best to worst in the Inspector)
        foreach (ScoreThreshold threshold in scoreThresholds)
        {
            // The player must be UNDER or EQUAL to both the max time and max spots allowed
            if (finalTime <= threshold.maxTimeAllowed && timesSpotted <= threshold.maxSpottedAllowed)
            {
                // Return the first threshold they successfully beat
                return threshold.starsAwarded;
            }
        }

        // Default fallback if they don't meet any threshold (e.g., took too long or spotted too much)
        return 0f;
    }
}