using UnityEngine;

public class TutorialReturnedArtifactToDogCollision : MonoBehaviour
{
    public GameObject dialoguePrefab;
    private bool dialogueTriggered = false; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !dialogueTriggered)
        {
            Transform[] allChildren = other.GetComponentsInChildren<Transform>();

            foreach (Transform child in allChildren)
            {
                if (child.CompareTag("CanPickUp"))
                {
                    dialogueTriggered = true;

                    GameObject _dialoguePrefab = Instantiate(dialoguePrefab);
                    _dialoguePrefab.GetComponent<DialogueBox>().hasReturnedArtifact = true;
 
                    break;
                }
            }
        }

        if (other.CompareTag("CanPickUp") && !dialogueTriggered)
        {
            dialogueTriggered = true;

            GameObject _dialoguePrefab = Instantiate(dialoguePrefab);
            _dialoguePrefab.GetComponent<DialogueBox>().hasReturnedArtifact = true;
        }
    }
}