using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Resolves player actions through dice rolls and applies results
/// </summary>
public class ActionResolver : MonoBehaviour
{
    public static ActionResolver Instance { get; private set; }

    [Header("Action Results Database")]
    [Tooltip("All possible action results in the game")]
    public List<PlayerActionResult> allActionResults;

    private bool isResolving = false;
    public bool IsResolving => isResolving;

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
    }

    /// <summary>
    /// Resolves a player action through skill checks
    /// </summary>
    public void ResolveAction(PlayerAction action, CombatContext context, System.Action<List<ActionOutcome>> onComplete)
    {
        if (isResolving)
        {
            Debug.LogWarning("Already resolving an action!");
            return;
        }

        StartCoroutine(ResolveActionCoroutine(action, context, onComplete));
    }

    IEnumerator ResolveActionCoroutine(PlayerAction action, CombatContext context, System.Action<List<ActionOutcome>> onComplete)
    {
        isResolving = true;
        context.currentAction = action;

        Debug.Log($"=== Resolving Action: {action.actionName} ===");

        // Step 1: Success Check
        int successValue = action.GetAttributeValue(context.actingPlayer, action.successCheckAttribute);
        bool successCheckPassed = false;

        yield return StartCoroutine(PerformSkillCheck(
            successValue,
            action.successRollType,
            $"{action.successCheckAttribute} Success Check",
            (result, success) => {
                successCheckPassed = success;
                Debug.Log($"Success Check: Rolled {result} vs {successValue} = {(success ? "PASS" : "FAIL")}");
            }
        ));

        ActionResult finalResult;

        if (successCheckPassed)
        {
            // Step 2: Critical Check (only if success passed)
            int criticalValue = action.GetAttributeValue(context.actingPlayer, action.criticalCheckAttribute);
            bool criticalCheckPassed = false;

            yield return StartCoroutine(PerformSkillCheck(
                criticalValue,
                action.criticalRollType,
                $"{action.criticalCheckAttribute} Critical Check",
                (result, success) => {
                    criticalCheckPassed = success;
                    Debug.Log($"Critical Check: Rolled {result} vs {criticalValue} = {(success ? "CRITICAL!" : "Normal Success")}");
                }
            ));

            finalResult = criticalCheckPassed ? ActionResult.CriticalSuccess : ActionResult.PartlySuccess;
        }
        else
        {
            // Step 3: Fumble Check (only if success failed)
            int fumbleValue = action.GetAttributeValue(context.actingPlayer, action.fumbleCheckAttribute);
            bool fumbleCheckPassed = false;

            yield return StartCoroutine(PerformSkillCheck(
                fumbleValue,
                action.fumbleRollType,
                $"{action.fumbleCheckAttribute} Fumble Check",
                (result, success) => {
                    fumbleCheckPassed = success;
                    Debug.Log($"Fumble Check: Rolled {result} vs {fumbleValue} = {(success ? "FUMBLE!" : "Normal Failure")}");
                }
            ));

            finalResult = fumbleCheckPassed ? ActionResult.Fumble : ActionResult.PartlyFailure;
        }

        context.actionResult = finalResult;
        Debug.Log($"=== Final Result: {finalResult} ===");

        // Step 4: Find and apply matching action results
        List<ActionOutcome> outcomes = FindAndApplyResults(action, finalResult, context);

        isResolving = false;
        onComplete?.Invoke(outcomes);
    }

    IEnumerator PerformSkillCheck(int targetValue, DiceRoller.RollType rollType, string checkName, System.Action<int, bool> callback)
    {
        bool rollComplete = false;
        int rollResult = 0;

        // Convert RollType to advantage/disadvantage counts
        int advantageCount = rollType == DiceRoller.RollType.Advantage ? 1 : 0;
        int disadvantageCount = rollType == DiceRoller.RollType.Disadvantage ? 1 : 0;

        DiceRoller.Instance.RollForSkillCheck(targetValue, advantageCount, disadvantageCount, (result) => {
            rollResult = result;
            rollComplete = true;
        });

        // Wait for roll to complete
        while (!rollComplete)
        {
            yield return null;
        }

        bool success = rollResult <= targetValue;
        callback?.Invoke(rollResult, success);
    }

    List<ActionOutcome> FindAndApplyResults(PlayerAction action, ActionResult result, CombatContext context)
    {
        List<ActionOutcome> allOutcomes = new List<ActionOutcome>();

        // Find all action results that apply
        foreach (var actionResult in allActionResults)
        {
            if (actionResult.AppliesTo(action, result) && actionResult.CheckConditions(context))
            {
                Debug.Log($"Applying Result: {actionResult.resultName}");

                // Add all outcomes from this result
                allOutcomes.AddRange(actionResult.outcomes);
            }
        }

        // Remove duplicates
        allOutcomes = allOutcomes.Distinct().ToList();

        if (allOutcomes.Count == 0)
        {
            Debug.LogWarning($"No action results found for {action.actionName} with {result}");
        }

        return allOutcomes;
    }

    /// <summary>
    /// Gets all active combat actions for a player
    /// </summary>
    public List<PlayerAction> GetAvailableActions(ActionTriggerType triggerType)
    {
        // TODO: Load from a database or player inventory
        // For now, return empty list
        return new List<PlayerAction>();
    }
}