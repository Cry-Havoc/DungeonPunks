using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Button for selecting a character to receive prisoner training
/// </summary>
public class CharacterSelectButton : MonoBehaviour
{
    [Header("UI Components")]
    public Button button;
    public TextMeshProUGUI buttonText;

    private int characterNumber;
    private PlayerCharacter character;
    private PrisonerData prisoner;
    private System.Action onClickCallback;

    /// <summary>
    /// Initializes the button with character data
    /// </summary>
    public void Initialize(int number, PlayerCharacter playerChar, PrisonerData prisonerData, System.Action onClick)
    {
        characterNumber = number;
        character = playerChar;
        prisoner = prisonerData;
        onClickCallback = onClick;

        UpdateDisplay();

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnButtonClick);
        }
    }

    /// <summary>
    /// Updates the button display text
    /// </summary>
    void UpdateDisplay()
    {
        if (character == null || prisoner == null || buttonText == null)
            return;

        // Get current attribute value
        int currentValue = GetCurrentAttributeValue();

        // Calculate potential upgrade
        int upgradeAmount = prisoner.CalculateUpgradeValue(currentValue);
        int newValue = currentValue + upgradeAmount;

        // Format display
        string skillName = prisoner.GetSkillName();
        buttonText.text = $"{characterNumber}. {character.characterName}\n" +
                         $"{skillName}: {currentValue} → {newValue} (+{upgradeAmount})";
    }

    /// <summary>
    /// Gets current attribute value based on prisoner's skill
    /// </summary>
    int GetCurrentAttributeValue()
    {
        switch (prisoner.skillToTeach)
        {
            case PlayerAttribute.FORCE:
                return character.force;
            case PlayerAttribute.PERCEPTION:
                return character.perception;
            case PlayerAttribute.REFLEXE:
                return character.reflexe;
            case PlayerAttribute.STAMINA:
                return character.stamina;
            case PlayerAttribute.REASON:
                return character.reason;
            case PlayerAttribute.WILLPOWER:
                return character.willPower;
            case PlayerAttribute.HEART:
                return character.heart;
            default:
                return 0;
        }
    }

    /// <summary>
    /// Handles button click
    /// </summary>
    void OnButtonClick()
    {
        onClickCallback?.Invoke();
    }
}