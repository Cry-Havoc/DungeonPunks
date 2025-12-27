using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MazeGenerator : MonoBehaviour
{
    public static MazeGenerator Instance { get; private set; }

    [Header("Maze Settings")]
    public int SizeOfDungeon = 15;

    [Header("Prefabs")]
    public GameObject floorPrefab;
    public GameObject ceilingPrefab;
    public List<GameObject> wallPrefabs = new List<GameObject>();

    [Header("Cell Settings")]
    public float cellSize = 2f;

    [Header("Prison Cell Settings")]
    [Tooltip("Number of prison cell corridors to generate")]
    [Range(3, 15)]
    public int prisonCorridorCount = 8;

    // The abstract maze data (true = wall, false = empty)
    private bool[,] mazeData;

    public bool[,] MazeData => mazeData;

    // Track prison cell positions for prisoner placement
    private List<Vector2Int> prisonCellPositions = new List<Vector2Int>();

    public List<Vector2Int> PrisonCellPositions => prisonCellPositions;

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

    void Start()
    {
        GenerateMaze();
        BuildMazeInScene();
    }

    void GenerateMaze()
    {
        mazeData = new bool[SizeOfDungeon, SizeOfDungeon];

        // Fill everything with walls initially
        for (int x = 0; x < SizeOfDungeon; x++)
        {
            for (int z = 0; z < SizeOfDungeon; z++)
            {
                mazeData[x, z] = true;
            }
        }

        // Create entrance hall structure first
        CreateEntranceHall();

        // Generate maze using recursive backtracking (starting from corridor ends)
        Stack<Vector2Int> stack = new Stack<Vector2Int>();

        // Get corridor end positions as starting points
        List<Vector2Int> corridorEnds = GetCorridorEnds();

        // Start from first corridor end
        if (corridorEnds.Count > 0)
        {
            Vector2Int start = corridorEnds[0];
            stack.Push(start);
        }

        Vector2Int[] directions = {
            new Vector2Int(0, 2),   // North
            new Vector2Int(2, 0),   // East
            new Vector2Int(0, -2),  // South
            new Vector2Int(-2, 0)   // West
        };

        while (stack.Count > 0)
        {
            Vector2Int current = stack.Peek();
            List<Vector2Int> unvisitedNeighbors = new List<Vector2Int>();

            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighbor = current + dir;

                if (neighbor.x > 0 && neighbor.x < SizeOfDungeon - 1 &&
                    neighbor.y > 0 && neighbor.y < SizeOfDungeon - 1 &&
                    mazeData[neighbor.x, neighbor.y])
                {
                    unvisitedNeighbors.Add(neighbor);
                }
            }

            if (unvisitedNeighbors.Count > 0)
            {
                Vector2Int next = unvisitedNeighbors[Random.Range(0, unvisitedNeighbors.Count)];
                Vector2Int between = new Vector2Int(
                    (current.x + next.x) / 2,
                    (current.y + next.y) / 2
                );

                mazeData[next.x, next.y] = false;
                mazeData[between.x, between.y] = false;
                stack.Push(next);
            }
            else
            {
                stack.Pop();
            }
        }

        // Connect remaining corridor ends to maze
        ConnectCorridorEndsToMaze(corridorEnds);

        // Create prison cell corridors throughout the maze
        CreatePrisonCellCorridors();
    }

    /// <summary>
    /// Creates the entrance hall: 3x3 room with 4 corridors extending 10 cells
    /// </summary>
    void CreateEntranceHall()
    {
        // Calculate center of dungeon (or near start)
        int centerX = 5; // Keep near corner for traditional start
        int centerZ = 5;

        // Create 3x3 room (center and surrounding cells)
        for (int x = centerX - 1; x <= centerX + 1; x++)
        {
            for (int z = centerZ - 1; z <= centerZ + 1; z++)
            {
                if (IsInBounds(x, z))
                {
                    mazeData[x, z] = false;
                }
            }
        }

        // Create North corridor (10 cells)
        for (int i = 1; i <= 10; i++)
        {
            int z = centerZ + 1 + i;
            if (IsInBounds(centerX, z))
            {
                mazeData[centerX, z] = false;
            }
        }

        // Create South corridor (10 cells)
        for (int i = 1; i <= 10; i++)
        {
            int z = centerZ - 1 - i;
            if (IsInBounds(centerX, z))
            {
                mazeData[centerX, z] = false;
            }
        }

        // Create East corridor (10 cells)
        for (int i = 1; i <= 10; i++)
        {
            int x = centerX + 1 + i;
            if (IsInBounds(x, centerZ))
            {
                mazeData[x, centerZ] = false;
            }
        }

        // Create West corridor (10 cells)
        for (int i = 1; i <= 10; i++)
        {
            int x = centerX - 1 - i;
            if (IsInBounds(x, centerZ))
            {
                mazeData[x, centerZ] = false;
            }
        }

        // Create cross sections at the end of each corridor
        CreateCrossSection(centerX, centerZ + 12);      // North end
        CreateCrossSection(centerX, centerZ - 12);      // South end
        CreateCrossSection(centerX + 12, centerZ);      // East end
        CreateCrossSection(centerX - 12, centerZ);      // West end
    }

    /// <summary>
    /// Creates a cross section (+ shape) at the given position
    /// </summary>
    void CreateCrossSection(int centerX, int centerZ)
    {
        // Center
        if (IsInBounds(centerX, centerZ))
            mazeData[centerX, centerZ] = false;

        // North
        if (IsInBounds(centerX, centerZ + 1))
            mazeData[centerX, centerZ + 1] = false;

        // South
        if (IsInBounds(centerX, centerZ - 1))
            mazeData[centerX, centerZ - 1] = false;

        // East
        if (IsInBounds(centerX + 1, centerZ))
            mazeData[centerX + 1, centerZ] = false;

        // West
        if (IsInBounds(centerX - 1, centerZ))
            mazeData[centerX - 1, centerZ] = false;
    }

    /// <summary>
    /// Gets the end positions of all corridors for maze generation
    /// </summary>
    List<Vector2Int> GetCorridorEnds()
    {
        int centerX = 5;
        int centerZ = 5;

        List<Vector2Int> ends = new List<Vector2Int>();

        // Corridor ends (where cross sections are)
        if (IsInBounds(centerX, centerZ + 12))
            ends.Add(new Vector2Int(centerX, centerZ + 12));

        if (IsInBounds(centerX, centerZ - 12))
            ends.Add(new Vector2Int(centerX, centerZ - 12));

        if (IsInBounds(centerX + 12, centerZ))
            ends.Add(new Vector2Int(centerX + 12, centerZ));

        if (IsInBounds(centerX - 12, centerZ))
            ends.Add(new Vector2Int(centerX - 12, centerZ));

        return ends;
    }

    /// <summary>
    /// Connects corridor ends to the main maze
    /// </summary>
    void ConnectCorridorEndsToMaze(List<Vector2Int> corridorEnds)
    {
        Stack<Vector2Int> stack = new Stack<Vector2Int>();

        Vector2Int[] directions = {
            new Vector2Int(0, 2),   // North
            new Vector2Int(2, 0),   // East
            new Vector2Int(0, -2),  // South
            new Vector2Int(-2, 0)   // West
        };

        // Try to extend from each corridor end
        foreach (Vector2Int end in corridorEnds)
        {
            // Try each direction
            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighbor = end + dir;
                Vector2Int between = new Vector2Int(
                    (end.x + neighbor.x) / 2,
                    (end.y + neighbor.y) / 2
                );

                if (IsInBounds(neighbor.x, neighbor.y) &&
                    IsInBounds(between.x, between.y) &&
                    mazeData[neighbor.x, neighbor.y])
                {
                    mazeData[neighbor.x, neighbor.y] = false;
                    mazeData[between.x, between.y] = false;
                    stack.Push(neighbor);

                    // Continue maze generation from this point
                    while (stack.Count > 0)
                    {
                        Vector2Int current = stack.Peek();
                        List<Vector2Int> unvisitedNeighbors = new List<Vector2Int>();

                        foreach (Vector2Int d in directions)
                        {
                            Vector2Int next = current + d;

                            if (IsInBounds(next.x, next.y) && mazeData[next.x, next.y])
                            {
                                unvisitedNeighbors.Add(next);
                            }
                        }

                        if (unvisitedNeighbors.Count > 0)
                        {
                            Vector2Int next = unvisitedNeighbors[Random.Range(0, unvisitedNeighbors.Count)];
                            Vector2Int betweenCells = new Vector2Int(
                                (current.x + next.x) / 2,
                                (current.y + next.y) / 2
                            );

                            mazeData[next.x, next.y] = false;
                            mazeData[betweenCells.x, betweenCells.y] = false;
                            stack.Push(next);
                        }
                        else
                        {
                            stack.Pop();
                        }
                    }

                    break; // Only need one connection per corridor end
                }
            }
        }
    }

    /// <summary>
    /// Creates prison cell corridors throughout the maze
    /// </summary>
    void CreatePrisonCellCorridors()
    {
        prisonCellPositions.Clear();

        // Find suitable locations for prison corridors
        List<PrisonCorridorCandidate> candidates = FindPrisonCorridorLocations();

        // Sort by distribution quality
        candidates = candidates.OrderByDescending(c => c.score).ToList();

        // Create the best corridors
        int created = 0;
        foreach (var candidate in candidates)
        {
            if (created >= prisonCorridorCount)
                break;

            if (CanPlacePrisonCorridor(candidate))
            {
                CreatePrisonCorridor(candidate);
                created++;
            }
        }

        Debug.Log($"Created {created} prison cell corridors with {prisonCellPositions.Count} cells total");
    }

    /// <summary>
    /// Finds potential locations for prison corridors
    /// </summary>
    List<PrisonCorridorCandidate> FindPrisonCorridorLocations()
    {
        List<PrisonCorridorCandidate> candidates = new List<PrisonCorridorCandidate>();

        // Scan maze for straight corridors
        for (int x = 10; x < SizeOfDungeon - 15; x += 3)
        {
            for (int z = 10; z < SizeOfDungeon - 15; z += 3)
            {
                // Try horizontal corridor
                if (CanStartHorizontalPrisonCorridor(x, z))
                {
                    float score = CalculateDistributionScore(x, z);
                    candidates.Add(new PrisonCorridorCandidate(
                        new Vector2Int(x, z),
                        true,
                        score
                    ));
                }

                // Try vertical corridor
                if (CanStartVerticalPrisonCorridor(x, z))
                {
                    float score = CalculateDistributionScore(x, z);
                    candidates.Add(new PrisonCorridorCandidate(
                        new Vector2Int(x, z),
                        false,
                        score
                    ));
                }
            }
        }

        return candidates;
    }

    /// <summary>
    /// Calculates how well distributed this position is
    /// </summary>
    float CalculateDistributionScore(int x, int z)
    {
        // Distance from entrance (prefer farther)
        float distFromEntrance = Vector2Int.Distance(new Vector2Int(x, z), new Vector2Int(5, 5));

        // Distance from existing corridors (prefer spread out)
        float minDistFromExisting = float.MaxValue;
        foreach (var existing in prisonCellPositions)
        {
            float dist = Vector2Int.Distance(new Vector2Int(x, z), existing);
            if (dist < minDistFromExisting)
                minDistFromExisting = dist;
        }

        return distFromEntrance + minDistFromExisting;
    }

    /// <summary>
    /// Checks if we can place a prison corridor
    /// </summary>
    bool CanPlacePrisonCorridor(PrisonCorridorCandidate candidate)
    {
        if (candidate.isHorizontal)
            return CanStartHorizontalPrisonCorridor(candidate.position.x, candidate.position.y);
        else
            return CanStartVerticalPrisonCorridor(candidate.position.x, candidate.position.y);
    }

    /// <summary>
    /// Checks if a horizontal prison corridor can be created
    /// </summary>
    bool CanStartHorizontalPrisonCorridor(int startX, int z)
    {
        // Check if we have space for 11-cell corridor + cells on sides
        for (int i = 0; i < 11; i++)
        {
            int x = startX + i;

            // Check corridor path is open or can be made open
            if (!IsInBounds(x, z))
                return false;

            // Check space above and below for cells
            if (!IsInBounds(x, z - 2) || !IsInBounds(x, z + 2))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Checks if a vertical prison corridor can be created
    /// </summary>
    bool CanStartVerticalPrisonCorridor(int x, int startZ)
    {
        // Check if we have space for 11-cell corridor + cells on sides
        for (int i = 0; i < 11; i++)
        {
            int z = startZ + i;

            // Check corridor path is open or can be made open
            if (!IsInBounds(x, z))
                return false;

            // Check space left and right for cells
            if (!IsInBounds(x - 2, z) || !IsInBounds(x + 2, z))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Creates a horizontal or vertical prison corridor
    /// </summary>
    void CreatePrisonCorridor(PrisonCorridorCandidate candidate)
    {
        if (candidate.isHorizontal)
            CreateHorizontalPrisonCorridor(candidate.position.x, candidate.position.y);
        else
            CreateVerticalPrisonCorridor(candidate.position.x, candidate.position.y);
    }

    /// <summary>
    /// Creates a horizontal prison corridor with cells
    /// </summary>
    void CreateHorizontalPrisonCorridor(int startX, int z)
    {
        // Create main corridor (11 cells)
        for (int i = 0; i < 11; i++)
        {
            int x = startX + i;
            mazeData[x, z] = false;

            // Every second position (odd indices): create cells
            if (i % 2 == 1)
            {
                // Create cell on north side
                CreatePrisonCell(x, z + 1, true);  // North

                // Create cell on south side
                CreatePrisonCell(x, z - 1, false); // South
            }
        }
    }

    /// <summary>
    /// Creates a vertical prison corridor with cells
    /// </summary>
    void CreateVerticalPrisonCorridor(int x, int startZ)
    {
        // Create main corridor (11 cells)
        for (int i = 0; i < 11; i++)
        {
            int z = startZ + i;
            mazeData[x, z] = false;

            // Every second position (odd indices): create cells
            if (i % 2 == 1)
            {
                // Create cell on east side
                CreatePrisonCell(x + 1, z, true);  // East

                // Create cell on west side
                CreatePrisonCell(x - 1, z, false); // West
            }
        }
    }

    /// <summary>
    /// Creates a single prison cell (1 cell opening, 3 walls)
    /// </summary>
    void CreatePrisonCell(int x, int z, bool isNorthOrEast)
    {
        if (!IsInBounds(x, z))
            return;

        // Clear the cell floor
        mazeData[x, z] = false;

        // Track this as a prison cell position
        prisonCellPositions.Add(new Vector2Int(x, z));

        // Create back wall and side walls based on orientation
        // The opening is towards the corridor
        // We don't actually need to create walls - they stay as walls by default
        // Just ensure the cell is accessible from corridor
    }

    /// <summary>
    /// Checks if coordinates are within maze bounds
    /// </summary>
    bool IsInBounds(int x, int z)
    {
        return x > 0 && x < SizeOfDungeon - 1 && z > 0 && z < SizeOfDungeon - 1;
    }

    void BuildMazeInScene()
    {
        for (int x = 0; x < SizeOfDungeon; x++)
        {
            for (int z = 0; z < SizeOfDungeon; z++)
            {
                Vector3 position = new Vector3(x * cellSize, 0, z * cellSize);

                // Always create floor and ceiling
                if (floorPrefab != null)
                {
                    GameObject floor = Instantiate(floorPrefab, position, Quaternion.identity, transform);
                    floor.name = $"Floor_{x}_{z}";
                }

                if (ceilingPrefab != null)
                {
                    Vector3 ceilingPos = position + Vector3.up * cellSize;
                    GameObject ceiling = Instantiate(ceilingPrefab, ceilingPos, Quaternion.identity, transform);
                    ceiling.name = $"Ceiling_{x}_{z}";
                }

                // Create wall if this cell is a wall
                if (mazeData[x, z] && wallPrefabs.Count > 0)
                {
                    GameObject wall = Instantiate(wallPrefabs[Random.Range(0, wallPrefabs.Count)], position, Quaternion.identity, transform);
                    wall.name = $"Wall_{x}_{z}";
                }
            }
        }
    }

    public Vector2Int GetStartPosition()
    {
        // Return center of entrance hall (3x3 room)
        return new Vector2Int(5, 5);
    }

    public bool IsCellWalkable(int x, int z)
    {
        if (x < 0 || x >= SizeOfDungeon || z < 0 || z >= SizeOfDungeon)
            return false;

        return !mazeData[x, z];
    }
}

/// <summary>
/// Represents a potential location for a prison corridor
/// </summary>
class PrisonCorridorCandidate
{
    public Vector2Int position;
    public bool isHorizontal;
    public float score;

    public PrisonCorridorCandidate(Vector2Int pos, bool horizontal, float distributionScore)
    {
        position = pos;
        isHorizontal = horizontal;
        score = distributionScore;
    }
}