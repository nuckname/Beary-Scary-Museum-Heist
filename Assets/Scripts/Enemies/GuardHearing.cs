using UnityEngine;
using System.Collections;

public class GuardHearing : MonoBehaviour, ISoundListener 
{
    [Header("Hearing Stats")]
    [Tooltip("Multiplier for how well this specific enemy hears. 1 is normal, 2 is double distance.")]
    public float hearingSensitivity = 1f;

    [Header("Reaction Settings")]
    public float suspicionDuration = 3f;
    private Coroutine suspicionRoutine;

    // Called by the dropping box because of the ISoundListener interface
    public void OnSoundHeard(Vector3 soundLocation, Transform soundSource)
    {
//        Debug.Log($"{gameObject.name} HEARD a sound at {soundLocation}!");
    
        // Tell the State Manager to switch to Investigate State
        GetComponentInParent<EnemyStateManager>().TriggerInvestigation(soundLocation);
    }

    private IEnumerator InvestigateSound(Vector3 location)
    {
        // Placeholder for NavMeshAgent? to walk to 'location'
        Debug.Log($"{gameObject.name} is investigating the noise...");
        
        yield return new WaitForSeconds(suspicionDuration);
        
        Debug.Log($"{gameObject.name} lost interest.");
    }

    // Optional: GUI to match your FieldOfView text
    private void OnGUI() 
    {
        if (suspicionRoutine != null) 
        {
            GUI.color = Color.yellow;
            GUI.skin.label.fontSize = 20;
            GUI.skin.label.fontStyle = FontStyle.Bold;

            Vector3 worldPos = transform.position + Vector3.up * 2.5f;
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

            if (screenPos.z > 0) 
            {
                float guiX = screenPos.x;
                float guiY = Screen.height - screenPos.y;
                GUI.Label(new Rect(guiX - 50, guiY, 200, 30), "HEARD");
            }
        }
    }
}