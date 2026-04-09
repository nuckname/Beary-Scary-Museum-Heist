using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using TMPro;

public class RoundInProgressState : RoundBaseState
{
    private bool playerIsInGreenZone = false;
    
    public override void EnterState(RoundStateManager manager)
    {
        Debug.Log("Round is in progress.");

        //manager.timer.isPaused = false;

        manager.MakeGuardsStartMoving();

        // Trigger the dramatic Elden Ring popup
        SpawnEldenRingPopup(manager.currentRoundIndex);
    }

    private void SpawnEldenRingPopup(int roundIndex)
    {
        // 1. Find an existing Canvas, or create a dynamic one on the fly
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("DynamicPopupCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100; // Force it over everything else
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }

        // 2. Create the Text GameObject
        GameObject popupObj = new GameObject("EldenRingRoundPopup");
        popupObj.transform.SetParent(canvas.transform, false);

        // 3. Add and configure TextMeshProUGUI
        TextMeshProUGUI tmpText = popupObj.AddComponent<TextMeshProUGUI>();
        tmpText.text = $"ROUND {roundIndex}"; // Starts at 0 based on your manager's default value
        tmpText.fontSize = 140; // Big!
        tmpText.alignment = TextAlignmentOptions.Center;
        tmpText.fontStyle = FontStyles.Bold;
        
        // Classic faint golden Elden Ring color (Starting alpha at 0 for the fade)
        tmpText.color = new Color(0.85f, 0.75f, 0.45f, 0f);

        // 4. Center it perfectly on the screen via RectTransform
        RectTransform rect = tmpText.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(1200, 300);
        rect.anchoredPosition = Vector2.zero;

        // 5. Attach the animator and fire it
        EldenRingTextFader fader = popupObj.AddComponent<EldenRingTextFader>();
        fader.StartSequence();
    }

    public override void UpdateState(RoundStateManager manager)
    {
    }

    public override void OnCollisionEnter(RoundStateManager manager, Collision collision)
    {
    }

    public override void OnCollisionExit(RoundStateManager manager, Collision collision)
    {
    }

    public override void OnTriggerEnter(RoundStateManager manager, Collider other)
    {
        HandlePlayerCollision(manager, other);
        HandleArtifactCollision(manager, other);
    }

    private static void HandleArtifactCollision(RoundStateManager manager, Collider other)
    {
        if (other.gameObject.CompareTag("CanPickUp"))
        {
            IArtifact artifact = other.GetComponent<IArtifact>();
        
            if (artifact != null)
            {
                if (!artifact.hasBeenUsed)
                {
                    manager.currentArtifacts++;
                    
                    artifact.hasBeenUsed = true; 
                    
                    manager.UpdateUI();
                }
            }
        }
    }

    private static void HandlePlayerCollision(RoundStateManager manager, Collider other)
    {
        // If player is in zone and we have all artifacts collected go to next round
        if (other.gameObject.CompareTag("Player"))
        {
            if(manager.currentArtifacts == manager.amountOfArtifactsToCompleteLevel)
            {
                manager.SwitchState(manager.AboutToStartState);
            }
        }
    }

    public override void OnTriggerExit(RoundStateManager manager, Collider other)
    {
    }  
}