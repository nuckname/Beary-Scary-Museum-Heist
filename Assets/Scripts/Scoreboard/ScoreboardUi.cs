using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class ScoreboardUi : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float lerpDuration = 0.75f;

    [Header("Text Elements")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI penaltiesText;
    public TextMeshProUGUI scoreText;

    [Header("Stars (Drag GameObjects here)")]
    public GameObject[] fullStars = new GameObject[3];
    public GameObject[] halfStars = new GameObject[3];

    public void PopulateScoreboard(string timeString, int penalties, int score, float starRating)
    {
        timeText.text = timeString;
        penaltiesText.text = $"(-750x{penalties})";
        
        // Start the incrementing animation instead of setting it instantly
        StartCoroutine(AnimateScore(score));

        // Turn off stars
        for (int i = 0; i < 3; i++)
        {
            fullStars[i].SetActive(false);
            halfStars[i].SetActive(false);
        }

        switch (starRating)
        {
            case 0.5f:
                halfStars[0].SetActive(true);
                break;
            case 1.0f:
                fullStars[0].SetActive(true);
                break;
            case 1.5f:
                fullStars[0].SetActive(true);
                halfStars[1].SetActive(true);
                break;
            case 2.0f:
                fullStars[0].SetActive(true);
                fullStars[1].SetActive(true);
                break;
            case 2.5f:
                fullStars[0].SetActive(true);
                fullStars[1].SetActive(true);
                halfStars[2].SetActive(true);
                break;
            case 3.0f:
                fullStars[0].SetActive(true);
                fullStars[1].SetActive(true);
                fullStars[2].SetActive(true);
                break;
        }
    }

    private IEnumerator AnimateScore(int targetScore)
    {
        float elapsed = 0f;
        int startScore = 0;

        while (elapsed < lerpDuration)
        {
            elapsed += Time.deltaTime;
            // Calculate progress (0 to 1)
            float pct = elapsed / lerpDuration;
            
            // Interpolate the value
            int currentDisplayScore = Mathf.RoundToInt(Mathf.Lerp(startScore, targetScore, pct));
            scoreText.text = currentDisplayScore.ToString();
            
            yield return null; // Wait for next frame
        }

        // Ensure it ends on the exact target number
        scoreText.text = targetScore.ToString();
    }
    
    // Called on button click
    public void RestartLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }

    // Called on button click
    public void LoadNextLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
    }
}