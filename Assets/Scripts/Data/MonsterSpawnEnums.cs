/// <summary>
/// Defines how a monster type can spawn in encounters
/// </summary>
public enum MonsterSpawnState
{
    Extinct,        // Never appears in encounters
    Wandering,      // Normal spawn probability
    Wandering_Rare, // Lower spawn probability
    Waiting         // Not yet spawning, will activate at day threshold
}