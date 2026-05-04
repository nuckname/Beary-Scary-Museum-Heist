using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Data")]
    [Tooltip("The ScriptableObject containing the conversation.")]
    [SerializeField] private DialogueSequence sequenceToPlay;
    
    [Tooltip("Drag the existing Dialogue Box UI from your scene hierarchy here.")]
    [SerializeField] private DialogueHandler existingDialogueUI;

    [Header("Interaction Settings")]
    [Tooltip("If true, you must Left Click to start the dialogue. If false, it plays automatically when you walk into the trigger.")]
    [SerializeField] private bool requireLeftClick = true;

    [Tooltip("If true, you must click directly ON this object's collider. If false, you can click anywhere on the screen while standing inside the trigger.")]
    [SerializeField] private bool mustClickDirectlyOnObject = true;

    private bool isPlayerInRange = false;

    private void Update()
    {
        if (requireLeftClick && !mustClickDirectlyOnObject && isPlayerInRange && Input.GetMouseButtonDown(0))
        {
            StartConversation();
        }
    }

    private void OnMouseDown()
    {
        if (requireLeftClick && mustClickDirectlyOnObject && isPlayerInRange)
        {
            StartConversation();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;

            if (!requireLeftClick)
            {
                StartConversation();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }

    public void StartConversation()
    {
        if (sequenceToPlay == null || existingDialogueUI == null)
        {
            Debug.LogWarning($"Missing Dialogue Sequence or UI Reference on {gameObject.name}");
            return;
        }

        // Turn on the UI if it was hidden, then start the sequence
        existingDialogueUI.gameObject.SetActive(true);
        existingDialogueUI.StartDialogue(sequenceToPlay);
    }
}