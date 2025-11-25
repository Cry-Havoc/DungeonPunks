using UnityEngine;
using System.Collections;
using System;

public class DiceRoller : MonoBehaviour
{
    public static DiceRoller Instance { get; private set; }

    [Header("Dice References")]
    public Die tensDigitDie;
    public Die onesDigitDie;

    [Header("Roll Settings")]
    public float rollDuration = 2f;
    public float settleTime = 0.5f;

    private bool isRolling = false;
    private Action<int> onRollComplete;

    public bool IsRolling => isRolling;

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

    void Update()
    {
        // Press SPACE to test roll
        if (Input.GetKeyDown(KeyCode.Space) && !isRolling)
        {
            RollForSkillCheck((result) =>
            {
                Debug.Log($"=== DICE ROLL RESULT: {result} ===");
            });
        }
    }

    /// <summary>
    /// Rolls two d10 dice to generate a number from 1-100
    /// </summary>
    /// <param name="callback">Called when roll is complete with the result (1-100)</param>
    public void RollForSkillCheck(Action<int> callback)
    {
        if (isRolling)
        {
            Debug.LogWarning("Dice are already rolling!");
            return;
        }

        StartCoroutine(PerformRoll(callback));
    }

    /// <summary>
    /// Rolls dice for a predetermined result
    /// </summary>
    /// <param name="targetResult">The desired result (1-100)</param>
    /// <param name="callback">Called when roll is complete</param>
    public void RollForPredeterminedResult(int targetResult, Action<int> callback)
    {
        if (isRolling)
        {
            Debug.LogWarning("Dice are already rolling!");
            return;
        }

        // Convert target result to two dice values
        int tens = (targetResult / 10) % 10;
        int ones = targetResult % 10;

        // Handle special case: 100 should be 0,0 on dice (representing 10,10)
        if (targetResult == 100)
        {
            tens = 0;
            ones = 0;
        }

        StartCoroutine(PerformPredeterminedRoll(tens, ones, callback));
    }

    IEnumerator PerformRoll(Action<int> callback)
    {
        isRolling = true;
        onRollComplete = callback;

        // Generate random predetermined values
        int tensValue = UnityEngine.Random.Range(0, 10); // 0-9 (0 represents 10)
        int onesValue = UnityEngine.Random.Range(0, 10); // 0-9 (0 represents 10)

        // Start both dice rolling
        tensDigitDie.RollToValue(tensValue, rollDuration);
        onesDigitDie.RollToValue(onesValue, rollDuration);

        // Wait for roll to complete
        yield return new WaitForSeconds(rollDuration + settleTime);

        // Calculate final result (1-100)
        int result = CalculateResult(tensValue, onesValue);

        isRolling = false;
        onRollComplete?.Invoke(result);
    }

    IEnumerator PerformPredeterminedRoll(int tensValue, int onesValue, Action<int> callback)
    {
        isRolling = true;
        onRollComplete = callback;

        // Start both dice rolling
        tensDigitDie.RollToValue(tensValue, rollDuration);
        onesDigitDie.RollToValue(onesValue, rollDuration);

        // Wait for roll to complete
        yield return new WaitForSeconds(rollDuration + settleTime);

        // Calculate final result
        int result = CalculateResult(tensValue, onesValue);

        isRolling = false;
        onRollComplete?.Invoke(result);
    }

    int CalculateResult(int tens, int ones)
    {
        // Convert dice values to result
        // 0 on die represents 10
        int tensDigit = (tens == 0) ? 10 : tens;
        int onesDigit = (ones == 0) ? 10 : ones;

        int result = (tensDigit * 10) + onesDigit;

        // Handle edge cases
        if (result > 100) result = 100;
        if (result < 1) result = 1;

        return result;
    }

    /// <summary>
    /// Test method to roll dice from anywhere
    /// </summary>
    public void TestRoll()
    {
        RollForSkillCheck((result) =>
        {
            Debug.Log($"Dice rolled: {result}");
        });
    }

    /// <summary>
    /// Test method for predetermined roll
    /// </summary>
    public void TestPredeterminedRoll(int target)
    {
        RollForPredeterminedResult(target, (result) =>
        {
            Debug.Log($"Dice rolled predetermined: {result}");
        });
    }
}