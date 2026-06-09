using UnityEngine;

/// <summary>
/// Controls the visual feedback and animations for the enemy. 
/// Manages the overhead state indicator sprites (e.g., chasing, confused), Animator state updates, and renders debug patrol paths.
/// </summary>
public class EnemyVisuals : MonoBehaviour
{
    private EnemyLocomotion locomotion;

    [Header("Guard Icons")]
    [SerializeField] private SpriteRenderer stateSpriteRenderer;
    [SerializeField] private Sprite heardASoundIcon;
    [SerializeField] private Sprite chasingPlayerIcon;
    [SerializeField] private Sprite lookingAroundConfusedIcon;
    [SerializeField] private Sprite guardIsStunnedIcon;
    [SerializeField] private Sprite guardHasFinishedLookingAroundAndDidntFindAnythingSoBackToPatrolling;

    [Header("Path Visuals Setup")] 
    public bool showGuardPaths = false;
    [SerializeField] private Color pathColor = Color.red; 
    [SerializeField] private float pathLineWidth = 0.15f;
    [HideInInspector] public LineRenderer lineRenderer;

    public Animator animator;

    private void Awake()
    {
        locomotion = GetComponent<EnemyLocomotion>();
    }

    private void Start()
    {
        if (showGuardPaths)
        {
            SetUpGuardPathingLines();
        }
        
        animator.SetBool("isMoving", false);
    }

    public void SetStateIcon(EnemyStateIcon iconType)
    {
        switch (iconType)
        {
            case EnemyStateIcon.HeardASound:
                stateSpriteRenderer.sprite = heardASoundIcon;
                stateSpriteRenderer.enabled = true;
                break;
            case EnemyStateIcon.ChasingPlayer:
                stateSpriteRenderer.sprite = chasingPlayerIcon;
                stateSpriteRenderer.enabled = true;
                break;
            case EnemyStateIcon.LookingAroundConfused:
                stateSpriteRenderer.sprite = lookingAroundConfusedIcon;
                stateSpriteRenderer.enabled = true;
                break;
            case EnemyStateIcon.HideIcon:
                stateSpriteRenderer.sprite = null;
                stateSpriteRenderer.enabled = false;
                break;
            case EnemyStateIcon.FinishedLookingAroundAndDidntFindAnythingSoBackToPatrolling:
                stateSpriteRenderer.sprite = guardHasFinishedLookingAroundAndDidntFindAnythingSoBackToPatrolling;
                stateSpriteRenderer.enabled = true;
                break;
        }
    }

    public void SetUpGuardPathingLines()
    {
        if (locomotion.waypoints == null || locomotion.waypoints.Length == 0) return;

        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        
        // Colour
        lineRenderer.startColor = pathColor;
        lineRenderer.endColor = pathColor;
        
        // Width
        lineRenderer.startWidth = pathLineWidth;
        lineRenderer.endWidth = pathLineWidth;

        // Make the lines on the ground
        Vector3[] linePositions = new Vector3[locomotion.waypoints.Length];
        for (int i = 0; i < locomotion.waypoints.Length; i++)
        {
            linePositions[i] = new Vector3(locomotion.waypoints[i].x, 0f, locomotion.waypoints[i].z);
        }
        
        lineRenderer.loop = true;
        lineRenderer.positionCount = linePositions.Length;
        lineRenderer.SetPositions(linePositions);
    }

    private void OnDrawGizmos()
    {
        // To draw gizmos before runtime, we need the pathHolder directly
        EnemyLocomotion loc = GetComponent<EnemyLocomotion>();
        if (loc == null || loc.pathHolder == null || !showGuardPaths) return;

        Gizmos.color = pathColor;

        for (int i = 0; i < loc.pathHolder.childCount - 1; i++)
        {
            Vector3 startPos = loc.pathHolder.GetChild(i).position;
            Vector3 endPos = loc.pathHolder.GetChild(i + 1).position;

            Gizmos.DrawLine(startPos, endPos);
        }

        // Connect last waypoint back to first
        if (loc.pathHolder.childCount > 1)
        {
            Vector3 lastPos = loc.pathHolder.GetChild(loc.pathHolder.childCount - 1).position;
            Vector3 firstPos = loc.pathHolder.GetChild(0).position;

            Gizmos.DrawLine(lastPos, firstPos);
        }
    }
}