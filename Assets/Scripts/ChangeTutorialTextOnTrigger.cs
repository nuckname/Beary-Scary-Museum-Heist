using System;
using System.Collections;
using UnityEngine;
using TMPro; 

public class ChangeTutorialTextOnTrigger : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI currentActiveText;
    
    [TextArea(2, 5)]
    public string text;
    
    // Called on start
    public bool useStartingText = false;
    public string startingText;
    
    [Header("Settings")]
    public float textSpeed = 0.05f;

    [Header("Clear Settings")]
    public float timeBeforeClear = 3f; 

    private Coroutine typingCoroutine;

    private void Start()
    {
        if(useStartingText)
        {
            TriggerTypingEffect(startingText);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TriggerTypingEffect(text);
        }
    }

    public void TriggerTypingEffect(string _text)
    {
        // Stop the current typing effect if one is already running
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        // Start typing the new text
        typingCoroutine = StartCoroutine(TypeLine(_text));
    }

    private IEnumerator TypeLine(string lineToType)
    {
        currentActiveText.text = "";

        // Loop through each character in the passed string
        foreach (char c in lineToType.ToCharArray())
        {
            currentActiveText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

        // Wait for the specified time, then clear the text
        if (timeBeforeClear > 0f)
        {
            yield return new WaitForSeconds(timeBeforeClear);
            currentActiveText.text = "";
        }
    }
}