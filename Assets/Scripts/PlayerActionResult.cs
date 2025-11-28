using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Action Result", menuName = "DungeonPunks/Player Action Result")]
public class PlayerActionResult : ScriptableObject
{
    [Header("Result Info")]
    public string resultName;
    public string buttonText; // Text shown on button (e.g., "Melee Attack")
    [TextArea(2, 4)]
    public string description;

    [Header("Result Types")]
    [Tooltip("Defines what happens for each action and result type combination")]
    public List<ResultType> resultTypes = new List<ResultType>();

    [Header("Conditions")]
    [Tooltip("All conditions must be met for this result to apply")]
    public List<ActionCondition> requiredConditions = new List<ActionCondition>();

    [Header("Outcomes")]
    [Tooltip("Outcomes that will be executed when this result applies")]
    public List<ActionOutcome> outcomes = new List<ActionOutcome>();

    /// <summary>
    /// Checks if this result applies to the given action and result
    /// </summary>
    public bool AppliesTo(PlayerAction action, ActionResult result)
    {
        foreach (var resultType in resultTypes)
        {
            if (resultType.action == action && resultType.result == result)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Checks if all conditions are met
    /// </summary>
    public bool CheckConditions(CombatContext context)
    {
        foreach (var condition in requiredConditions)
        {
            if (!EvaluateCondition(condition, context))
            {
                return false;
            }
        }
        return true;
    }

    private bool EvaluateCondition(ActionCondition condition, CombatContext context)
    {
        switch (condition)
        {
            case ActionCondition.PlayerHasMeleeWeapon:
                // TODO: Implement weapon check
                return true;

            case ActionCondition.PlayerHasRangedWeapon:
                // TODO: Implement weapon check
                return true;

            case ActionCondition.PlayerCanCastSpell:
                // TODO: Implement spell check
                return true;

            case ActionCondition.PlayerHasMultipleEnemies:
                return context.enemyCount > 1;

            case ActionCondition.PlayerInjured:
                return context.actingPlayer.healthPoints < context.actingPlayer.maxHealthPoints / 2;

            case ActionCondition.PlayerHealthy:
                return context.actingPlayer.healthPoints >= context.actingPlayer.maxHealthPoints / 2;

            case ActionCondition.EnemyWeakened:
                return context.targetEnemy != null && context.targetEnemy.currentHealthPoints < context.targetEnemy.maxHealthPoints / 2;

            case ActionCondition.EnemyStrong:
                return context.targetEnemy != null && context.targetEnemy.currentHealthPoints >= context.targetEnemy.maxHealthPoints / 2;

            case ActionCondition.AllyNearby:
                return context.aliveAllyCount > 1; // More than just the acting player

            case ActionCondition.PlayerAlone:
                return context.aliveAllyCount == 1; // Only the acting player

            default:
                return true;
        }
    }
}

/// <summary>
/// Links a specific action and result type
/// </summary>
[System.Serializable]
public class ResultType
{
    public PlayerAction action;
    public ActionResult result;
}