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
    
    [Header("Settings")]
    public float textSpeed = 0.05f;
    public float delayBeforeStart = 0f; 

    private bool hasBeenTriggered = false;
    
    [Header("Clear Settings")]
    public float timeBeforeClear = 3f; 

    private Coroutine typingCoroutine;

    // A static variable shared across ALL instances of this script.
    private static int globalTextActionID = 0;


    [SerializeField] private bool triggerOnPlayer = true;
    [SerializeField] private bool triggerOnArtifact = false;
    

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && triggerOnPlayer)
        {
            if (!hasBeenTriggered)
            {
                if (delayBeforeStart > 0f)
                {
                    StartCoroutine(ExecuteAfterDelay(delayBeforeStart, text));
                }
                else
                {
                    TriggerTypingEffect(text);
                }
            }
        }
        
        if (other.CompareTag("Player") && triggerOnArtifact)
        {
            CanPickUpItem canPickUpItem = other.GetComponentInChildren<CanPickUpItem>();

            if (canPickUpItem != null) 
            {
                if (!hasBeenTriggered)
                {
                    TriggerTypingEffect(text);
                }
            }
        }
    }

    private IEnumerator ExecuteAfterDelay(float delay, string textToType)
    {
        hasBeenTriggered = true;
        yield return new WaitForSeconds(delay);
        TriggerTypingEffect(textToType);
    }

    public void TriggerTypingEffect(string _text)
    {
        // Increment the global ID because a new typing event is starting
        globalTextActionID++;
        
        // Save the current global ID as this specific action's ID
        int myActionID = globalTextActionID;

        // Stop the current typing effect if one is already running (on this specific instance)
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        // Start typing the new text, passing our unique ID into the coroutine
        typingCoroutine = StartCoroutine(TypeLine(_text, myActionID));

        hasBeenTriggered = true;
    }

    private IEnumerator TypeLine(string lineToType, int actionID)
    {
        currentActiveText.text = "";

        // Loop through each character in the passed string
        foreach (char c in lineToType.ToCharArray())
        {
            if (globalTextActionID != actionID)
            {
                yield break; 
            }

            currentActiveText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

        // Wait for the specified time, then clear the text
        if (timeBeforeClear > 0f)
        {
            yield return new WaitForSeconds(timeBeforeClear);
            
            // NEW: Before clearing, make sure another trigger hasn't taken over the UI!
            if (globalTextActionID == actionID)
            {
                currentActiveText.text = "";
            }
        }
    }
}