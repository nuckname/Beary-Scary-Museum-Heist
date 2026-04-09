using UnityEngine;

public class GlobalEnemyDebugConsole : MonoBehaviour
{
    [Header("Target Enemies")]
    [Tooltip("Drag in your enemies here. If left empty, the script will auto-find all EnemyStateManagers in the scene.")]
    public EnemyStateManager[] enemies;

    // GUI State
    private bool isOpen = false;
    private Rect windowRect = new Rect(10, 10, 300, 420);

    // Master Settings Values
    private float m_patrolSpeed;
    private float m_chaseSpeed;
    private float m_turnSpeed;
    private float m_waitTime;
    private float m_noiseRadius;
    private int m_turnAngle;
    private bool m_investigateLastSight;
    private bool m_makeNoise;
    private bool m_showGuardPaths;

    private void Start()
    {
        // Auto-populate the array if you forget to drag them in
        if (enemies == null || enemies.Length == 0)
        {
            enemies = FindObjectsOfType<EnemyStateManager>();
        }

        // Initialize the master sliders based on the first enemy in the array
        if (enemies.Length > 0 && enemies[0] != null)
        {
            m_patrolSpeed = enemies[0].guardPatrollSpeed;
            m_chaseSpeed = enemies[0].guardChaseSpeed;
            m_turnSpeed = enemies[0].turnSpeed;
            m_waitTime = enemies[0].waitTime;
            m_noiseRadius = enemies[0].guardNoiseRadiusWhenTheySeeThePlayer;
            m_turnAngle = enemies[0].turnAngle;
            m_investigateLastSight = enemies[0].makeGuardsInvestiageLastPlayerLocationWhenTheyLoseSight;
            m_makeNoise = enemies[0].makeGuardsCreateNoiseWhenTheySeeThePlayer;
            m_showGuardPaths = enemies[0].showGuardPaths;
        }
    }

    private void OnGUI()
    {
        // 1. Draw the Toggle Button in the bottom-left corner
        float btnWidth = 120f;
        float btnHeight = 30f;
        Rect toggleBtnRect = new Rect(10, Screen.height - btnHeight - 10, btnWidth, btnHeight);

        if (GUI.Button(toggleBtnRect, isOpen ? "▼ Close Debug" : "▲ Enemy Debug"))
        {
            isOpen = !isOpen;
            if (isOpen)
            {
                // Snap the window to sit just above the toggle button
                windowRect.x = 10;
                windowRect.y = Screen.height - btnHeight - windowRect.height - 20;
            }
        }

        // 2. Draw the Console Window if open
        if (isOpen)
        {
            windowRect = GUI.Window(0, windowRect, DrawConsoleWindow, "Global Enemy Settings");
        }
    }

    private void DrawConsoleWindow(int windowID)
    {
        GUILayout.Space(10);

        if (enemies == null || enemies.Length == 0)
        {
            GUILayout.Label("No enemies found in array!");
            GUI.DragWindow();
            return;
        }

        // Track if the user interacted with any UI element this frame
        GUI.changed = false;

        // --- SPEED SETTINGS ---
        GUILayout.Label($"Patrol Speed: {m_patrolSpeed:F1}");
        m_patrolSpeed = GUILayout.HorizontalSlider(m_patrolSpeed, 1f, 10f);

        GUILayout.Label($"Chase Speed: {m_chaseSpeed:F1}");
        m_chaseSpeed = GUILayout.HorizontalSlider(m_chaseSpeed, 1f, 15f);

        // --- BEHAVIOR SETTINGS ---
        GUILayout.Label($"Wait Time: {m_waitTime:F1}");
        m_waitTime = GUILayout.HorizontalSlider(m_waitTime, 0f, 10f);

        GUILayout.Label($"Turn Speed: {m_turnSpeed:F1}");
        m_turnSpeed = GUILayout.HorizontalSlider(m_turnSpeed, 10f, 360f);

        GUILayout.Label($"Turn Angle: {m_turnAngle}");
        m_turnAngle = (int)GUILayout.HorizontalSlider(m_turnAngle, 10, 180);

        GUILayout.Label($"Noise Radius: {m_noiseRadius:F1}");
        m_noiseRadius = GUILayout.HorizontalSlider(m_noiseRadius, 1f, 20f);

        GUILayout.Space(10);

        // --- TOGGLES ---
        m_investigateLastSight = GUILayout.Toggle(m_investigateLastSight, " Investigate Last Sight");
        m_makeNoise = GUILayout.Toggle(m_makeNoise, " Make Noise On Sight");
        m_showGuardPaths = GUILayout.Toggle(m_showGuardPaths, " Show Guard Paths");
        
        // If any slider or toggle was adjusted, apply the new values to all enemies
        if (GUI.changed)
        {
            ApplyValuesToAllEnemies();
        }

        // Allow the user to drag the window around the screen
        GUI.DragWindow();
    }

    private void ApplyValuesToAllEnemies()
    {
        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;

            enemy.guardPatrollSpeed = m_patrolSpeed;
            enemy.guardChaseSpeed = m_chaseSpeed;
            enemy.turnSpeed = m_turnSpeed;
            enemy.waitTime = m_waitTime;
            enemy.turnAngle = m_turnAngle;
            enemy.guardNoiseRadiusWhenTheySeeThePlayer = m_noiseRadius;
            
            enemy.makeGuardsInvestiageLastPlayerLocationWhenTheyLoseSight = m_investigateLastSight;
            enemy.makeGuardsCreateNoiseWhenTheySeeThePlayer = m_makeNoise;

            // Handle Pathing Lines
            enemy.showGuardPaths = m_showGuardPaths;
            if (m_showGuardPaths && enemy.lineRenderer == null)
            {
                enemy.SetUpGuardPathingLines();
            }
            else if (enemy.lineRenderer != null)
            {
                enemy.lineRenderer.enabled = m_showGuardPaths;
            }

            // Optional: If an enemy is currently patrolling/chasing, instantly update their active agent speed
            if (enemy.agent != null && !enemy.agent.isStopped)
            {
                if (enemy.EnemyCurrentState == enemy.EnemyFollowPathState)
                    enemy.agent.speed = m_patrolSpeed;
                else if (enemy.EnemyCurrentState == enemy.EnemyChasePlayerState)
                    enemy.agent.speed = m_chaseSpeed;
            }
        }
    }
}