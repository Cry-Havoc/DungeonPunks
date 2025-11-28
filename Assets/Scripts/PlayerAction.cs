using UnityEngine;

[CreateAssetMenu(fileName = "New Player Action", menuName = "DungeonPunks/Player Action")]
public class PlayerAction : ScriptableObject
{
    [Header("Action Info")]
    public string actionName;
    public string buttonText; // Text shown on button (e.g., "Melee Attack")
    [TextArea(3, 6)]
    public string description;

    [Header("Trigger")]
    public ActionTriggerType triggerType;

    [Header("Skill Checks")]
    [Tooltip("Attribute checked for success")]
    public PlayerAttribute successCheckAttribute;

    [Tooltip("Attribute checked for critical (only if success check passes)")]
    public PlayerAttribute criticalCheckAttribute;

    [Tooltip("Attribute checked for fumble (only if success check fails)")]
    public PlayerAttribute fumbleCheckAttribute;

    [Header("Advantage/Disadvantage")]
    public DiceRoller.RollType successRollType = DiceRoller.RollType.Normal;
    public DiceRoller.RollType criticalRollType = DiceRoller.RollType.Normal;
    public DiceRoller.RollType fumbleRollType = DiceRoller.RollType.Normal;

    [Header("Combat Text")]
    public string targetSelectionText = "Which enemy does {CHARACTER} want to target?";
    public string actionVerb = "attacking"; // "attacking", "taunting", etc.

    /// <summary>
    /// Gets the attribute value for a given check type
    /// </summary>
    public int GetAttributeValue(PlayerCharacter character, PlayerAttribute attribute)
    {
        switch (attribute)
        {
            case PlayerAttribute.FORCE: return character.force;
            case PlayerAttribute.PERCEPTION: return character.perception;
            case PlayerAttribute.REFLEXE: return character.reflexe;
            case PlayerAttribute.STAMINA: return character.stamina;
            case PlayerAttribute.REASON: return character.reason;
            case PlayerAttribute.WILLPOWER: return character.willPower;
            case PlayerAttribute.HEART: return character.heart;
            default: return 0;
        }
    }

    public string GetAttributeName(PlayerAttribute attribute)
    {
        return attribute.ToString();
    }
}