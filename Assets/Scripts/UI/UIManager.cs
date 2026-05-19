using System;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    // Singleton Instance
    public static UIManager Instance { get; private set; }

    [SerializeField] private GameObject InGameUi;
    
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
    }

    private void Start()
    {
        GameObject textObject = GameObject.FindGameObjectWithTag("artifactValueText");
        if (textObject != null)
        {
            artifactValueText = textObject.GetComponent<TextMeshProUGUI>();
        }
        
        InGameUi = GameObject.FindGameObjectWithTag("InGameUi");
    }

    public void UpdateArtifactDisplay(int current, int total)
    {
        if (artifactValueText != null)
        {
            artifactValueText.text = $"{current} / {total}";
        }
    }

    public void TurnOnInGameUi(bool turnOn)
    {
        if (InGameUi != null)
        {
            InGameUi.SetActive(turnOn);
        }
    }
}