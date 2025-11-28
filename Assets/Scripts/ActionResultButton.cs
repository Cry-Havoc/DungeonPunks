using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ActionResultButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI buttonText;
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;

    private int resultNumber;
    private PlayerActionResult actionResult;
    private bool isSelectable = false;
    private System.Action<PlayerActionResult> onSelected;

    void Update()
    {
        // Handle number key input
        if (isSelectable && resultNumber >= 1 && resultNumber <= 10)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + (resultNumber - 1)) ||
                Input.GetKeyDown(KeyCode.Keypad1 + (resultNumber - 1)))
            {
                OnPointerClick(null);
            }
        }
    }

    public void InitializeResultButton(int number, PlayerActionResult result, System.Action<PlayerActionResult> callback)
    {
        resultNumber = number;
        actionResult = result;
        onSelected = callback;

        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if (buttonText != null && actionResult != null)
        {
            buttonText.text = $"{resultNumber}. {actionResult.buttonText} ( {actionResult.description} )";
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
        if (isSelectable && actionResult != null)
        {
            onSelected?.Invoke(actionResult);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isSelectable && buttonText != null)
        {
            buttonText.color = hoverColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (buttonText != null)
        {
            buttonText.color = normalColor;
        }
    }
}