using System.Collections;
using UnityEngine;
using TMPro;

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
    public GameObject bearDialogueBox; // The parent object/panel for the Bear's text
    public TextMeshProUGUI bearText;   // The text component inside the Bear's box

    [Header("Dog UI")]
    public GameObject dogDialogueBox;  // The parent object/panel for the Dog's text
    public TextMeshProUGUI dogText;    // The text component inside the Dog's box

    [Header("Bear Emotion Images")]
    public GameObject bearHappy;
    public GameObject bearHelp;
    public GameObject bearScared;

    [Header("Dog Emotion Images")]
    public GameObject dogExplain;
    public GameObject dogSad;
    public GameObject dogOpeningHappy;
    
    [Space(10)]
    public DialogueLine[] lines;
    public float textSpeed;

    private int index;
    private TextMeshProUGUI currentActiveText; // Tracks which text box is currently typing
    
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
            if (currentActiveText.text == lines[index].text)
            {
                NextLine();
            }
            else
            {
                StopAllCoroutines();
                currentActiveText.text = lines[index].text;
            }
        }
    }

    void StartDialogue()
    {
        index = 0;
        UpdateSpeakerUI();
        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        foreach (char c in lines[index].text.ToCharArray())
        {
            currentActiveText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }

    void NextLine()
    {
        if (index < lines.Length - 1)
        {
            index++;
            UpdateSpeakerUI();
            StartCoroutine(TypeLine());
        }
        else
        {
            // End of dialogue, hide everything
            if (bearDialogueBox != null) bearDialogueBox.SetActive(false);
            if (dogDialogueBox != null) dogDialogueBox.SetActive(false);
            HideAllImages();
            gameObject.SetActive(false);
        }
    }

    private void UpdateSpeakerUI()
    {
        HideAllImages();

        // Turn off both dialogue boxes initially
        if (bearDialogueBox != null) bearDialogueBox.SetActive(false);
        if (dogDialogueBox != null) dogDialogueBox.SetActive(false);

        SpeakerType currentSpeaker = lines[index].speaker;
        Emotion currentEmotion = lines[index].emotion;

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