using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages prisoner placement and rescue mechanics
/// </summary>
public class PrisonerSystem : MonoBehaviour
{
    public static PrisonerSystem Instance { get; private set; }

    [Header("Prisoner Configuration")]
    [Tooltip("Number of prisoners to place in the dungeon")]
    [Range(1, 20)]
    public int prisonerCount = 10;

    [Header("Prisoner Names Pool")]
    public List<string> prisonerNames = new List<string>
    {
        "Aldric", "Beatrice", "Cedric", "Diana", "Edmund",
        "Fiona", "Gareth", "Helena", "Isaac", "Jasmine",
        "Kael", "Lyra", "Marcus", "Nora", "Oliver",
        "Petra", "Quinlan", "Rosa", "Silas", "Thalia"
    };

    [Header("Prisoner Types Pool")]
    public List<string> prisonerTypes = new List<string>
    {
        "Warrior", "Mage", "Thief", "Cleric", "Ranger",
        "Paladin", "Bard", "Monk", "Druid", "Warlock"
    };

    [Header("Placement Settings")]
    [Tooltip("Minimum distance between prisoners (in cells)")]
    public int minDistanceBetweenPrisoners = 5;

    private List<PrisonerData> prisoners = new List<PrisonerData>();
    private Vector2Int startPosition;
    private bool isInitialized = false;

    public int RescuedCount => prisoners.Count(p => p.hasBeenRescued);
    public int TotalPrisoners => prisoners.Count;
    public bool AllPrisonersRescued => RescuedCount == TotalPrisoners;

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
    /// Initializes the prisoner system and places prisoners in the maze
    /// </summary>
    public void Initialize(Vector2Int startPos)
    {
        if (isInitialized) return;

        startPosition = startPos;
        PlacePrisoners();
        isInitialized = true;

        // Initialize lighting system
        if (PrisonerLightingSystem.Instance != null && MazeGenerator.Instance != null)
        {
            float cellSize = MazeGenerator.Instance.cellSize;
            PrisonerLightingSystem.Instance.InitializeLights(startPosition, prisoners, cellSize);
        }

        Debug.Log($"Prisoner System Initialized: {prisoners.Count} prisoners placed");
    }

    /// <summary>
    /// Places prisoners evenly distributed throughout the maze
    /// </summary>
    void PlacePrisoners()
    {
        prisoners.Clear();

        if (MazeGenerator.Instance == null)
        {
            Debug.LogError("MazeGenerator not found! Cannot place prisoners.");
            return;
        }

        // Get all walkable positions
        List<Vector2Int> walkablePositions = GetAllWalkablePositions();

        if (walkablePositions.Count == 0)
        {
            Debug.LogError("No walkable positions found!");
            return;
        }

        // Remove start position from candidates
        walkablePositions.Remove(startPosition);

        // Shuffle names and types
        List<string> shuffledNames = new List<string>(prisonerNames);
        List<string> shuffledTypes = new List<string>(prisonerTypes);
        ShuffleList(shuffledNames);
        ShuffleList(shuffledTypes);

        // Place prisoners with even distribution
        List<Vector2Int> prisonerPositions = SelectEvenlyDistributedPositions(
            walkablePositions, 
            prisonerCount
        );

        // Create prisoner data
        for (int i = 0; i < prisonerPositions.Count; i++)
        {
            string name = shuffledNames[i % shuffledNames.Count];
            string type = shuffledTypes[i % shuffledTypes.Count];
            
            PrisonerData prisoner = new PrisonerData(name, type, prisonerPositions[i]);
            prisoners.Add(prisoner);

            Debug.Log($"Prisoner {i + 1}: {name} the {type} at {prisonerPositions[i]}");
        }
    }

