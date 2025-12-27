using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages lighting for prisoner locations and entrance
/// </summary>
public class PrisonerLightingSystem : MonoBehaviour
{
    public static PrisonerLightingSystem Instance { get; private set; }

    [Header("Light Prefab")]
    [Tooltip("Prefab with Light component for spawning")]
    public GameObject lightPrefab;

    [Header("Prisoner Light Settings")]
    [Tooltip("Color of prisoner lights")]
    public Color prisonerLightColor = new Color(1f, 0.5f, 0f); // Orange
    
    [Tooltip("Intensity of prisoner lights")]
    [Range(0.5f, 5f)]
    public float prisonerLightIntensity = 2f;
    
    [Tooltip("Range of prisoner lights")]
    [Range(2f, 15f)]
    public float prisonerLightRange = 8f;

    [Header("Entrance Light Settings")]
    [Tooltip("Color of entrance light")]
    public Color entranceLightColor = new Color(0f, 0.5f, 1f); // Blue
    
    [Tooltip("Intensity of entrance light")]
    [Range(0.5f, 5f)]
    public float entranceLightIntensity = 3f;
    
    [Tooltip("Range of entrance light")]
    [Range(2f, 15f)]
    public float entranceLightRange = 10f;

    [Header("Flicker Settings")]
    [Tooltip("Minimum intensity multiplier for flicker")]
    [Range(0.5f, 0.95f)]
    public float minFlickerIntensity = 0.8f;
    
    [Tooltip("Maximum intensity multiplier for flicker")]
    [Range(1.05f, 1.5f)]
    public float maxFlickerIntensity = 1.2f;
    
    [Tooltip("How often flicker changes (higher = faster)")]
    [Range(1f, 20f)]
    public float flickerSpeed = 10f;
    
    [Tooltip("Smoothing for flicker transitions")]
    [Range(0.05f, 0.5f)]
    public float flickerSmoothing = 0.1f;

    private GameObject entranceLight;
    private Dictionary<Vector2Int, GameObject> prisonerLights = new Dictionary<Vector2Int, GameObject>();

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
    /// Initializes all lights for prisoners and entrance
    /// </summary>
    public void InitializeLights(Vector2Int entrancePosition, List<PrisonerData> prisoners, float cellSize)
    {
        // Create entrance light
        CreateEntranceLight(entrancePosition, cellSize);

        // Create prisoner lights
        foreach (var prisoner in prisoners)
        {
            if (!prisoner.hasBeenRescued)
            {
                CreatePrisonerLight(prisoner.gridPosition, cellSize);
            }
        }

        Debug.Log($"Lighting System: Created entrance light + {prisonerLights.Count} prisoner lights");
    }

    /// <summary>
    /// Creates the entrance light
    /// </summary>
    void CreateEntranceLight(Vector2Int position, float cellSize)
    {
        Vector3 worldPos = GridToWorldPosition(position, cellSize);
        worldPos.y = cellSize * 0.5f; // Middle height of cell

        if (lightPrefab != null)
        {
            entranceLight = Instantiate(lightPrefab, worldPos, Quaternion.identity, transform);
            entranceLight.name = "Entrance_Light";

            Light lightComponent = entranceLight.GetComponentInChildren<Light>();
            if (lightComponent != null)
            { 
                lightComponent.color = entranceLightColor;
                lightComponent.intensity = entranceLightIntensity;
                lightComponent.range = entranceLightRange;
            }

            // Add flicker component
            FlickeringLight flickerScript = entranceLight.GetComponentInChildren<FlickeringLight>();
            if (flickerScript == null)
            {
                flickerScript = entranceLight.AddComponent<FlickeringLight>();
            }
            
            flickerScript.Initialize(
                entranceLightIntensity,
                minFlickerIntensity,
                maxFlickerIntensity,
                flickerSpeed,
                flickerSmoothing
            );
        }
        else
        {
            Debug.LogError("Light prefab not assigned!");
        }
    }

    /// <summary>
    /// Creates a light at a prisoner location
    /// </summary>
    void CreatePrisonerLight(Vector2Int position, float cellSize)
    {
        Vector3 worldPos = GridToWorldPosition(position, cellSize);
        worldPos.y = cellSize * 0.5f; // Middle height of cell

        if (lightPrefab != null)
        {
            GameObject prisonerLight = Instantiate(lightPrefab, worldPos, Quaternion.identity, transform);
            prisonerLight.name = $"Prisoner_Light_{position.x}_{position.y}";

            Light lightComponent = prisonerLight.GetComponentInChildren<Light>();
            if (lightComponent != null)
            { 
                lightComponent.color = prisonerLightColor;
                lightComponent.intensity = prisonerLightIntensity;
                lightComponent.range = prisonerLightRange;
            }

            // Add flicker component
            FlickeringLight flickerScript = prisonerLight.GetComponentInChildren<FlickeringLight>();
            if (flickerScript == null)
            {
                flickerScript = prisonerLight.GetComponentInChildren<FlickeringLight>();
            }
            
            flickerScript.Initialize(
                prisonerLightIntensity,
                minFlickerIntensity,
                maxFlickerIntensity,
                flickerSpeed,
                flickerSmoothing
            );

            prisonerLights.Add(position, prisonerLight);
        }
    }

    /// <summary>
    /// Disables light at a prisoner location when they're rescued
    /// </summary>
    public void DisablePrisonerLight(Vector2Int position)
    {
        if (prisonerLights.ContainsKey(position))
        {
            GameObject light = prisonerLights[position];
            if (light != null)
            {
                light.SetActive(false);
                Debug.Log($"Disabled prisoner light at {position}");
            }
            prisonerLights.Remove(position);
        }
    }

    /// <summary>
    /// Converts grid position to world position
    /// </summary>
    Vector3 GridToWorldPosition(Vector2Int gridPos, float cellSize)
    {
        return new Vector3(gridPos.x * cellSize, 0, gridPos.y * cellSize);
    }

    /// <summary>
    /// Cleans up all lights
    /// </summary>
    public void ClearAllLights()
    {
        if (entranceLight != null)
        {
            Destroy(entranceLight);
            entranceLight = null;
        }

        foreach (var light in prisonerLights.Values)
        {
            if (light != null)
            {
                Destroy(light);
            }
        }
        prisonerLights.Clear();

        Debug.Log("All lights cleared");
    }

    void OnDestroy()
    {
        ClearAllLights();
    }
}