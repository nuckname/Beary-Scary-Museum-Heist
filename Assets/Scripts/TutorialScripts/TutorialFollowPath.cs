using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// gameobject follows a path with diagloue 
/// </summary>

[System.Serializable]
public struct Waypoint
{
    public Transform targetTransform;
    public float waitTime;
    
    [TextArea(2, 5)]
    public string dialogueText; 
}

public class TutorialFollowPath : MonoBehaviour
{
    [Header("Path Settings")]
    public List<Waypoint> waypoints = new List<Waypoint>();
    public float moveSpeed = 5f;

    [Header("Dialogue Settings")]
    public TMP_Text floatingText;
    
    [Tooltip("The time in seconds between each letter appearing.")]
    public float textTypeSpeed = 0.05f; 
    
    [Tooltip("Tracks which waypoint the object is currently traveling to or acting upon.")]
    public int currentWaypointIndex = 0; 

    private void Start()
    {
        StartCoroutine(FollowPath());
    }

    private IEnumerator FollowPath()
    {
        for (currentWaypointIndex = 0; currentWaypointIndex < waypoints.Count; currentWaypointIndex++)
        {
            Waypoint point = waypoints[currentWaypointIndex];

            // Move towards the transform
            while (Vector3.Distance(transform.position, point.targetTransform.position) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, point.targetTransform.position, moveSpeed * Time.deltaTime);
                yield return null; 
            }

            transform.position = point.targetTransform.position;

            // Trigger the dialogue for this specific waypoint
            if (!string.IsNullOrEmpty(point.dialogueText))
            {
                yield return StartCoroutine(TypeText(point.dialogueText));
            }

            // Wait for the specified amount of time at this transform
            if (point.waitTime > 0)
            {
                yield return new WaitForSeconds(point.waitTime);
            }

            floatingText.text = "";
        }
        
        Debug.Log("Path complete!");
    }

    // Slowly write out the text
    // Text effect
    private IEnumerator TypeText(string textToType)
    {
        floatingText.text = "";
        
        // Loop through each character in the string
        foreach (char c in textToType.ToCharArray())
        {
            // Add the current character to the text display
            floatingText.text += c;
            
            // Wait for the specified delay before adding the next character
            yield return new WaitForSeconds(textTypeSpeed);
        }
    }
}