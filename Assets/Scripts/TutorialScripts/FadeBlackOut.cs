using UnityEngine;
using UnityEngine.UI; // Required for Image
using System.Collections; // Required for Coroutine

public class FadeBlackOut : MonoBehaviour
{
    [Header("Tutorial Settings")]
    [SerializeField] private bool isTutorial = false;
    
    [Header("Fade Settings")]
    [SerializeField] private Image fadeImage; // Assign your full-screen black image here
    [SerializeField] private float fadeDuration = 1.5f;

    private void Start()
    {
        // If it's the tutorial, start the fade out
        if (isTutorial && fadeImage != null)
        {
            // Ensure the image starts fully opaque (alpha = 1)
            Color startColor = fadeImage.color;
            startColor.a = 1f;
            fadeImage.color = startColor;
            fadeImage.gameObject.SetActive(true);

            StartCoroutine(FadeOutCoroutine());
        }
        else if (fadeImage != null)
        {
            // If it's not the tutorial, disable the black screen immediately
            fadeImage.gameObject.SetActive(false);
        }
    }

    private IEnumerator FadeOutCoroutine()
    {
        float elapsedTime = 0f;
        Color baseColor = fadeImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            
            // Lerp alpha from 1 to 0 over the duration
            float newAlpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            fadeImage.color = new Color(baseColor.r, baseColor.g, baseColor.b, newAlpha);

            yield return null; // Wait for the next frame
        }

        // Clamp to exactly 0f at the end to ensure it's completely invisible
        fadeImage.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0f);
        
        // Disable the game object so it doesn't block UI clicks or raycasts
        fadeImage.gameObject.SetActive(false);
    }
}