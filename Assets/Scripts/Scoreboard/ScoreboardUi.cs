using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ScoreboardUi : MonoBehaviour
{
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
        scoreText.text = score.ToString();

        // Turn of stars
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
        else
        {
            Debug.LogWarning("No more levels found! Make sure your next scene is added to File -> Build Settings.");
        }
    }
}
