using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ActionButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI buttonText;
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;

    private int actionNumber;
    private PlayerAction action;
    private PlayerCharacter character;
    private bool isSelectable = false;
    private System.Action<PlayerAction> onSelected;

    private string normalTextContent;
    private string hoverTextContent;

    void Update()
    {
        // Handle number key input
        if (isSelectable && actionNumber >= 1 && actionNumber <= 10)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + (actionNumber - 1)) ||
                Input.GetKeyDown(KeyCode.Keypad1 + (actionNumber - 1)))
            {
                OnPointerClick(null);
            }
        }
    }

    public void Initialize(int number, PlayerAction playerAction, PlayerCharacter playerChar, System.Action<PlayerAction> callback)
    {
        actionNumber = number;
        action = playerAction;
        character = playerChar;
        onSelected = callback;

        BuildText();
        UpdateDisplay();
    }

    void BuildText()
    {
        // Get attribute values
        int successValue = action.GetAttributeValue(character, action.successCheckAttribute);
        int criticalValue = action.GetAttributeValue(character, action.criticalCheckAttribute);
        int fumbleValue = action.GetAttributeValue(character, action.fumbleCheckAttribute);

        // Build normal text with attribute names
        string successAttr = FormatAttribute(action.successCheckAttribute, successValue, false);
        string criticalAttr = FormatAttribute(action.criticalCheckAttribute, criticalValue, false);
        string fumbleAttr = FormatAttribute(action.fumbleCheckAttribute, fumbleValue, false);

        normalTextContent = $"{actionNumber}. {action.buttonText} ( {successAttr}, {criticalAttr}, {fumbleAttr} )";

        // Build hover text with attribute values
        string successAttrHover = FormatAttribute(action.successCheckAttribute, successValue, true);
        string criticalAttrHover = FormatAttribute(action.criticalCheckAttribute, criticalValue, true);
        string fumbleAttrHover = FormatAttribute(action.fumbleCheckAttribute, fumbleValue, true);

        hoverTextContent = $"{actionNumber}. {action.buttonText} ( {successAttrHover}, {criticalAttrHover}, {fumbleAttrHover} )";
    }

    string FormatAttribute(PlayerAttribute attribute, int value, bool showValue)
    {
        string text = showValue ? value.ToString() : attribute.ToString();

        // Color based on value
        Color color = Color.white;
        if (value >= 70)
        {
            color = Color.blue;
        }
        else if (value <= 30)
        {
            color = Color.red;
        }

        return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{text}</color>";
    }

    void UpdateDisplay()
    {
        if (buttonText != null)
        {
            buttonText.text = normalTextContent;
        }
    }

    public void SetSelectable(bool selectable)
    {
        isSelectable = selectable;
        if (!selectable && buttonText != null)
        {
            buttonText.color = normalColor;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isSelectable && action != null)
        {
            onSelected?.Invoke(action);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isSelectable && buttonText != null)
        {
            buttonText.color = hoverColor;
            buttonText.text = hoverTextContent; // Show values
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (buttonText != null)
        {
            buttonText.color = normalColor;
            buttonText.text = normalTextContent; // Show names
        }
    }
}