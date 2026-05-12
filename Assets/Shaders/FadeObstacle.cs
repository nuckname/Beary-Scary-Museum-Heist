using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// https://www.youtube.com/shorts/D4xMei0nYW0
// https://www.patreon.com/posts/how-to-make-144078368
public class FadeObstacles : MonoBehaviour
{
    [Header("Targets")]
    [Tooltip("Assign the GameObjects you want to fade out here.")]
    public List<GameObject> objectsToFade = new List<GameObject>();

    [Header("Fade Settings")]
    public float fadedAlpha = 0.2f;
    public float fadeDuration = 0.25f;

    [Header("Trigger Settings")]
    public bool useTrigger = false;
    public string triggerTag = "MainCamera";
    public bool fadeOutOnEnter = true;

    // State Tracking to prevent conflicts between Raycasts and Triggers
    private bool _isFadedByRaycast = false;
    private bool _isFadedByTrigger = false;
    private bool _isCurrentlyFaded = false; 

    private BoxCollider _collider;
    
    private class FadeData
    {
        public Material material;
        public float originalAlpha;
        public float currentStartAlpha;
    }

    private List<FadeData> _fadeDataList = new List<FadeData>();
    private Coroutine _fadeRoutine;

    private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");

    private void Awake()
    {
        foreach (GameObject obj in objectsToFade)
        {
            if (obj != null)
            {
                Renderer renderer = obj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material mat = renderer.material; 
                    Color c = mat.GetColor(BaseColorID);
                    
                    _fadeDataList.Add(new FadeData
                    {
                        material = mat,
                        originalAlpha = c.a
                    });
                }
            }
        }
    }

    private void Start()
    {
        // sometimes i forgot to turn it on
        _collider.isTrigger = true;
    }

    public void SetRaycastFade(bool fade)
    {
        _isFadedByRaycast = fade;
        EvaluateFadeState();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!useTrigger) return;

        if (other.CompareTag(triggerTag))
        {
            _isFadedByTrigger = fadeOutOnEnter;
            EvaluateFadeState();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!useTrigger) return;

        if (other.CompareTag(triggerTag))
        {
            _isFadedByTrigger = !fadeOutOnEnter;
            EvaluateFadeState();
        }
    }

    // Checks BOTH conditions. Only fades in if NEITHER the raycast / trigger need it faded.
    private void EvaluateFadeState()
    {
        bool shouldBeFaded = _isFadedByRaycast || _isFadedByTrigger;

        if (shouldBeFaded != _isCurrentlyFaded)
        {
            _isCurrentlyFaded = shouldBeFaded;
            StartFade(_isCurrentlyFaded);
        }
    }

    private void StartFade(bool isFadingOut)
    {
        if (_fadeRoutine != null)
            StopCoroutine(_fadeRoutine);

        foreach (var data in _fadeDataList)
        {
            data.currentStartAlpha = data.material.GetColor(BaseColorID).a;
        }

        _fadeRoutine = StartCoroutine(FadeCoroutine(isFadingOut));
    }

    private IEnumerator FadeCoroutine(bool isFadingOut)
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / fadeDuration;
            if (t > 1f) t = 1f; 

            foreach (var data in _fadeDataList)
            {
                float targetAlpha = isFadingOut ? (data.originalAlpha * fadedAlpha) : data.originalAlpha;
                float a = Mathf.Lerp(data.currentStartAlpha, targetAlpha, t);
                
                Color c = data.material.GetColor(BaseColorID);
                c.a = a;
                data.material.SetColor(BaseColorID, c);
            }

            yield return null;
        }

        _fadeRoutine = null;
    }
}