using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUIManager : MonoBehaviour
{
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
    public PlayerPartyController partyController;

    [Header("Party Members")]
    public PlayerCharacter[] partyMembers = new PlayerCharacter[6];

    private RenderTexture dungeonRenderTexture;
    private PlayerFrameUI[] playerFrameUIs = new PlayerFrameUI[6];
    private int selectedCharacterIndex = -1;
    private bool isCharacterMode = false;

    void Start()
    {
        SetupDungeonCamera();
        SpawnPlayerFrames();
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

    void SetDungeonMode()
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
        if (partyController != null)
        {
            partyController.enabled = true;
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
        if (partyController != null)
        {
            partyController.enabled = false;
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