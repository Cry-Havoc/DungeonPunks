using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class PlayerFrameUI : MonoBehaviour, IPointerClickHandler
{
    [Header("UI References")]
    public Image characterImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI healthText;

    [Header("Settings")]
    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;

    private PlayerCharacter character;
    private GameUIManager uiManager;
    private int characterIndex;

    public void Initialize(PlayerCharacter playerChar, GameUIManager manager, int index)
    {
        character = playerChar;
        uiManager = manager;
        characterIndex = index;

        // Subscribe to stat changes
        character.OnStatsChanged += UpdateUI;

        UpdateUI();
    }

    void OnDestroy()
    {
        if (character != null)
        {
            character.OnStatsChanged -= UpdateUI;
        }
    }

    public void UpdateUI()
    {
        if (character == null) return;

        // Set character image
        if (characterImage != null && character.characterPicture != null)
        {
            characterImage.sprite = character.characterPicture;
        }

        // Set name
        if (nameText != null)
        {
            nameText.text = character.characterName;
        }

        // Set health with box characters
        if (healthText != null)
        {
            string healthDisplay = "";
            for (int i = 0; i < character.maxHealthPoints; i++)
            {
                if (i < character.healthPoints)
                {
                    healthDisplay += "■"; // Filled box
                }
                else
                {
                    healthDisplay += "□"; // Empty box
                }
            }
            healthText.text = healthDisplay;
        }
    }

    public void SetSelected(bool selected)
    {
        if (nameText != null)
        {
            nameText.color = selected ? selectedColor : normalColor;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (uiManager != null)
        {
            uiManager.SelectCharacter(characterIndex);
        }
    }
}