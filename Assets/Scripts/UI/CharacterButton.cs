using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class CharacterButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI characterText;
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;

    private int characterNumber;
    private Monster monster;
    private bool isSelectable = false;
    private int characterIndex;

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

    public void Initialize(int number, Monster m, CombatManager manager)
    {
        characterNumber = number;
        monster = m;
        characterIndex = number - 1;
        UpdateDisplay();
    }

    public void SetNumber(int number)
    {
        characterNumber = number;
        characterIndex = number - 1;
    }

    public void UpdateDisplay()
    {
        if (characterText != null && monster != null)
        {
            characterText.text = $"{characterNumber}. {monster.monsterName} {monster.GetHealthDisplay()}";
        }
    }

    public void SetSelectable(bool selectable)
    {
        isSelectable = selectable;
        if (!selectable && characterText != null)
        {
            characterText.color = normalColor;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isSelectable && CombatManager.Instance != null)
        {
            CombatManager.Instance.OnMonsterSelected(characterIndex);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isSelectable && characterText != null)
        {
            characterText.color = hoverColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (characterText != null)
        {
            characterText.color = normalColor;
        }
    }
}