using System.Collections;
using UnityEngine;
using TMPro;

// https://www.youtube.com/watch?v=8oTYabhj248&t=142s
// https://gemini.google.com/share/a39faa31283b
public enum SpeakerType
{
    Bear,
    Dog
}

public enum Emotion
{
    Bear_Happy,
    Bear_Help,
    Bear_Scared,
    
    Dog_Explain,
    Dog_Sad,
    Dog_OpeningHappy
}

[System.Serializable]
public struct DialogueLine
{
    public SpeakerType speaker;
    public Emotion emotion;
    
    [TextArea(3, 10)]
    public string text;
}

public class DialogueBox : MonoBehaviour
{
    [Header("Bear UI")]
    public GameObject bearDialogueBox; 
    public TextMeshProUGUI bearText;

    [Header("Dog UI")]
    public GameObject dogDialogueBox; 
    public TextMeshProUGUI dogText;
    
    [Header("Bear Emotion Images")]
    public GameObject bearHappy;
    public GameObject bearHelp;
    public GameObject bearScared;

    [Header("Dog Emotion Images")]
    public GameObject dogExplain;
    public GameObject dogSad;
    public GameObject dogOpeningHappy;
    
    [Space(10)]
    public bool hasReturnedArtifact = false;
    public DialogueLine[] IntroLineslines;
    public DialogueLine[] haveReturnedTheArtifactlines;
    public float textSpeed;

    [Header("Camera Pan")]
    private CameraFollow cameraScript; 
    private Transform panTargetObject;
    public float panSpeed = 3f;  
    public float waitTimeBeforeReturn = 2f; // How long to look at the target before returning

    [Header("Text Animation Settings")]
    public float waveSpeed = 8f;
    public float waveHeight = 5f;
    public float rainbowSpeed = 2f;

    private int index;
    private TextMeshProUGUI currentActiveText;
    
    // This private array will hold whichever list of lines we are currently reading
    private DialogueLine[] currentLines; 

    private void Awake()
    {
        cameraScript = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraFollow>();
        panTargetObject = GameObject.FindGameObjectWithTag("CameraPanTarget").transform;
    }

    void Start()
    {
        if (bearText != null) bearText.text = "";
        if (dogText != null) dogText.text = "";
        
        StartDialogue();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && currentActiveText != null)
        {
            // Check if the current visible characters match the total characters
            if (currentActiveText.maxVisibleCharacters >= currentActiveText.textInfo.characterCount)
            {
                NextLine();
            }
            else
            {
                StopAllCoroutines();
                // Reveal all characters instantly
                currentActiveText.maxVisibleCharacters = currentActiveText.textInfo.characterCount;
            }
        }

