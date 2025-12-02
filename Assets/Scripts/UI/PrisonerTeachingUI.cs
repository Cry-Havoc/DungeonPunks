using UnityEngine;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Manages the UI for selecting which character receives training from a prisoner
/// </summary>
public class PrisonerTeachingUI : MonoBehaviour
{
    public static PrisonerTeachingUI Instance { get; private set; }

    [Header("UI References")]
    public GameObject characterSelectionPanel;
    public Transform characterButtonContainer;
    public GameObject characterButtonPrefab;
    public TextMeshProUGUI instructionText;

    [Header("Current Teaching Session")]
    private PrisonerData currentPrisoner;
    private PlayerCharacter[] partyMembers;
    private List<GameObject> activeButtons = new List<GameObject>();
    private System.Action onTeachingComplete;

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

        if (characterSelectionPanel != null)
        {
            characterSelectionPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Shows the character selection UI for teaching
    /// </summary>
    public void ShowTeachingSelection(PrisonerData prisoner, PlayerCharacter[] party, System.Action onComplete)
    {
        currentPrisoner = prisoner;
        partyMembers = party;
        onTeachingComplete = onComplete;

        // Clear existing buttons
        ClearButtons();

        // Show panel
        if (characterSelectionPanel != null)
        {
            characterSelectionPanel.SetActive(true);
        }

        // Update instruction text
        if (instructionText != null)
        {
            instructionText.text = $"Select a character to receive {prisoner.GetSkillName()} training from {prisoner.prisonerName}:";
        }

        // Create character buttons
        CreateCharacterButtons();
    }

    /// <summary>
    /// Creates clickable buttons for each character
    /// </summary>
    void CreateCharacterButtons()
    {
        for (int i = 0; i < partyMembers.Length; i++)
        {
            if (partyMembers[i] == null) continue;

            GameObject buttonObj = Instantiate(characterButtonPrefab, characterButtonContainer);
            activeButtons.Add(buttonObj);

            // Get button component
            CharacterSelectButton buttonScript = buttonObj.GetComponent<CharacterSelectButton>();
            if (buttonScript != null)
            {
                int characterIndex = i; // Capture for lambda
                buttonScript.Initialize(
                    i + 1, // Number (1-based)
                    partyMembers[i],
                    currentPrisoner,
                    () => OnCharacterSelected(characterIndex)
                );
            }
        }
    }

    /// <summary>
    /// Handles character selection
    /// </summary>
    void OnCharacterSelected(int characterIndex)
    {
        if (characterIndex < 0 || characterIndex >= partyMembers.Length)
            return;

        PlayerCharacter selectedCharacter = partyMembers[characterIndex];
        if (selectedCharacter == null) return;

        // Get current attribute value
        int currentValue = GetCurrentAttributeValue(selectedCharacter, currentPrisoner.skillToTeach);

        // Calculate upgrade with diminishing returns
        int upgradeAmount = currentPrisoner.CalculateUpgradeValue(currentValue);

        // Apply upgrade
        ApplyAttributeUpgrade(selectedCharacter, currentPrisoner.skillToTeach, upgradeAmount);

        // Show result message
        ShowUpgradeResult(selectedCharacter, currentValue, upgradeAmount);

        // Hide UI
        HideTeachingUI();

        // Complete callback
        onTeachingComplete?.Invoke();
    }

    /// <summary>
    /// Gets the current value of the specified attribute
    /// </summary>
    int GetCurrentAttributeValue(PlayerCharacter character, PlayerAttribute skill)
    {
        switch (skill)
        {
            case PlayerAttribute.FORCE:
                return character.force;
            case PlayerAttribute.REFLEXE:
                return character.reflexe;
            case PlayerAttribute.REASON:
                return character.reason;
            case PlayerAttribute.STAMINA:
                return character.stamina;
            case PlayerAttribute.HEART:
                return character.heart;
            case PlayerAttribute.PERCEPTION:
                return character.perception;
            case PlayerAttribute.WILLPOWER:
                return character.willPower;
            default:
                return 0;
        }
    }

    /// <summary>
    /// Applies the attribute upgrade to the character
    /// </summary>
    void ApplyAttributeUpgrade(PlayerCharacter character, PlayerAttribute skill, int amount)
    {
        switch (skill)
        {
            case PlayerAttribute.FORCE:
                character.force += amount;
                break;
            case PlayerAttribute.REFLEXE:
                character.reflexe += amount;
                break;
            case PlayerAttribute.HEART:
                character.heart += amount;
                break;
            case PlayerAttribute.STAMINA:
                character.stamina += amount;
                break;
            case PlayerAttribute.REASON:
                character.reason += amount;
                break;
            case PlayerAttribute.WILLPOWER:
                character.willPower += amount;
                break;
            case PlayerAttribute.PERCEPTION:
                character.perception += amount;
                break;
        }

        Debug.Log($"{character.characterName} gained +{amount} {skill}");
    }

    /// <summary>
    /// Shows the upgrade result message
    /// </summary>
    void ShowUpgradeResult(PlayerCharacter character, int oldValue, int upgradeAmount)
    {
        if (GameUIManager.Instance == null || GameUIManager.Instance.encounterText == null)
            return;

        int newValue = oldValue + upgradeAmount;
        string skillName = currentPrisoner.GetSkillName();

        GameUIManager.Instance.encounterText.text = 
            $"{character.characterName} trained with {currentPrisoner.prisonerName}!\n\n" +
            $"{skillName}: {oldValue} → {newValue} (+{upgradeAmount})\n\n" +
            $"{currentPrisoner.prisonerName} thanks you and leaves for the exit.\n\n" +
            $"({PrisonerSystem.Instance.RescuedCount}/{PrisonerSystem.Instance.TotalPrisoners} rescued)\n\n" +
            "Press <u>Space</u> to continue...";
    }

    /// <summary>
    /// Hides the teaching UI
    /// </summary>
    void HideTeachingUI()
    {
        if (characterSelectionPanel != null)
        {
            characterSelectionPanel.SetActive(false);
        }

        ClearButtons();
    }

    /// <summary>
    /// Clears all character buttons
    /// </summary>
    void ClearButtons()
    {
        foreach (GameObject button in activeButtons)
        {
            if (button != null)
            {
                Destroy(button);
            }
        }
        activeButtons.Clear();
    }

    /// <summary>
    /// Handles keyboard input for character selection
    /// </summary>
    void Update()
    {
        // Block input when menu is open
        if (MainMenuManager.IsGamePaused())
            return;

        if (!characterSelectionPanel.activeSelf) return;

        // Numbers 1-9 for direct selection
        for (int i = 0; i < 9 && i < partyMembers.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                OnCharacterSelected(i);
                break;
            }
        }
    }
}