    /// <summary>
    /// Gets all walkable positions in the maze
    /// </summary>
    List<Vector2Int> GetAllWalkablePositions()
    {
        List<Vector2Int> walkable = new List<Vector2Int>();

        int size = MazeGenerator.Instance.SizeOfDungeon;

        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                if (MazeGenerator.Instance.IsCellWalkable(x, z))
                {
                    walkable.Add(new Vector2Int(x, z));
                }
            }
        }

        return walkable;
    }

    /// <summary>
    /// Selects positions that are evenly distributed throughout the maze
    /// </summary>
    List<Vector2Int> SelectEvenlyDistributedPositions(List<Vector2Int> candidates, int count)
    {
        List<Vector2Int> selected = new List<Vector2Int>();
        List<Vector2Int> remaining = new List<Vector2Int>(candidates);

        if (remaining.Count == 0) return selected;

        // First prisoner: pick a random far position from start
        List<Vector2Int> farPositions = remaining
            .OrderByDescending(pos => Vector2Int.Distance(pos, startPosition))
            .Take(remaining.Count / 4) // Top 25% farthest
            .ToList();

        if (farPositions.Count > 0)
        {
            Vector2Int first = farPositions[Random.Range(0, farPositions.Count)];
            selected.Add(first);
            remaining.Remove(first);
        }

        // Subsequent prisoners: maximize distance from already placed prisoners
        while (selected.Count < count && remaining.Count > 0)
        {
            Vector2Int bestPosition = remaining[0];
            float bestMinDistance = 0f;

            // Find position with maximum minimum distance to all selected prisoners
            foreach (var candidate in remaining)
            {
                float minDistance = float.MaxValue;

                foreach (var placed in selected)
                {
                    float distance = Vector2Int.Distance(candidate, placed);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                    }
                }

                if (minDistance > bestMinDistance)
                {
                    bestMinDistance = minDistance;
                    bestPosition = candidate;
                }
            }

            selected.Add(bestPosition);
            remaining.Remove(bestPosition);
        }

        return selected;
    }

    /// <summary>
    /// Shuffles a list in place
    /// </summary>
    void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    /// <summary>
    /// Checks if a position contains a prisoner
    /// </summary>
    public bool IsPrisonerAt(Vector2Int position)
    {
        return prisoners.Any(p => p.gridPosition == position && !p.hasBeenRescued);
    }

    /// <summary>
    /// Checks if player is at the start position
    /// </summary>
    public bool IsAtStartPosition(Vector2Int position)
    {
        return position == startPosition;
    }

    /// <summary>
    /// Rescues a prisoner at the given position
    /// </summary>
    public PrisonerData RescuePrisoner(Vector2Int position)
    {
        PrisonerData prisoner = prisoners.FirstOrDefault(p => 
            p.gridPosition == position && !p.hasBeenRescued
        );

        if (prisoner != null)
        {
            prisoner.hasBeenRescued = true;
            
            // Disable the light at this position
            if (PrisonerLightingSystem.Instance != null)
            {
                PrisonerLightingSystem.Instance.DisablePrisonerLight(position);
            }
            
            Debug.Log($"Rescued: {prisoner.prisonerName} ({RescuedCount}/{TotalPrisoners})");
        }

        return prisoner;
    }

    /// <summary>
    /// Gets the entrance message
    /// </summary>
    public string GetEntranceMessage()
    {
        int remaining = TotalPrisoners - RescuedCount;

        if (AllPrisonersRescued)
        {
            return "All your friends escaped and you can leave this cursed place for good.\n\n" +
                   "<b>Thank you for playing Gutter Knight!</b>";
        }
        else
        {
            return $"This is the entrance to the abandoned dungeon of the jailer guild.\n\n" +
                   $"{remaining} of your friends are still imprisoned here.\n\nFind them and return here...";
        }
    }

    /// <summary>
    /// Gets debug info about prisoner positions
    /// </summary>
    public string GetDebugInfo()
    {
        string info = $"Prisoners: {RescuedCount}/{TotalPrisoners}\n";
        
        foreach (var prisoner in prisoners)
        {
            string status = prisoner.hasBeenRescued ? "RESCUED" : "IMPRISONED";
            info += $"- {prisoner.prisonerName} ({prisoner.prisonerType}) at {prisoner.gridPosition}: {status}\n";
        }

        return info;
    }

    /// <summary>
    /// Resets the prisoner system (for game restart)
    /// </summary>
    public void Reset()
    {
        prisoners.Clear();
        isInitialized = false;
        
        // Clear all lights
        if (PrisonerLightingSystem.Instance != null)
        {
            PrisonerLightingSystem.Instance.ClearAllLights();
        }
    }
}