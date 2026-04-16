using UnityEngine;
using System.Collections;

public class AlarmComponent : MonoBehaviour
{
    [Header("Alarm Settings")]
    public float alarmBeepInterval = 1f;
    public float alarmNoiseRadius = 10f;
    
    public NoiseEmitter noiseEmitter; 

    private Coroutine alarmCoroutine;

    public void StartAlarm()
    {
        if (alarmCoroutine != null) return; 
        
        alarmCoroutine = StartCoroutine(AlarmRoutine());
    }

    public void StopAlarm()
    {
        if (alarmCoroutine != null)
        {
            StopCoroutine(alarmCoroutine);
            alarmCoroutine = null;
        }
    }

    private IEnumerator AlarmRoutine()
    {
        // loop forever until StopCoroutine is called
        while (true) 
        {
            if (noiseEmitter != null)
            {
                noiseEmitter.EmitNoise(alarmNoiseRadius, NoiseType.Player);
            }
            
            yield return new WaitForSeconds(alarmBeepInterval);
        }
    }
}