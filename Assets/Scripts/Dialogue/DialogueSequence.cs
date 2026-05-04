using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct DialogueLine
{
    public string speakerName;
    
    [TextArea(3, 5)]
    public string text;
}

[CreateAssetMenu(fileName = "New Conversation", menuName = "Dialogue System/Conversation Sequence")]
public class DialogueSequence : ScriptableObject
{
    [Header("The Conversation")]
    [Tooltip("Add lines in the exact order they should be spoken.")]
    public List<DialogueLine> conversation;
}