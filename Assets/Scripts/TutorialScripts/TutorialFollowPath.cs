using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.Text.RegularExpressions;

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
    public float textTypeSpeed = 0.05f; 
    public int currentWaypointIndex = 0; 

    [Header("Wave & Rainbow Settings")]
    public float waveSpeed = 5f;
    public float waveHeight = 10f;
    public float rainbowColorSpeed = 1f;

    private void Start()
    {
        StartCoroutine(FollowPath());
    }

    private void Update()
    {
        AnimateSpecialText();
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

            // Trigger the dialogue
            if (!string.IsNullOrEmpty(point.dialogueText))
            {
                yield return StartCoroutine(TypeText(point.dialogueText));
            }

            // Wait
            if (point.waitTime > 0)
            {
                yield return new WaitForSeconds(point.waitTime);
            }

            floatingText.maxVisibleCharacters = 0; 
            floatingText.text = "";
        }
        
        Debug.Log("Path complete!");
    }

    private IEnumerator TypeText(string rawText)
    {
        // Secretly convert <rainbow> tags into TMP <link> tags so we can find them in the Update loop
        string processedText = Regex.Replace(rawText, @"<rainbow>(.*?)</rainbow>", "<link=\"rainbow\">$1</link>");

        floatingText.text = processedText;
        floatingText.ForceMeshUpdate();

        floatingText.maxVisibleCharacters = 0;
        int totalVisibleCharacters = floatingText.textInfo.characterCount;

        for (int i = 0; i <= totalVisibleCharacters; i++)
        {
            floatingText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(textTypeSpeed);
        }
    }

    // This handles the mesh manipulation every frame
    private void AnimateSpecialText()
    {
        if (floatingText == null || floatingText.textInfo == null || floatingText.textInfo.characterCount == 0) return;

        // Force mesh update to reset vertices to their default positions before we move them
        floatingText.ForceMeshUpdate();
        TMP_TextInfo textInfo = floatingText.textInfo;

        // Look through the text for our hidden "rainbow" links
        foreach (TMP_LinkInfo link in textInfo.linkInfo)
        {
            if (link.GetLinkID() == "rainbow")
            {
                // Loop through every character inside that specific word
                for (int i = link.linkTextfirstCharacterIndex; i < link.linkTextfirstCharacterIndex + link.linkTextLength; i++)
                {
                    TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

                    // Skip spaces or letters that haven't been typed yet
                    if (!charInfo.isVisible) continue; 

                    int materialIndex = charInfo.materialReferenceIndex;
                    int vertexIndex = charInfo.vertexIndex;

                    // --- 1. FLOWING RAINBOW COLOR ---
                    // Use Time.time to make the colors shift over time
                    float hue = (Time.time * rainbowColorSpeed + i * 0.1f) % 1f; 
                    Color32 charColor = Color.HSVToRGB(hue, 1f, 1f);

                    Color32[] vertexColors = textInfo.meshInfo[materialIndex].colors32;
                    vertexColors[vertexIndex + 0] = charColor;
                    vertexColors[vertexIndex + 1] = charColor;
                    vertexColors[vertexIndex + 2] = charColor;
                    vertexColors[vertexIndex + 3] = charColor;

                    // --- 2. BOUNCING SINE WAVE ---
                    // Calculate a wave offset based on time and the letter's position
                    float waveOffset = Mathf.Sin(Time.time * waveSpeed + i * 1f) * waveHeight; 

                    Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;
                    vertices[vertexIndex + 0].y += waveOffset;
                    vertices[vertexIndex + 1].y += waveOffset;
                    vertices[vertexIndex + 2].y += waveOffset;
                    vertices[vertexIndex + 3].y += waveOffset;
                }
            }
        }

        // Push our modified vertex data back into the visible mesh
        floatingText.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices | TMP_VertexDataUpdateFlags.Colors32);
    }
}