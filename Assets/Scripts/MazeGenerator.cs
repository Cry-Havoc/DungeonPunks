using UnityEngine;
using System.Collections.Generic;

public class MazeGenerator : MonoBehaviour
{
    public static MazeGenerator Instance { get; private set; }

    [Header("Maze Settings")]
    public int SizeOfDungeon = 15;

    [Header("Prefabs")]
    public GameObject floorPrefab;
    public GameObject ceilingPrefab;
    public GameObject wallPrefab;

    [Header("Cell Settings")]
    public float cellSize = 2f;

    // The abstract maze data (true = wall, false = empty)
    private bool[,] mazeData;

    public bool[,] MazeData => mazeData;

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

        // Generate maze using recursive backtracking
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        Vector2Int start = new Vector2Int(1, 1);
        mazeData[start.x, start.y] = false;
        stack.Push(start);

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
                if (mazeData[x, z] && wallPrefab != null)
                {
                    GameObject wall = Instantiate(wallPrefab, position, Quaternion.identity, transform);
                    wall.name = $"Wall_{x}_{z}";
                }
            }
        }
    }

    public Vector2Int GetStartPosition()
    {
        // Find the first free cell (usually 1,1 after generation)
        for (int x = 1; x < SizeOfDungeon - 1; x++)
        {
            for (int z = 1; z < SizeOfDungeon - 1; z++)
            {
                if (!mazeData[x, z])
                {
                    return new Vector2Int(x, z);
                }
            }
        }
        return new Vector2Int(1, 1);
    }

    public bool IsCellWalkable(int x, int z)
    {
        if (x < 0 || x >= SizeOfDungeon || z < 0 || z >= SizeOfDungeon)
            return false;

        return !mazeData[x, z];
    }
}