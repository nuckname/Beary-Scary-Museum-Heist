using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    // Singleton Instance
    public static UIManager Instance { get; private set; }

    private TextMeshProUGUI artifactValueText;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        artifactValueText = GameObject.FindGameObjectWithTag("artifactValueText").GetComponent<TextMeshProUGUI>();
    }
    
    public void UpdateArtifactDisplay(int current, int total)
    {
        artifactValueText.text = $"Artifacts: {current} / {total}";
    }
}