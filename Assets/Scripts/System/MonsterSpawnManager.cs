using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages monster spawning based on day progression and spawn states
/// </summary>
public class MonsterSpawnManager : MonoBehaviour
{
    public static MonsterSpawnManager Instance { get; private set; }

    [Header("Monster Spawn Configuration")]
    [Tooltip("All monster types and their spawn data")]
    public List<MonsterSpawnData> monsterSpawnData = new List<MonsterSpawnData>();

    [Header("Encounter Settings")]
    [Tooltip("Minimum number of monsters in an encounter")]
    [Range(1, 8)]
    public int minMonsterCount = 1;
    
    [Tooltip("Maximum number of monsters in an encounter")]
    [Range(1, 8)]
    public int maxMonsterCount = 8;
    
    [Tooltip("Maximum different monster types in one encounter")]
    [Range(1, 3)]
    public int maxMonsterTypes = 3;

    [Header("Ecosystem Settings")]
    [Tooltip("Maximum number of Wandering monster types before extinction events can occur")]
    [Range(4, 10)]
    public int maxWanderingTypes = 6;
    
    [Tooltip("Chance per rest for extinction event to occur when over capacity")]
    [Range(0f, 1f)]
    public float extinctionChance = 0.4f;

    private List<string> pendingAnnouncementTexts = new List<string>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Updates monster spawn states based on current day
    /// Called after resting
    /// </summary>
    public void UpdateSpawnStatesForDay(int currentDay)
    {
        pendingAnnouncementTexts.Clear();

        // TODO pick only one of the spawn data to change state or be killed
        foreach (var spawnData in monsterSpawnData)
        {
            if (spawnData.monsterPrefab == null) continue; 

            // Check if a Waiting monster should become active
            if (spawnData.spawnState == MonsterSpawnState.Waiting && 
                currentDay >= spawnData.dayThreshold)
            {
                spawnData.spawnState = MonsterSpawnState.Wandering_Rare;
                pendingAnnouncementTexts.Add(spawnData.GetRumorText());
                
                Debug.Log($"Day {currentDay}: {spawnData.GetMonsterName()} now Wandering_Rare");
                return; //break so this only happens once
            }
            // Check if a Wandering_Rare monster should become common  
            else if (spawnData.spawnState == MonsterSpawnState.Wandering_Rare)
            {
                // Random chance to promote rare to common (20% per rest)
                if (Random.value < 0.2f)
                {
                    spawnData.spawnState = MonsterSpawnState.Wandering;
                    pendingAnnouncementTexts.Add(spawnData.GetCommonText());
                    
                    Debug.Log($"Day {currentDay}: {spawnData.GetMonsterName()} now Wandering");
                    return; //break so this only happens once
                }
            }
        }

        // Check for ecosystem overcrowding and extinction events
        ProcessEcosystemBalance(currentDay);
    }

    /// <summary>
    /// Processes ecosystem balance - stronger monsters eliminate weaker ones when overcrowded
    /// </summary>
    void ProcessEcosystemBalance(int currentDay)
    {
        // Count Wandering monsters
        List<MonsterSpawnData> wanderingMonsters = monsterSpawnData
            .Where(data => data.spawnState == MonsterSpawnState.Wandering && data.monsterPrefab != null)
            .ToList();

        // Check if we're over capacity
        if (wanderingMonsters.Count <= maxWanderingTypes)
            return;

        // Random chance for extinction event
        if (Random.value > extinctionChance)
            return;

        Debug.Log($"Ecosystem overcrowded: {wanderingMonsters.Count} wandering types (max: {maxWanderingTypes})");

        // Find potential victims (weaker monsters with lower day thresholds)
        List<MonsterSpawnData> potentialVictims = wanderingMonsters
            .OrderBy(data => data.dayThreshold)
            .Take(wanderingMonsters.Count - maxWanderingTypes + 1) // Get the weakest ones
            .ToList();

        if (potentialVictims.Count == 0)
            return;

        // Pick a random victim from the weakest
        MonsterSpawnData victim = potentialVictims[Random.Range(0, potentialVictims.Count)];

        // Find potential predators (stronger monsters with higher day thresholds)
        List<MonsterSpawnData> potentialPredators = wanderingMonsters
            .Where(data => data.dayThreshold > victim.dayThreshold && data != victim)
            .OrderByDescending(data => data.dayThreshold)
            .ToList();

        if (potentialPredators.Count == 0)
        {
            Debug.Log($"No valid predators found for {victim.GetMonsterName()}");
            return;
        }

        // Pick a random predator from the stronger ones
        MonsterSpawnData predator = potentialPredators[Random.Range(0, Mathf.Min(3, potentialPredators.Count))];

        // Execute extinction
        victim.spawnState = MonsterSpawnState.Extinct;
        string extinctionMessage = victim.GetExtinctionText(predator.GetMonsterName());
        pendingAnnouncementTexts.Add(extinctionMessage);

        Debug.Log($"EXTINCTION: {victim.GetMonsterName()} (Day {victim.dayThreshold}) eliminated by {predator.GetMonsterName()} (Day {predator.dayThreshold})");
    }

