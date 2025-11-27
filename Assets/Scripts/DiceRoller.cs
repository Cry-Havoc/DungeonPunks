using UnityEngine;
using UnityEngine.UI;
using TMPro;
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
    public float swapAnimationDuration = 0.5f;

    [Header("UI References")]
    public TextMeshProUGUI rollResultText;

    [Header("Dice Materials")]
    public Material whiteMaterial;
    public Material redMaterial;
    public Material blueMaterial;
    public Material greenMaterial;

    private bool isRolling = false;
    private Action<int> onRollComplete;
    private RollType currentRollType = RollType.Normal;
    private int currentSkillValue = 50;

    public bool IsRolling => isRolling;

    public enum RollType
    {
        Normal,
        Advantage,
        Disadvantage
    }

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
    /// Rolls for a skill check with advantage/disadvantage
    /// </summary>
    /// <param name="skillValue">Target skill value (1-100) to beat</param>
    /// <param name="rollType">Normal, Advantage, or Disadvantage</param>
    /// <param name="callback">Called with final result after all animations</param>
    public void RollForSkillCheck(int skillValue, RollType rollType, Action<int> callback)
    {
        if (isRolling)
        {
            Debug.LogWarning("Dice are already rolling!");
            return;
        }

        currentSkillValue = skillValue;
        currentRollType = rollType;
        StartCoroutine(PerformRoll(callback));
    }

    /// <summary>
    /// Rolls dice for a predetermined result with roll type
    /// </summary>
    public void RollForPredeterminedResult(int targetResult, int skillValue, RollType rollType, Action<int> callback)
    {
        if (isRolling)
        {
            Debug.LogWarning("Dice are already rolling!");
            return;
        }

        currentSkillValue = skillValue;
        currentRollType = rollType;

        // Convert target result to two dice values
        int tens = (targetResult / 10) % 10;
        int ones = targetResult % 10;

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

        // Clear text
        if (rollResultText != null)
        {
            rollResultText.text = "";
        }

        // Set both dice to white initially
        tensDigitDie.SetMaterial(whiteMaterial);
        onesDigitDie.SetMaterial(whiteMaterial);

        // Generate random predetermined values
        int tensValue = UnityEngine.Random.Range(0, 10);
        int onesValue = UnityEngine.Random.Range(0, 10);

        // Start both dice rolling
        tensDigitDie.RollToValue(tensValue, rollDuration);
        onesDigitDie.RollToValue(onesValue, rollDuration);

        // Wait for roll to complete
        yield return new WaitForSeconds(rollDuration + settleTime);

        // Calculate initial result
        int initialResult = CalculateResult(tensValue, onesValue);

        // Apply advantage/disadvantage logic
        yield return StartCoroutine(HandleAdvantageDisadvantage(tensValue, onesValue, initialResult));

        isRolling = false;
    }

    IEnumerator PerformPredeterminedRoll(int tensValue, int onesValue, Action<int> callback)
    {
        isRolling = true;
        onRollComplete = callback;

        // Clear text
        if (rollResultText != null)
        {
            rollResultText.text = "";
        }

        // Set both dice to white initially
        tensDigitDie.SetMaterial(whiteMaterial);
        onesDigitDie.SetMaterial(whiteMaterial);

        // Start both dice rolling
        tensDigitDie.RollToValue(tensValue, rollDuration);
        onesDigitDie.RollToValue(onesValue, rollDuration);

        // Wait for roll to complete
        yield return new WaitForSeconds(rollDuration + settleTime);

        // Calculate initial result
        int initialResult = CalculateResult(tensValue, onesValue);

        // Apply advantage/disadvantage logic
        yield return StartCoroutine(HandleAdvantageDisadvantage(tensValue, onesValue, initialResult));

        isRolling = false;
    }

    IEnumerator HandleAdvantageDisadvantage(int tensValue, int onesValue, int initialResult)
    {
        int tensActual = (tensValue == 0) ? 10 : tensValue;
        int onesActual = (onesValue == 0) ? 10 : onesValue;

        // Check for mirror dice
        if (tensActual == onesActual)
        {
            tensDigitDie.SetMaterial(greenMaterial);
            onesDigitDie.SetMaterial(greenMaterial);

            bool mirrorSuccess = initialResult <= currentSkillValue;
            string mirrorSuccessText = mirrorSuccess ? "Success" : "Failure";
            Color mirrorSuccessColor = mirrorSuccess ? Color.blue : Color.red;

            if (rollResultText != null)
            {
                rollResultText.text = $"<color=green>Mirror Dice</color>\n<color=#{ColorUtility.ToHtmlStringRGB(mirrorSuccessColor)}>{mirrorSuccessText}</color>";
            }

            onRollComplete?.Invoke(initialResult);
            yield break;
        }

        int finalResult = initialResult;
        bool needSwap = false;

        if (currentRollType == RollType.Disadvantage)
        {
            // Higher number gets red
            if (tensActual > onesActual)
            {
                tensDigitDie.SetMaterial(redMaterial);
                onesDigitDie.SetMaterial(whiteMaterial);
            }
            else
            {
                tensDigitDie.SetMaterial(whiteMaterial);
                onesDigitDie.SetMaterial(redMaterial);
            }

            // If higher is on right (ones), swap to bring it to front
            if (onesActual > tensActual)
            {
                needSwap = true;
                if (rollResultText != null)
                {
                    rollResultText.text = "Disadvantage";
                    rollResultText.color = Color.red;
                }
            }
        }
        else if (currentRollType == RollType.Advantage)
        {
            // Lower number gets blue
            if (tensActual < onesActual)
            {
                tensDigitDie.SetMaterial(blueMaterial);
                onesDigitDie.SetMaterial(whiteMaterial);
            }
            else
            {
                tensDigitDie.SetMaterial(whiteMaterial);
                onesDigitDie.SetMaterial(blueMaterial);
            }

            // If lower is on right (ones), swap to bring it to front
            if (onesActual < tensActual)
            {
                needSwap = true;
                if (rollResultText != null)
                {
                    rollResultText.text = "Advantage";
                    rollResultText.color = Color.blue;
                }
            }
        }

        // Perform swap animation if needed
        if (needSwap)
        {
            yield return new WaitForSeconds(0.5f); // Brief pause before swap
            yield return StartCoroutine(SwapDiceAnimation());

            // Calculate swapped result
            finalResult = CalculateResult(onesValue, tensValue);
        }

        // Add success/failure text
        bool finalSuccess = finalResult <= currentSkillValue;
        string finalSuccessText = finalSuccess ? "Success" : "Failure";
        Color finalSuccessColor = finalSuccess ? Color.blue : Color.red;

        if (rollResultText != null)
        {
            if (currentRollType == RollType.Normal)
            {
                rollResultText.text = finalSuccessText;
                rollResultText.color = finalSuccessColor;
            }
            else
            {
                string advantageText = currentRollType == RollType.Advantage ? "Advantage" : "Disadvantage";
                Color advantageColor = currentRollType == RollType.Advantage ? Color.blue : Color.red;

                rollResultText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(advantageColor)}>{advantageText}</color>\n<color=#{ColorUtility.ToHtmlStringRGB(finalSuccessColor)}>{finalSuccessText}</color>";
            }
        }

        onRollComplete?.Invoke(finalResult);
    }

    IEnumerator SwapDiceAnimation()
    {
        Vector3 tensStart = tensDigitDie.transform.position;
        Vector3 onesStart = onesDigitDie.transform.position;

        Quaternion tensRotStart = tensDigitDie.transform.rotation;
        Quaternion onesRotStart = onesDigitDie.transform.rotation;

        float elapsed = 0f;

        while (elapsed < swapAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / swapAnimationDuration;

            // Use ease-in-out for smooth animation
            float smoothT = Mathf.SmoothStep(0, 1, t);

            // Swap positions with arc
            float arcHeight = 1f;
            Vector3 arcOffset = Vector3.up * Mathf.Sin(t * Mathf.PI) * arcHeight;

            tensDigitDie.transform.position = Vector3.Lerp(tensStart, onesStart, smoothT) + arcOffset;
            onesDigitDie.transform.position = Vector3.Lerp(onesStart, tensStart, smoothT) + arcOffset;

            // Swap rotations
            tensDigitDie.transform.rotation = Quaternion.Slerp(tensRotStart, onesRotStart, smoothT);
            onesDigitDie.transform.rotation = Quaternion.Slerp(onesRotStart, tensRotStart, smoothT);

            yield return null;
        }

        // Ensure final positions are exact
        tensDigitDie.transform.position = onesStart;
        onesDigitDie.transform.position = tensStart;
        tensDigitDie.transform.rotation = onesRotStart;
        onesDigitDie.transform.rotation = tensRotStart;

        // Update original positions in Die scripts
        tensDigitDie.SetOriginalPosition(onesStart);
        onesDigitDie.SetOriginalPosition(tensStart);

        // Swap the die references
        Die temp = tensDigitDie;
        tensDigitDie = onesDigitDie;
        onesDigitDie = temp;
    }

    int CalculateResult(int tens, int ones)
    {
        int tensDigit = (tens == 0) ? 10 : tens;
        int onesDigit = (ones == 0) ? 10 : ones;

        int result = (tensDigit * 10) + onesDigit;

        if (result > 100) result = 100;
        if (result < 1) result = 1;

        return result;
    }
}