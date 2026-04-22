using UnityEngine;

public class FlashingSpotlight : MonoBehaviour
{
    [Header("Timing")]
    public float minOnTime = 0.5f;
    public float maxOnTime = 2.0f;
    public float minOffTime = 0.2f;
    public float maxOffTime = 1.5f;

    [Header("Flicker Settings")]
    [Range(0, 1)] 
    public float flickerChance = 0.1f;
    public int flickerCount = 3; 
    public float flickerSpeed = 0.05f;

    private Light _light;
    private bool _isFlickering = false;

    void Start()
    {
        _light = GetComponent<Light>();
        StartCoroutine(LightRoutine());
    }

    private System.Collections.IEnumerator LightRoutine()
    {
        while (true)
        {
            _light.enabled = true;
            
            if (Random.value < flickerChance)
            {
                yield return StartCoroutine(FlickerSubRoutine());
            }
            else
            {
                yield return new WaitForSeconds(Random.Range(minOnTime, maxOnTime));
            }

            _light.enabled = false;
            yield return new WaitForSeconds(Random.Range(minOffTime, maxOffTime));
        }
    }

    private System.Collections.IEnumerator FlickerSubRoutine()
    {
        _isFlickering = true;
        for (int i = 0; i < flickerCount; i++)
        {
            _light.enabled = false;
            yield return new WaitForSeconds(flickerSpeed);
            _light.enabled = true;
            yield return new WaitForSeconds(flickerSpeed);
        }
        _isFlickering = false;
        
        yield return new WaitForSeconds(minOnTime);
    }
}