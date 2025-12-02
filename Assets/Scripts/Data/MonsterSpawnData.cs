using UnityEngine;

/// <summary>
/// Tracks spawn state and progression for a monster type
/// </summary>
[CreateAssetMenu(fileName = "New Monster Spawn Data", menuName = "DungeonPunks/Monster Spawn Data")]
public class MonsterSpawnData : ScriptableObject
{
    [Header("Monster Reference")]
    public GameObject monsterPrefab;
    
    [Header("Spawn Configuration")]
    [Tooltip("Current spawn state of this monster type")]
    public MonsterSpawnState spawnState = MonsterSpawnState.Waiting;
    
    [Tooltip("Day when this monster becomes available (if in Waiting state)")]
    public int dayThreshold = 1;
    
    [Header("Spawn Weights")]
    [Tooltip("Chance multiplier when Wandering (1.0 = normal)")]
    [Range(0.1f, 3f)]
    public float normalWeight = 1f;
    
    [Tooltip("Chance multiplier when Wandering_Rare (0.3 = 30% of normal)")]
    [Range(0.1f, 1f)]
    public float rareWeight = 0.3f;

    [Header("Flavor Text")]
    [Tooltip("Text shown when transitioning from Waiting to Wandering_Rare")]
    public string rumorText = "You hear rumors that {MONSTER} found their way into the Dungeon";
    
    [Tooltip("Text shown when transitioning from Wandering_Rare to Wandering")]
    public string commonText = "The dungeon is full of sounds of wandering {MONSTER}";
    
    [Tooltip("Text shown when this monster is eliminated by another (use {MONSTER} for victim, {KILLER} for predator)")]
    public string extinctionText = "You hear screams of {MONSTER}, later you find their remains torn into pieces by {KILLER}";

    /// <summary>
    /// Gets the effective spawn weight based on current state
    /// </summary>
    public float GetSpawnWeight()
    {
        switch (spawnState)
        {
            case MonsterSpawnState.Wandering:
                return normalWeight;
            case MonsterSpawnState.Wandering_Rare:
                return normalWeight * rareWeight;
            case MonsterSpawnState.Waiting:
            case MonsterSpawnState.Extinct:
            default:
                return 0f;
        }
    }

    /// <summary>
    /// Checks if this monster can spawn
    /// </summary>
    public bool CanSpawn()
    {
        return spawnState == MonsterSpawnState.Wandering || 
               spawnState == MonsterSpawnState.Wandering_Rare;
    }

    /// <summary>
    /// Gets the monster's base name for text replacement
    /// </summary>
    public string GetMonsterName()
    {
        if (monsterPrefab == null) return "Unknown";
        
        Monster monster = monsterPrefab.GetComponent<Monster>();
        return monster != null ? monster.baseMonsterName : monsterPrefab.name;
    }

    /// <summary>
    /// Gets the rumor text with monster name inserted
    /// </summary>
    public string GetRumorText()
    {
        return rumorText.Replace("{MONSTER}", GetMonsterName());
    }

    /// <summary>
    /// Gets the common text with monster name inserted
    /// </summary>
    public string GetCommonText()
    {
        return commonText.Replace("{MONSTER}", GetMonsterName());
    }

    /// <summary>
    /// Gets the extinction text with both victim and killer names inserted
    /// </summary>
    public string GetExtinctionText(string killerName)
    {
        return extinctionText
            .Replace("{MONSTER}", GetMonsterName())
            .Replace("{KILLER}", killerName);
    }
}