        // Animate the text every frame if there is active text
        if (currentActiveText != null && currentActiveText.maxVisibleCharacters > 0)
        {
            AnimateText();
        }
    }

    void StartDialogue()
    {
        // Decide which set of lines to use based
        if (hasReturnedArtifact)
        {
            currentLines = haveReturnedTheArtifactlines;
        }
        else
        {
            currentLines = IntroLineslines;
        }

        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStealthController>().cannotMove = true;
        
        index = 0;
        UpdateSpeakerUI();
        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        // Set the full text immediately, but hide it
        currentActiveText.text = currentLines[index].text;
        currentActiveText.maxVisibleCharacters = 0;
        
        // Force the mesh to update so we can accurately get the character count
        currentActiveText.ForceMeshUpdate();
        int totalCharacters = currentActiveText.textInfo.characterCount;

        // Reveal one character at a time using maxVisibleCharacters to preserve rich text tags
        while (currentActiveText.maxVisibleCharacters < totalCharacters)
        {
            currentActiveText.maxVisibleCharacters++;
            yield return new WaitForSeconds(textSpeed);
        }
    }

    void NextLine()
    {
        if (index < currentLines.Length - 1)
        {
            
            index++;
            UpdateSpeakerUI();
            StartCoroutine(TypeLine());
        }
        else
        {
            // End of dialogue, hide all UI visuals
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStealthController>().cannotMove = false;
            
            bearDialogueBox.SetActive(false);
            dogDialogueBox.SetActive(false);
            
            if (bearText != null) bearText.text = "";
            if (dogText != null) dogText.text = "";
            HideAllImages();
            
            // Clear text reference so mouse clicks during the wait don't trigger anything
            currentActiveText = null; 

            // Trigger the camera pan sequence if the variables are assigned
            if (cameraScript != null && panTargetObject != null && !hasReturnedArtifact)
            {
                
                StartCoroutine(CameraPanSequence());
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }

    private IEnumerator CameraPanSequence()
    {
        // 1. Pan to the empty game object
        cameraScript.StartPanning(panTargetObject, panSpeed);

        // 2. Wait for the specified time
        yield return new WaitForSeconds(waitTimeBeforeReturn);

        // 3. Stop panning (CameraFollow will automatically lerp back to the player)
        cameraScript.StopPanning();

        // 4. Now that the sequence is done, disable this dialogue object
        gameObject.SetActive(false);
        
        // unfreeze player
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStealthController>().cannotMove = false;
    }

    private void UpdateSpeakerUI()
    {
        HideAllImages();

        if (bearText != null) bearText.text = "";
        if (dogText != null) dogText.text = "";
        
        // Turn off both dialogue boxes initially
        if (bearDialogueBox != null) bearDialogueBox.SetActive(false);
        if (dogDialogueBox != null) dogDialogueBox.SetActive(false);

        SpeakerType currentSpeaker = currentLines[index].speaker;
        Emotion currentEmotion = currentLines[index].emotion;

        // Assign the correct UI elements based on the speaker
        if (currentSpeaker == SpeakerType.Bear)
        {
            if (bearDialogueBox != null) bearDialogueBox.SetActive(true);
            currentActiveText = bearText;

            switch (currentEmotion)
            {
                case Emotion.Bear_Happy:
                    if (bearHappy != null) bearHappy.SetActive(true);
                    break;
                case Emotion.Bear_Help:
                    if (bearHelp != null) bearHelp.SetActive(true);
                    break;
                case Emotion.Bear_Scared:
                    if (bearScared != null) bearScared.SetActive(true);
                    break;
                default:
                    Debug.LogWarning("Emotion not assigned or not applicable for Bear.");
                    break;
            }
        }
        else if (currentSpeaker == SpeakerType.Dog)
        {
            if (dogDialogueBox != null) dogDialogueBox.SetActive(true);
            currentActiveText = dogText;

            switch (currentEmotion)
            {
                case Emotion.Dog_Explain:
                    if (dogExplain != null) dogExplain.SetActive(true);
                    break;
                case Emotion.Dog_Sad:
                    if (dogSad != null) dogSad.SetActive(true);
                    break;
                case Emotion.Dog_OpeningHappy:
                    if (dogOpeningHappy != null) dogOpeningHappy.SetActive(true);
                    break;
                default:
                    Debug.LogWarning("Emotion not assigned or not applicable for Dog.");
                    break;
            }
        }

        // Clear the text box of the active speaker before we start typing
        if (currentActiveText != null)
        {
            currentActiveText.text = string.Empty;
        }
    }

    // Helper method to keep the logic clean and ensure no overlapping images
    private void HideAllImages()
    {
        if (bearHappy != null) bearHappy.SetActive(false);
        if (bearHelp != null) bearHelp.SetActive(false);
        if (bearScared != null) bearScared.SetActive(false);
        
        if (dogExplain != null) dogExplain.SetActive(false);
        if (dogSad != null) dogSad.SetActive(false);
        if (dogOpeningHappy != null) dogOpeningHappy.SetActive(false);
    }
    
    // Parses TMP Links to apply custom vertex animations (Wave, Rainbow)
    private void AnimateText()
    {
        // Regenerate the mesh cleanly so animations don't infinitely compound every frame
        currentActiveText.ForceMeshUpdate();
        TMP_TextInfo textInfo = currentActiveText.textInfo;

        // Loop through all custom links in the text
        for (int i = 0; i < textInfo.linkCount; i++)
        {
            TMP_LinkInfo linkInfo = textInfo.linkInfo[i];
            string linkID = linkInfo.GetLinkID().ToLower();
            
            bool isWave = linkID.Contains("wave");
            bool isRainbow = linkID.Contains("rainbow");

            if (!isWave && !isRainbow) continue;

            // Loop through all characters contained within the link tags
            for (int j = 0; j < linkInfo.linkTextLength; j++)
            {
                int charIndex = linkInfo.linkTextfirstCharacterIndex + j;
                
                // Skip invisible characters like spaces
                if (!textInfo.characterInfo[charIndex].isVisible) continue;

                int materialIndex = textInfo.characterInfo[charIndex].materialReferenceIndex;
                int vertexIndex = textInfo.characterInfo[charIndex].vertexIndex;
                
                Vector3[] sourceVertices = textInfo.meshInfo[materialIndex].vertices;
                Color32[] vertexColors = textInfo.meshInfo[materialIndex].colors32;

                // Apply Wave effect
                if (isWave)
                {
                    Vector3 offset = new Vector3(0, Mathf.Sin(Time.time * waveSpeed + charIndex) * waveHeight, 0);
                    for (int v = 0; v < 4; v++)
                    {
                        sourceVertices[vertexIndex + v] += offset;
                    }
                }

                // Apply Rainbow color effect
                if (isRainbow)
                {
                    // Generate color based on time and character index to get a moving rainbow
                    Color32 rainbowColor = Color.HSVToRGB(Mathf.Repeat(Time.time * rainbowSpeed + charIndex * 0.1f, 1f), 1f, 1f);
                    for (int v = 0; v < 4; v++)
                    {
                        vertexColors[vertexIndex + v] = rainbowColor;
                    }
                }
            }
        }

        // Push all vertex and color changes back to the text mesh
        currentActiveText.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices | TMP_VertexDataUpdateFlags.Colors32);
    }
}