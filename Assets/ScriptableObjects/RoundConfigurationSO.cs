using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRoundConfig", menuName = "Game/Round Configuration")]
public class RoundConfigurationSO : ScriptableObject
{
    [Header("Round Settings")]
    public int amountOfArtifactsToCompleteLevel = 3;

    [Header("Scene Modification")]
    [Tooltip("disable only this round turn them back on next round.")]
    public List<int> objectsToRemoveIndices;

    [Header("Spawning")]
    public List<GuardSpawnData> guardsToSpawn;
    public List<ArtifactSpawnData> artifactsToSpawn;
}