    /// <summary>
    /// Gets pending announcement texts and clears them
    /// </summary>
    public List<string> GetAndClearAnnouncements()
    {
        List<string> announcements = new List<string>(pendingAnnouncementTexts);
        pendingAnnouncementTexts.Clear();
        return announcements;
    }

    /// <summary>
    /// Spawns monsters for a random encounter
    /// </summary>
    public List<Monster> SpawnEncounter()
    {
        List<Monster> spawnedMonsters = new List<Monster>();

        // Get all spawnable monster types
        List<MonsterSpawnData> spawnableTypes = monsterSpawnData
            .Where(data => data.CanSpawn() && data.monsterPrefab != null)
            .ToList();

        if (spawnableTypes.Count == 0)
        {
            Debug.LogWarning("No spawnable monsters available!");
            return spawnedMonsters;
        }

        // Determine number of monsters
        int monsterCount = Random.Range(minMonsterCount, maxMonsterCount + 1);

        // Select monster types with weighted random selection
        List<MonsterSpawnData> selectedTypes = SelectMonsterTypes(spawnableTypes);

        if (selectedTypes.Count == 0)
        {
            Debug.LogWarning("Failed to select any monster types!");
            return spawnedMonsters;
        }

        // Spawn monsters
        for (int i = 0; i < monsterCount; i++)
        {
            // Pick a random type from selected types
            MonsterSpawnData typeToSpawn = selectedTypes[Random.Range(0, selectedTypes.Count)];
            Monster monster = typeToSpawn.monsterPrefab.GetComponent<Monster>().CreateCopy();
            monster.gameObject.SetActive(false);
            spawnedMonsters.Add(monster);
        }

        return spawnedMonsters;
    }

    /// <summary>
    /// Selects which monster types to include in this encounter
    /// </summary>
    List<MonsterSpawnData> SelectMonsterTypes(List<MonsterSpawnData> availableTypes)
    {
        List<MonsterSpawnData> selectedTypes = new List<MonsterSpawnData>();

        // Determine how many types to include
        int typeCount = Mathf.Min(Random.Range(1, maxMonsterTypes + 1), availableTypes.Count);

        // Use weighted random selection
        List<MonsterSpawnData> remainingTypes = new List<MonsterSpawnData>(availableTypes);

        for (int i = 0; i < typeCount; i++)
        {
            if (remainingTypes.Count == 0) break;

            MonsterSpawnData selected = WeightedRandomSelection(remainingTypes);
            if (selected != null)
            {
                selectedTypes.Add(selected);
                remainingTypes.Remove(selected);
            }
        }

        return selectedTypes;
    }

    /// <summary>
    /// Performs weighted random selection from a list of monster spawn data
    /// </summary>
    MonsterSpawnData WeightedRandomSelection(List<MonsterSpawnData> options)
    {
        if (options.Count == 0) return null;
        if (options.Count == 1) return options[0];

        // Calculate total weight
        float totalWeight = 0f;
        foreach (var option in options)
        {
            totalWeight += option.GetSpawnWeight();
        }

        if (totalWeight <= 0f)
        {
            // Fallback to uniform random if no valid weights
            return options[Random.Range(0, options.Count)];
        }

        // Random selection based on weight
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        foreach (var option in options)
        {
            currentWeight += option.GetSpawnWeight();
            if (randomValue <= currentWeight)
            {
                return option;
            }
        }

        // Fallback (shouldn't reach here)
        return options[options.Count - 1];
    }

    /// <summary>
    /// Gets a list of all currently spawnable monsters for debugging
    /// </summary>
    public List<string> GetSpawnableMonsterNames()
    {
        return monsterSpawnData
            .Where(data => data.CanSpawn() && data.monsterPrefab != null)
            .Select(data => $"{data.GetMonsterName()} ({data.spawnState})")
            .ToList();
    }

    /// <summary>
    /// Force a monster to a specific spawn state (for debugging/events)
    /// </summary>
    public void SetMonsterSpawnState(string monsterName, MonsterSpawnState newState)
    {
        foreach (var spawnData in monsterSpawnData)
        {
            if (spawnData.GetMonsterName() == monsterName)
            {
                spawnData.spawnState = newState;
                Debug.Log($"Set {monsterName} to {newState}");
                return;
            }
        }
        
        Debug.LogWarning($"Monster {monsterName} not found in spawn data!");
    }

    /// <summary>
    /// Gets current ecosystem status for debugging
    /// </summary>
    public string GetEcosystemStatus()
    {
        int wandering = monsterSpawnData.Count(d => d.spawnState == MonsterSpawnState.Wandering);
        int wanderingRare = monsterSpawnData.Count(d => d.spawnState == MonsterSpawnState.Wandering_Rare);
        int waiting = monsterSpawnData.Count(d => d.spawnState == MonsterSpawnState.Waiting);
        int extinct = monsterSpawnData.Count(d => d.spawnState == MonsterSpawnState.Extinct);

        string status = $"Ecosystem Status:\n";
        status += $"Wandering: {wandering}/{maxWanderingTypes} (Overcrowded: {(wandering > maxWanderingTypes ? "YES" : "NO")})\n";
        status += $"Wandering_Rare: {wanderingRare}\n";
        status += $"Waiting: {waiting}\n";
        status += $"Extinct: {extinct}\n";

        return status;
    }
}