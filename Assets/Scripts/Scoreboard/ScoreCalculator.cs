using System.Collections.Generic;
using UnityEngine;



[System.Serializable]
public struct ScoreThreshold
{
    [Tooltip("The minimum score required to earn this many base stars.")]
    public int minScoreRequired;
    [Tooltip("Base stars awarded for reaching this score (e.g., 3, 2, 1, 0).")]
    public float baseStars;
}

public class ScoreCalculator : MonoBehaviour
{
    [Header("Level Configuration")]
    [Tooltip("Order these from Highest Score (Best) to Lowest Score (Worst).")]
    public List<ScoreThreshold> scoreThresholds;

    [Header("References")]
    [Tooltip("Drag the GameObject with your Timer script here")]
    public Timer timer;

    [Header("Scoring Rules")]
    [Tooltip("The maximum points a player can get if they finish in 0 seconds.")]
    public int maxScore = 10000;
    
    [Tooltip("The time in seconds when the time-score completely drops to 0.")]
    public float maxTimeLimit = 100f;
    
    [Tooltip("How many points are lost per spot.")]
    public int penaltyPerSpot = 750;

    public int CalculateRawScore()
    {
        float finalTime = timer._currentTime;
        int penalties = RoundStateManager.AmountOfTimesPlayerSpottedByGuards;

        // Calculate how much time has passed as a percentage (0.0 to 1.0)
        // Clamp01 ensures this value never goes above 1 (100%), even if they take 5 minutes
        float timeFraction = Mathf.Clamp01(finalTime / maxTimeLimit);

        // Invert the fraction (so 0 seconds = 1.0 multiplier, 100 seconds = 0.0 multiplier)
        float scoreMultiplier = 1f - timeFraction;

        int timeScore = Mathf.RoundToInt(maxScore * scoreMultiplier);

        int finalScore = timeScore - (penalties * penaltyPerSpot);

        return Mathf.Max(0, finalScore);
    }

    public float CalculateFinalStars(int rawScore)
    {
        int timesSpotted = RoundStateManager.AmountOfTimesPlayerSpottedByGuards;
        float baseStars = 0f;

        // Iterate through thresholds to find the first one the player beat
        foreach (ScoreThreshold threshold in scoreThresholds)
        {
            if (rawScore >= threshold.minScoreRequired)
            {
                baseStars = threshold.baseStars;
                break;
            }
        }

        float penalty = 0f;
        if (timesSpotted > 0)
        {
            // First time spotted = 0.5 penalty
            penalty += 0.5f;

            // After that, 0.5 penalty every 2 times spotted
            if (timesSpotted > 1)
            {
                int additionalSpots = timesSpotted - 1;
                penalty += Mathf.FloorToInt(additionalSpots / 2f) * 0.5f;
            }
        }

        // Apply penalty and clamp between 0 and 3 stars
        float finalStars = baseStars - penalty;
        finalStars = Mathf.Clamp(finalStars, 0f, 3f);

        return finalStars;
    }
}