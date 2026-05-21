using UnityEngine;

public class MainMenuSystem : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject backgroundImage;
    public GameObject mainMenuImage;

    [Header("Level Images/Panels (Index 0 is Tutorial)")]
    public GameObject[] levels;

    private GameObject currentActiveLevel;

    private void Start()
    {
        DisableAllLevels();
        mainMenuImage.SetActive(true);
        backgroundImage.SetActive(false);
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
                
                // Update our tracker to the newly enabled level
                currentActiveLevel = levels[levelIndex];
            }
            else
            {
                Debug.LogError($"error.");
            }
        }
        else
        {
            Debug.LogWarning($"error.");
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
}