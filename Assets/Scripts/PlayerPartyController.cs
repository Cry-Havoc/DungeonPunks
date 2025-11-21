using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerPartyController : MonoBehaviour
{
    public static PlayerPartyController Instance { get; private set; }

    [Header("References")]
    public MazeGenerator mazeGenerator;
    public Camera playerCamera; 

    [Header("Movement Settings")]
    public float moveSpeed = 1f; // Time in seconds to move one cell
    public float turnSpeed = 1f; // Time in seconds to turn 90 degrees

    [Header("Encounter Settings")]
    [Range(0f, 1f)] public float encounterChance = 0.05f; // 5% chance per step

    [Header("Grid Settings")]
    public float cellSize = 2f;

    // Current grid position
    private Vector2Int gridPosition;

    // Current facing direction (0=North, 1=East, 2=South, 3=West)
    private int facingDirection = 0;

    // Movement state
    private bool isMoving = false;
    private Queue<System.Action> commandQueue = new Queue<System.Action>();

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
        if (mazeGenerator == null)
        {
            mazeGenerator = MazeGenerator.Instance;
        }

        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
        } 

        // Wait a frame for maze to generate, then position player
        StartCoroutine(InitializePosition());
    }

    IEnumerator InitializePosition()
    {
        yield return null; // Wait one frame for maze generation

        gridPosition = mazeGenerator.GetStartPosition();
        Vector3 worldPos = GridToWorldPosition(gridPosition);
        transform.position = worldPos + Vector3.up * cellSize * 0.5f; // Center camera at eye level
        transform.rotation = Quaternion.Euler(0, 0, 0); // Face north
    }

    void Update()
    {
        if (mazeGenerator == null || mazeGenerator.MazeData == null)
            return;

        // Only process input if controller is enabled
        if (!enabled)
            return;

        if(CombatManager.Instance.IsInCombat) 
            return; 

        // Queue movement commands
        if (Input.GetKeyDown(KeyCode.W))
        {
            commandQueue.Enqueue(() => MoveForward());
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            commandQueue.Enqueue(() => MoveBackward());
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            commandQueue.Enqueue(() => MoveLeft());
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            commandQueue.Enqueue(() => MoveRight());
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            commandQueue.Enqueue(() => TurnLeft());
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            commandQueue.Enqueue(() => TurnRight());
        }

        // Process queued commands if not currently moving
        if (!isMoving && commandQueue.Count > 0)
        {
            System.Action nextCommand = commandQueue.Dequeue();
            nextCommand.Invoke();
        }
    }

    void MoveForward()
    {
        Vector2Int targetPos = gridPosition + GetDirectionVector(facingDirection);
        if (CanMoveTo(targetPos))
        {
            StartCoroutine(MoveToPosition(targetPos));
        }
    }

    void MoveBackward()
    {
        Vector2Int targetPos = gridPosition - GetDirectionVector(facingDirection);
        if (CanMoveTo(targetPos))
        {
            StartCoroutine(MoveToPosition(targetPos));
        }
    }

    void MoveLeft()
    {
        Vector2Int targetPos = gridPosition + GetDirectionVector((facingDirection + 3) % 4);
        if (CanMoveTo(targetPos))
        {
            StartCoroutine(MoveToPosition(targetPos));
        }
    }

    void MoveRight()
    {
        Vector2Int targetPos = gridPosition + GetDirectionVector((facingDirection + 1) % 4);
        if (CanMoveTo(targetPos))
        {
            StartCoroutine(MoveToPosition(targetPos));
        }
    }

    void TurnLeft()
    {
        StartCoroutine(RotateToAngle(-90));
    }

    void TurnRight()
    {
        StartCoroutine(RotateToAngle(90));
    }

    IEnumerator MoveToPosition(Vector2Int targetGrid)
    {
        isMoving = true;

        Vector3 startPos = transform.position;
        Vector3 targetPos = GridToWorldPosition(targetGrid) + Vector3.up * cellSize * 0.5f;

        float elapsed = 0f;
        while (elapsed < moveSpeed)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveSpeed;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        transform.position = targetPos;
        gridPosition = targetGrid;
        isMoving = false;

        // Check for random encounter
        CheckForEncounter();
    }

    IEnumerator RotateToAngle(float deltaAngle)
    {
        isMoving = true;

        Quaternion startRot = transform.rotation;
        Quaternion targetRot = startRot * Quaternion.Euler(0, deltaAngle, 0);

        // Update facing direction
        if (deltaAngle > 0)
        {
            facingDirection = (facingDirection + 1) % 4;
        }
        else
        {
            facingDirection = (facingDirection + 3) % 4;
        }

        float elapsed = 0f;
        while (elapsed < turnSpeed)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / turnSpeed;
            transform.rotation = Quaternion.Lerp(startRot, targetRot, t);
            yield return null;
        }

        transform.rotation = targetRot;
        isMoving = false;
    }

    bool CanMoveTo(Vector2Int targetPos)
    {
        return mazeGenerator.IsCellWalkable(targetPos.x, targetPos.y);
    }

    Vector2Int GetDirectionVector(int direction)
    {
        switch (direction)
        {
            case 0: return new Vector2Int(0, 1);  // North
            case 1: return new Vector2Int(1, 0);  // East
            case 2: return new Vector2Int(0, -1); // South
            case 3: return new Vector2Int(-1, 0); // West
            default: return Vector2Int.zero;
        }
    }

    Vector3 GridToWorldPosition(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x * cellSize, 0, gridPos.y * cellSize);
    }

    void CheckForEncounter()
    {
        if (CombatManager.Instance != null && Random.value < encounterChance)
        {
            CombatManager.Instance.StartRandomEncounter();
        }
    }
}