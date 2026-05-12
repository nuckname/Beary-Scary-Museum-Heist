using System;
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
    }

    private void Start()
    {
        GameObject textObject = GameObject.FindGameObjectWithTag("artifactValueText");
        if (textObject != null)
        {
            artifactValueText = textObject.GetComponent<TextMeshProUGUI>();
        }
    }

    public void UpdateArtifactDisplay(int current, int total)
    {
        if (artifactValueText != null)
        {
            artifactValueText.text = $"{current} / {total}";
        }
    }
}