using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEditor.Experimental.GraphView;

/// <summary>
/// Button for selecting a character to receive prisoner training
/// </summary>
public class CharacterSelectButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Components")] 
    public TextMeshProUGUI playerText;

    private int characterNumber;
    private PlayerCharacter character;
    private PrisonerData prisoner;
    private System.Action onClickCallback;

    private bool isSelectable = false;


    /// <summary>
    /// Initializes the button with character data
    /// </summary>
    public void Initialize(int number, PlayerCharacter playerChar, PrisonerData prisonerData, System.Action onClick)
    {
        characterNumber = number;
        character = playerChar;
        prisoner = prisonerData;
        onClickCallback = onClick;
        isSelectable = true;

        UpdateDisplay(); 
    }

    /// <summary>
    /// Updates the button display text
    /// </summary>
    void UpdateDisplay()
    {
        if (character == null || prisoner == null || playerText == null)
            return;

        // Get current attribute value
        int currentValue = GetCurrentAttributeValue();

        // Calculate potential upgrade
        int upgradeAmount = prisoner.CalculateUpgradeValue(currentValue);
        int newValue = currentValue + upgradeAmount;

        // Format display
        string skillName = prisoner.GetSkillName();
        playerText.text = $"{characterNumber}. {character.characterName} " +
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

    void Update()
    {
        // Block input when menu is open
        if (MainMenuManager.IsGamePaused())
            return;

        // Handle number key input
        if (isSelectable && characterNumber >= 1 && characterNumber <= 8)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + (characterNumber - 1)))
            {
                OnPointerClick(null);
            }
        }
    }
    public void SetSelectable(bool selectable)
    {
        isSelectable = selectable;
        if (!selectable && playerText != null)
        {
            playerText.color = GameUIManager.Instance.normalText;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isSelectable )
        {
            onClickCallback.Invoke();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isSelectable && playerText != null)
        {
            playerText.color = GameUIManager.Instance.positiveText;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (playerText != null)
        {
            playerText.color = GameUIManager.Instance.normalText;
        }
    }
}