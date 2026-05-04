using System.Collections;
using UnityEngine;

public class PlayDogAwake : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Drag the Dog GameObject here. This script acts as the trigger, but this object will play the animation.")]
    [SerializeField] private Transform playDeadAnimationOnThisGameObject;

    [SerializeField] private GameObject dialoguePrefab;
    
    // Optional: If your dialogue prefab doesn't have a Screen Space Canvas built in, drag your scene's main canvas here
    [SerializeField] private Transform mainCanvas;
    
    // Stubs for your custom parameters
    [System.Serializable]
    public struct CustomAnimationParams
    {
        public SpecialAction action;
        public float reviveDuration;
        public float reviveHeight;
        public float reviveSpinSpeed;
    }
    public enum SpecialAction { None, PlayDeadAndRevive }

    [Header("Animation Settings")]
    [SerializeField] private CustomAnimationParams customParam;
    
    private bool isAnimating = false;

    private void OnTriggerEnter(Collider other)
    {
        // Only trigger if it's the player AND we aren't already playing the animation
        if (other.CompareTag("Player") && !isAnimating)
        {
            if (playDeadAnimationOnThisGameObject != null)
            {
                // Start the coroutine and pass the player's transform so the dog can look at them
                StartCoroutine(PlayDogAnimationRoutine(other.transform));
            }
            else
            {
                Debug.LogWarning("PlayDogAwake: No Dog Transform assigned to animate!");
            }
        }
    }

    private IEnumerator PlayDogAnimationRoutine(Transform playerTransform)
    {
        if (customParam.action != SpecialAction.PlayDeadAndRevive) yield break;

        isAnimating = true;

        // Use a local variable to make the code cleaner
        Transform dogTransform = playDeadAnimationOnThisGameObject;

        // 1. Instantly flip upside down to "Play Dead"
        Vector3 currentRot = dogTransform.eulerAngles;
        dogTransform.eulerAngles = new Vector3(currentRot.x, currentRot.y, 180f);

        // 2. Calculate where the player is to find our final landing angle
        float startAngle = dogTransform.eulerAngles.y;
        float targetAngle = startAngle;

        if (playerTransform != null)
        {
            Vector3 dirToPlayer = playerTransform.position - dogTransform.position;
            dirToPlayer.y = 0; // Ignore height differences
            if (dirToPlayer != Vector3.zero)
            {
                targetAngle = Quaternion.LookRotation(dirToPlayer).eulerAngles.y;
            }
        }

        // Default safety values in case inspector variables were left at 0
        float rDuration = customParam.reviveDuration > 0f ? customParam.reviveDuration : 2f;
        float rHeight = customParam.reviveHeight > 0f ? customParam.reviveHeight : 2f;
        float rSpinSpeed = customParam.reviveSpinSpeed > 0f ? customParam.reviveSpinSpeed : 1000f;

        // Calculate the exact degrees needed to spin and land perfectly on the target angle
        float rawTotalDegrees = 0.5f * rDuration * rSpinSpeed;
        float angleDifference = Mathf.DeltaAngle(startAngle + rawTotalDegrees, targetAngle);
        float totalDegrees = rawTotalDegrees + angleDifference;

        Vector3 groundPos = dogTransform.position;
        float elapsedTime = 0f;

        // 3. The Revive Animation Loop
        while (elapsedTime < rDuration)
        {
            elapsedTime += Time.deltaTime;
            float timeNormalized = Mathf.Clamp01(elapsedTime / rDuration);

            // -- Height Math: Float up and down using a Sine Wave --
            float heightOffset = Mathf.Sin(timeNormalized * Mathf.PI) * rHeight;
            dogTransform.position = groundPos + (Vector3.up * heightOffset);

            // -- Spin Math: Smoothly decelerate using Quadratic Ease-Out --
            float easeOut = (2f * timeNormalized) - (timeNormalized * timeNormalized);
            float currentYAngle = startAngle + (totalDegrees * easeOut);
            
            dogTransform.rotation = Quaternion.Euler(0f, currentYAngle, 0f);

            yield return null;
        }
        
        Instantiate(dialoguePrefab);
        
        dogTransform.position = groundPos;
        isAnimating = false;
    }
}