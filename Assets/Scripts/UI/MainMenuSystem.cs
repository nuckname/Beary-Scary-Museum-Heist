using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; 
using System.Collections; 

// https://gemini.google.com/share/b383687aeffd
// AI helped with black screen 
public class MainMenuSystem : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject backgroundImage;
    public GameObject mainMenuImage;
    
    [Header("Transition Settings")]
    public Image fadeImage;       // Assign a full-screen black image here
    public float fadeDuration = 1f; // How long the fade takes in seconds

    [Header("Level Images/Panels (Index 0 is Tutorial)")]
    public GameObject[] levels;

    private GameObject currentActiveLevel;

    private void Start()
    {
        DisableAllLevels();
        mainMenuImage.SetActive(true);
        backgroundImage.SetActive(false);
        
        // Ensure the fade image starts fully transparent and inactive
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
            fadeImage.gameObject.SetActive(false);
        }
    }

    public void LoadLevelSelect()
    {
        mainMenuImage.SetActive(false);
        SelectLevel(0); 
        backgroundImage.SetActive(true);
    }

    public void SelectLevel(int levelIndex)
    {
        if (currentActiveLevel != null)
        {
            currentActiveLevel.SetActive(false);
        }
        
        // Check if the index is within the array size
        if (levelIndex >= 0 && levelIndex < levels.Length)
        {
            // Prevent the crash if the inspector slot is empty
            if (levels[levelIndex] != null) 
            {
                levels[levelIndex].SetActive(true);
                currentActiveLevel = levels[levelIndex];
            }
            else
            {
                Debug.LogError("Level slot in inspector is empty.");
            }
        }
        else
        {
            Debug.LogWarning("Level index out of bounds.");
        }
    }

    private void DisableAllLevels()
    {
        foreach (GameObject level in levels)
        {
            if (level != null)
            {
                level.SetActive(false);
            }
        }
        currentActiveLevel = null;
    }

    // Updated LoadLevel triggers the coroutine
    public void LoadLevel(int index)
    {
        if (fadeImage != null)
        {
            StartCoroutine(FadeInAndLoad(index));
        }
        else
        {
            Debug.LogWarning("Fade Image not assigned. Loading scene instantly.");
            SceneManager.LoadScene(index);
        }
    }

    // Coroutine handles the alpha interpolation over time
    private IEnumerator FadeInAndLoad(int sceneIndex)
    {
        fadeImage.gameObject.SetActive(true);
        float elapsedTime = 0f;
        Color startColor = fadeImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            
            // Calculate the new alpha using Lerp for a smooth transition
            float newAlpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            fadeImage.color = new Color(startColor.r, startColor.g, startColor.b, newAlpha);

            yield return null; // Wait for the next frame
        }

        // Clamp to exactly 1f at the end to ensure it's fully opaque
        fadeImage.color = new Color(startColor.r, startColor.g, startColor.b, 1f);

        // Load the scene once the fade is complete
        SceneManager.LoadScene(sceneIndex);
    }
}