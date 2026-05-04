using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

[RequireComponent(typeof(AudioSource))]
public class DialogueHandler : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text speakerNameText;
    public TMP_Text dialogueBodyText;

    [Header("Typing Settings")]
    public float textTypeSpeed = 0.05f;
    
    [Header("Audio Settings")]
    public AudioClip voiceBlip;
    [Range(0.5f, 2f)] public float minPitch = 0.8f;
    [Range(0.5f, 2f)] public float maxPitch = 1.2f;

    [Header("Text Effects Settings")]
    public float waveSpeed = 5f;
    public float waveHeight = 10f;
    public float rainbowColorSpeed = 1f;
    public float shakeAmount = 2f;
    public float ghostFadeSpeed = 3f;
    [Range(0f, 1f)] public float glitchChance = 0.1f;
    public float glitchIntensity = 5f;

    [Header("Explosion Settings")]
    public float explosionForce = 50f;
    public float explosionGravity = 20f;
    
    [HideInInspector] public bool isDialogueComplete = false;

    private AudioSource audioSource;
    private List<DialogueLine> currentConversation;
    private int currentLineIndex = 0;
    
    private bool isTyping = false;
    private bool isExploding = false;
    private float explosionStartTime;
    private Coroutine typingCoroutine;

    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    /// <summary>
    /// Called by the trigger script to kick off the dialogue sequence.
    /// </summary>
    public void StartDialogue(DialogueSequence sequence)
    {
        currentConversation = sequence.conversation;
        currentLineIndex = 0;
        isDialogueComplete = false;

        if (currentConversation != null && currentConversation.Count > 0)
        {
            PlayCurrentLine();
        }
        else
        {
            CompleteSequence();
        }
    }

    private void Update()
    {
        AnimateSpecialText();

        // Left Click to advance or skip typing
        if (Input.GetMouseButtonDown(0) && !isExploding)
        {
            if (isTyping)
            {
                // Instant finish typing
                if (typingCoroutine != null) StopCoroutine(typingCoroutine);
                dialogueBodyText.maxVisibleCharacters = dialogueBodyText.textInfo.characterCount;
                isTyping = false;
            }
            else
            {
                // Go to next line
                currentLineIndex++;
                if (currentLineIndex < currentConversation.Count)
                {
                    PlayCurrentLine();
                }
                else
                {
                    // No more lines, trigger explosion out
                    StartCoroutine(ExplodeAndFinish());
                }
            }
        }
    }

    private void PlayCurrentLine()
    {
        DialogueLine currentLine = currentConversation[currentLineIndex];
        
        // Update the speaker name dynamically for this specific line
        if (speakerNameText != null) 
        {
            speakerNameText.text = currentLine.speakerName;
        }

        // Start typing the text
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(currentLine.text));
    }

    private IEnumerator TypeText(string rawText)
    {
        isTyping = true;
        
        string processedText = Regex.Replace(rawText, @"<rainbow>(.*?)</rainbow>", "<link=\"rainbow\">$1</link>");
        processedText = Regex.Replace(processedText, @"<wave>(.*?)</wave>", "<link=\"wave\">$1</link>");
        processedText = Regex.Replace(processedText, @"<shake>(.*?)</shake>", "<link=\"shake\">$1</link>");
        processedText = Regex.Replace(processedText, @"<ghost>(.*?)</ghost>", "<link=\"ghost\">$1</link>");
        processedText = Regex.Replace(processedText, @"<glitch>(.*?)</glitch>", "<link=\"glitch\">$1</link>");
        processedText = Regex.Replace(processedText, @"<explode>(.*?)</explode>", "<link=\"explode\">$1</link>");

        dialogueBodyText.text = processedText;
        dialogueBodyText.ForceMeshUpdate();

        dialogueBodyText.maxVisibleCharacters = 0;
        int totalVisibleCharacters = dialogueBodyText.textInfo.characterCount;

        for (int i = 0; i <= totalVisibleCharacters; i++)
        {
            dialogueBodyText.maxVisibleCharacters = i;

            if (i > 0 && i <= dialogueBodyText.textInfo.characterCount)
            {
                char c = dialogueBodyText.textInfo.characterInfo[i - 1].character;
                if (c != ' ' && voiceBlip != null)
                {
                    audioSource.pitch = Random.Range(minPitch, maxPitch);
                    audioSource.PlayOneShot(voiceBlip, 0.5f);
                }
            }

            yield return new WaitForSeconds(textTypeSpeed);
        }

        isTyping = false;
    }

    private IEnumerator ExplodeAndFinish()
    {
        isExploding = true;
        explosionStartTime = Time.time;
        
        // Wait for the text to physically scatter
        yield return new WaitForSeconds(1.5f); 
        
        CompleteSequence();
    }

    private void CompleteSequence()
    {
        isDialogueComplete = true;
        Destroy(gameObject); // Cleanup the spawned prefab
    }

    private void AnimateSpecialText()
    {
        if (dialogueBodyText == null || dialogueBodyText.textInfo == null || dialogueBodyText.textInfo.characterCount == 0) return;

        dialogueBodyText.ForceMeshUpdate();
        TMP_TextInfo textInfo = dialogueBodyText.textInfo;
        float timeSinceExplosion = Time.time - explosionStartTime;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;
            Color32[] vertexColors = textInfo.meshInfo[materialIndex].colors32;

            bool hasAnyTag = false;
            bool isRainbow = false, isWave = false, isShake = false, isGhost = false, isGlitch = false;

            foreach (TMP_LinkInfo link in textInfo.linkInfo)
            {
                if (i >= link.linkTextfirstCharacterIndex && i < link.linkTextfirstCharacterIndex + link.linkTextLength)
                {
                    hasAnyTag = true;
                    string linkId = link.GetLinkID();
                    if (linkId == "rainbow") isRainbow = true;
                    if (linkId == "wave") isWave = true;
                    if (linkId == "shake") isShake = true;
                    if (linkId == "ghost") isGhost = true;
                    if (linkId == "glitch") isGlitch = true;
                }
            }

            if (isExploding)
            {
                byte fadeAlpha = (byte)Mathf.Clamp(255 - (timeSinceExplosion * 150), 0, 255);
                for (int v = 0; v < 4; v++) vertexColors[vertexIndex + v].a = fadeAlpha;

                if (hasAnyTag)
                {
                    Random.InitState(i * 100); 
                    Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), Random.Range(0.5f, 1.5f), 0).normalized;
                    Vector3 explosionOffset = randomDir * explosionForce * timeSinceExplosion;
                    explosionOffset.y -= explosionGravity * (timeSinceExplosion * timeSinceExplosion); 

                    Matrix4x4 matrix = Matrix4x4.Rotate(Quaternion.Euler(0, 0, Random.Range(-300f, 300f) * timeSinceExplosion));
                    Vector3 center = (vertices[vertexIndex + 0] + vertices[vertexIndex + 2]) / 2f;

                    for (int v = 0; v < 4; v++)
                    {
                        vertices[vertexIndex + v] = center + matrix.MultiplyPoint3x4(vertices[vertexIndex + v] - center) + explosionOffset;
                    }
                }
                continue; 
            }

            if (isRainbow)
            {
                float hue = (Time.time * rainbowColorSpeed + i * 0.1f) % 1f;
                Color32 charColor = Color.HSVToRGB(hue, 1f, 1f);
                for (int v = 0; v < 4; v++) vertexColors[vertexIndex + v] = charColor;
            }

            if (isWave || isRainbow)
            {
                float waveOffset = Mathf.Sin(Time.time * waveSpeed + i * 1f) * waveHeight;
                for (int v = 0; v < 4; v++) vertices[vertexIndex + v].y += waveOffset;
            }

            if (isShake)
            {
                Vector3 jitterOffset = new Vector3(Random.Range(-shakeAmount, shakeAmount), Random.Range(-shakeAmount, shakeAmount), 0);
                for (int v = 0; v < 4; v++) vertices[vertexIndex + v] += jitterOffset;
            }

            if (isGhost)
            {
                float alphaFloat = (Mathf.Sin(Time.time * ghostFadeSpeed + i) + 1f) / 2f;
                byte alphaByte = (byte)(alphaFloat * 255);
                for (int v = 0; v < 4; v++) vertexColors[vertexIndex + v].a = alphaByte;
            }

            if (isGlitch)
            {
                if (Random.value < glitchChance)
                {
                    Vector3 glitchOffset = new Vector3(Random.Range(-glitchIntensity, glitchIntensity), Random.Range(-glitchIntensity, glitchIntensity), 0);
                    Color32 glitchColor = Random.value > 0.5f ? Color.cyan : Color.magenta;

                    for (int v = 0; v < 4; v++)
                    {
                        vertices[vertexIndex + v] += glitchOffset;
                        vertexColors[vertexIndex + v] = glitchColor;
                    }
                }
            }
        }
        dialogueBodyText.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices | TMP_VertexDataUpdateFlags.Colors32);
    }
}