using System;
using System.Collections;
using UnityEngine;
using TMPro;

// https://www.youtube.com/watch?v=8oTYabhj248&t=142s

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
            if (currentActiveText.text == currentLines[index].text)
            {
                NextLine();
            }
            else
            {
                StopAllCoroutines();
                currentActiveText.text = currentLines[index].text;
            }
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

        index = 0;
        UpdateSpeakerUI();
        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        foreach (char c in currentLines[index].text.ToCharArray())
        {
            currentActiveText.text += c;
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
}