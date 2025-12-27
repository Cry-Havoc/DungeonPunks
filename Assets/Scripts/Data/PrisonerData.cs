using UnityEngine;

/// <summary>
/// Data for a single prisoner that can be rescued
/// </summary>
[System.Serializable]
public class PrisonerData
{
    [Header("Prisoner Info")]
    public string prisonerName;
    public string prisonerType; // "Warrior", "Mage", "Thief", etc.

    [Header("Skill Teaching")]
    [Tooltip("The attribute this prisoner can upgrade")]
    public PlayerAttribute skillToTeach;

    [Tooltip("Base upgrade value before diminishing returns")]
    [Range(10, 100)]
    public int baseUpgradeValue = 50;

    [Header("Position")]
    public Vector2Int gridPosition;
    public bool hasBeenRescued = false;

    public PrisonerData(string name, string type, Vector2Int position)
    {
        prisonerName = name;
        prisonerType = type;
        gridPosition = position;
        hasBeenRescued = false;

        // Assign random skill based on type
        AssignSkillBasedOnType(type);
    }

    /// <summary>
    /// Assigns appropriate skill based on prisoner type
    /// </summary>
    void AssignSkillBasedOnType(string type)
    {
        switch (type.ToLower())
        {
            case "warrior":
                skillToTeach = PlayerAttribute.FORCE;
                baseUpgradeValue = 50;
                break; 
            case "thief":
                skillToTeach = PlayerAttribute.REFLEXE;
                baseUpgradeValue = 50;
                break;
            case "cleric":
                skillToTeach = PlayerAttribute.WILLPOWER;
                baseUpgradeValue = 45;
                break;
            case "ranger":
                skillToTeach = PlayerAttribute.PERCEPTION;
                baseUpgradeValue = 45;
                break;
            case "paladin":
                skillToTeach = PlayerAttribute.STAMINA;
                baseUpgradeValue = 45;
                break;
            case "bard":
                skillToTeach = PlayerAttribute.HEART;
                baseUpgradeValue = 40;
                break;
            case "monk":
                skillToTeach = PlayerAttribute.STAMINA;
                baseUpgradeValue = 55;
                break;
            case "druid":
                skillToTeach = PlayerAttribute.WILLPOWER;
                baseUpgradeValue = 45;
                break;
            case "warlock":
                skillToTeach = PlayerAttribute.HEART;
                baseUpgradeValue = 55;
                break;
            case "barbarian":
                skillToTeach = PlayerAttribute.FORCE;
                baseUpgradeValue = 55;
                break;
            case "beggar":
                skillToTeach = PlayerAttribute.PERCEPTION;
                baseUpgradeValue = 45;
                break;
            case "dancer":
                skillToTeach = PlayerAttribute.REFLEXE;
                baseUpgradeValue = 55;
                break;
            default:
                skillToTeach = PlayerAttribute.FORCE;
                baseUpgradeValue = 40;
                break;
        }
    }

    /// <summary>
    /// Gets the skill name in readable format
    /// </summary>
    public string GetSkillName()
    {
        return GameUIManager.ToDisplayName(skillToTeach);
    }

    /// <summary>
    /// Calculates actual upgrade value with diminishing returns
    /// </summary>
    public int CalculateUpgradeValue(int currentAttributeValue)
    {
        // Calculate percentage reduction based on current value
        float reductionPercent = currentAttributeValue / 100f;

        // Apply diminishing returns
        float actualUpgrade = baseUpgradeValue * (1f - reductionPercent);

        // Round and ensure minimum of 1
        return Mathf.Max(1, Mathf.RoundToInt(actualUpgrade));
    }

    /// <summary>
    /// Gets the initial rescue message
    /// </summary>
    public string GetRescueMessage()
    {
        return $"You found your old friend {prisonerName}!\n\n" +
               $"Freed from their shackles, the {prisonerType} \n\n" +
               $"tells you of their hard times in the dungeon,\n\n" +
               $"only surviving on their {GetSkillName()}.\n\n" +
               $"{prisonerName} is willing to share their knowledge...";
    }
}