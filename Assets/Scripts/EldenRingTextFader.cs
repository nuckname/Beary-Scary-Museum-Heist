using UnityEngine;
using TMPro;
using System.Collections;

public class EldenRingTextFader : MonoBehaviour
{
    private TextMeshProUGUI textMesh;
    private float fadeInDuration = 1f;
    private float stayDuration = 1.0f;
    private float fadeOutDuration = 1f;

    public void StartSequence()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        StartCoroutine(SequenceCoroutine());
    }

    private IEnumerator SequenceCoroutine()
    {
        float timer = 0;
        Color color = textMesh.color;

        // 1. Slow Fade In & Scale Up
        while (timer < fadeInDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeInDuration;
            
            color.a = Mathf.Lerp(0, 1, progress);
            textMesh.color = color;

            float scale = Mathf.Lerp(0.9f, 1.0f, progress); // Slight dramatic zoom out/in
            transform.localScale = new Vector3(scale, scale, scale);
            
            yield return null;
        }

        // 2. Hold on screen
        yield return new WaitForSeconds(stayDuration);

        // 3. Slow Fade Out
        timer = 0;
        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeOutDuration;
            
            color.a = Mathf.Lerp(1, 0, progress);
            textMesh.color = color;
            
            yield return null;
        }

        // Clean up the text object completely once done
        Destroy(gameObject);
    }
}