using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }

    [Header("Main UI Elements")]
    public RawImage dungeonFrame;
    public Image encounterFrame;
    public TextMeshProUGUI encounterText;

    [Header("Player Frame Setup")]
    public GameObject playerFramePrefab;
    public Transform topRowContainer;
    public Transform bottomRowContainer;

    [Header("References")]
    public Camera dungeonCamera; 

    [Header("Party Members")]
    public PlayerCharacter[] partyMembers = new PlayerCharacter[6];

    private RenderTexture dungeonRenderTexture;
    private PlayerFrameUI[] playerFrameUIs = new PlayerFrameUI[6];
    private int selectedCharacterIndex = -1;
    private bool isCharacterMode = false;

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
        SetupDungeonCamera();
        SpawnPlayerFrames();
          
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.Initialize(partyMembers);
        }

        SetDungeonMode();
    }

    void Update()
    {
        HandleModeInput();
    }

    void SetupDungeonCamera()
    {
        if (dungeonCamera != null && dungeonFrame != null)
        {
            // Create render texture for the camera
            dungeonRenderTexture = new RenderTexture(512, 512, 24);
            dungeonCamera.targetTexture = dungeonRenderTexture;
            dungeonFrame.texture = dungeonRenderTexture;
        }
    }

    void SpawnPlayerFrames()
    {
        if (playerFramePrefab == null || topRowContainer == null || bottomRowContainer == null)
        {
            Debug.LogError("PlayerFramePrefab or Row Containers not assigned!");
            return;
        }

        for (int i = 0; i < 6; i++)
        {
            // First 3 characters go in top row, last 3 in bottom row
            Transform parentContainer = (i < 3) ? topRowContainer : bottomRowContainer;

            GameObject frameObj = Instantiate(playerFramePrefab, parentContainer);
            PlayerFrameUI frameUI = frameObj.GetComponent<PlayerFrameUI>();

            if (frameUI != null && partyMembers[i] != null)
            {
                frameUI.Initialize(partyMembers[i], this, i);
                playerFrameUIs[i] = frameUI;
            }
        }
    }

    void HandleModeInput()
    {
        // Don't handle mode input during combat
        if (CombatManager.Instance != null && CombatManager.Instance.IsInCombat)
            return;

        // Number keys to select characters
        if (!isCharacterMode)
        {
            for (int i = 0; i < 6; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    SelectCharacter(i);
                }
            }
        }

        // Escape or Space to return to dungeon mode
        if (isCharacterMode && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space)))
        {
            SetDungeonMode();
        }
    }

    public void SelectCharacter(int index)
    {
        if (index < 0 || index >= 6 || partyMembers[index] == null)
            return;

        selectedCharacterIndex = index;
        SetCharacterMode();
    }

    public void SetDungeonMode()
    {
        isCharacterMode = false;

        // Enable dungeon camera rendering
        if (dungeonFrame != null)
        {
            dungeonFrame.gameObject.SetActive(true);
            dungeonFrame.texture = dungeonRenderTexture;
        }

        // Clear encounter frame
        if (encounterFrame != null)
        {
            encounterFrame.gameObject.SetActive(false);
        }
        if (encounterText != null)
        {
            encounterText.text = "";
        }

        // Enable party controller
        if (PlayerPartyController.Instance != null)
        {
            PlayerPartyController.Instance.enabled = true;
        }

        // Deselect all character frames
        for (int i = 0; i < playerFrameUIs.Length; i++)
        {
            if (playerFrameUIs[i] != null)
            {
                playerFrameUIs[i].SetSelected(false);
            }
        }
    }

    public void SetCombatMode()
    {
        isCharacterMode = false;

        // Disable dungeon rendering
        if (dungeonFrame != null)
        {
            dungeonFrame.gameObject.SetActive(false);
        }

        // Enable encounter frame
        if (encounterFrame != null)
        {
            encounterFrame.gameObject.SetActive(true);
        }

        // Disable party controller (already disabled in CombatManager but just in case)
        if (PlayerPartyController.Instance != null)
        {
            PlayerPartyController.Instance.enabled = false;
        }
    }

    void SetCharacterMode()
    {
        isCharacterMode = true;

        // Disable dungeon camera rendering
        if (dungeonFrame != null)
        {
            dungeonFrame.gameObject.SetActive(false);
        }

        // Show character details in encounter frame
        if (encounterFrame != null && partyMembers[selectedCharacterIndex] != null)
        {
            encounterFrame.gameObject.SetActive(true);
            encounterFrame.sprite = partyMembers[selectedCharacterIndex].characterPicture;
        }

        if (encounterText != null && partyMembers[selectedCharacterIndex] != null)
        {
            encounterText.text = partyMembers[selectedCharacterIndex].GetStatsText();
        }

        // Disable party controller
        if (PlayerPartyController.Instance != null)
        {
            PlayerPartyController.Instance.enabled = false;
        }

        // Highlight selected character
        for (int i = 0; i < playerFrameUIs.Length; i++)
        {
            if (playerFrameUIs[i] != null)
            {
                playerFrameUIs[i].SetSelected(i == selectedCharacterIndex);
            }
        }
    }

    void OnDestroy()
    {
        if (dungeonRenderTexture != null)
        {
            dungeonRenderTexture.Release();
        }
    }
}