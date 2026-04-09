using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GuardSpawnData
{
    public GameObject guardPrefab;
    
    public int spawnPointIndex;
    
    public int pathHolderIndex;

    [Header("Guard Settings")]
    public float guardPatrolSpeed = 3f;
    public float guardChaseSpeed = 5f;
    public int amountOfTimesTheGuardTurns = 2;
    public float turnVisionRadiusReductionPercentage = 0.5f;
    public bool alwaysTurnRight = false;
}