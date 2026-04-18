using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueBox : MonoBehaviour
{
    [SerializeField] [TextArea] private List<string> dialogueLines;
    [SerializeField] private int lineIndex = 0;
    
    private TMP_Text dialogueText;
    private CanvasGroup canvasGroup;
    void Start()
    {
        dialogueText = GetComponent<TextMeshPro>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
