using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class ScoreboardUi : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float timeToPenaltiesDelay = 1.0f; 
    [SerializeField] private float penaltiesToScoreDelay = 0.5f;
    [SerializeField] private float lerpDuration = 0.75f;
    [SerializeField] private float starFadeDuration = 0.4f;
    [SerializeField] private float delayBetweenStars = 0.15f;

    [Header("Text Elements")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI penaltiesText;
    public TextMeshProUGUI scoreText;

    [Header("Stars (Drag GameObjects here)")]
    public GameObject[] fullStars = new GameObject[3];
    public GameObject[] halfStars = new GameObject[3];

    public void PopulateScoreboard(string timeString, int penalties, int score, float starRating)
    {
        timeText.gameObject.SetActive(false);
        penaltiesText.gameObject.SetActive(false);
        scoreText.gameObject.SetActive(false);
        
        for (int i = 0; i < 3; i++)
        {
            fullStars[i].SetActive(false);
            halfStars[i].SetActive(false);
        }

        StartCoroutine(AnimateScoreSequence(timeString, penalties, score, starRating));
    }

    private IEnumerator AnimateScoreSequence(string timeString, int penalties, int targetScore, float starRating)
    {
        // Pop in time
        timeText.text = timeString;
        timeText.gameObject.SetActive(true);

        yield return new WaitForSeconds(timeToPenaltiesDelay);

        // Pop in Penalties
        penaltiesText.text = $"(-750x{penalties})";
        penaltiesText.gameObject.SetActive(true);

        yield return new WaitForSeconds(penaltiesToScoreDelay);

        // Show and Animate Score
        scoreText.text = "0";
        scoreText.gameObject.SetActive(true);

        float elapsed = 0f;
        int startScore = 0;

        while (elapsed < lerpDuration)
        {
            elapsed += Time.deltaTime;
            float pct = elapsed / lerpDuration;
            
            int currentDisplayScore = Mathf.RoundToInt(Mathf.Lerp(startScore, targetScore, pct));
            scoreText.text = currentDisplayScore.ToString();
            
            yield return null; 
        }

        // Ensure it ends on the exact target number
        scoreText.text = targetScore.ToString();

        // Fade in Stars
        yield return StartCoroutine(FadeInStars(starRating));
    }

    private IEnumerator FadeInStars(float starRating)
    {
        // Activate the correct stars based on the rating
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

        // Collect CanvasGroups in order from left to right
        List<CanvasGroup> activeStars = new List<CanvasGroup>();
        
        for (int i = 0; i < 3; i++)
        {
            if (fullStars[i].activeSelf) 
            {
                activeStars.Add(GetOrAddCanvasGroup(fullStars[i]));
            }
            else if (halfStars[i].activeSelf) 
            {
                activeStars.Add(GetOrAddCanvasGroup(halfStars[i]));
            }
        }

        // Fade them one by one
        foreach (CanvasGroup cg in activeStars)
        {
            float elapsed = 0f;
            while (elapsed < starFadeDuration)
            {
                elapsed += Time.deltaTime;
                cg.alpha = elapsed / starFadeDuration;
                yield return null;
            }
            
            cg.alpha = 1f;

            if (delayBetweenStars > 0f)
            {
                yield return new WaitForSeconds(delayBetweenStars);
            }
        }
    }

    private CanvasGroup GetOrAddCanvasGroup(GameObject obj)
    {
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = obj.AddComponent<CanvasGroup>();
        }
        cg.alpha = 0f; 
        return cg;
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