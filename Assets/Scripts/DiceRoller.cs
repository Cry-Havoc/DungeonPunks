using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System;
using static DiceRoller;

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

    public struct RollTypeData
    {
        public RollType rollType;
        public int advantages; 
        public int disadvantages;
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

    public void Start()
    {
        HideDice();
    }

    public void HideDice()
    {
        tensDigitDie.gameObject.SetActive(false);
        onesDigitDie.gameObject.SetActive(false);
        rollResultText.gameObject.SetActive(false);
    }

    public void ShowDice()
    {
        tensDigitDie.gameObject.SetActive(true);
        onesDigitDie.gameObject.SetActive(true);
        rollResultText.gameObject.SetActive(true); 
    }

    /// <summary>
    /// Rolls for a skill check with advantage/disadvantage counts
    /// </summary>
    /// <param name="skillValue">Target skill value (1-100) to beat</param>
    /// <param name="advantageCount">Number of advantages</param>
    /// <param name="disadvantageCount">Number of disadvantages</param>
    /// <param name="callback">Called with final result after all animations</param>
    public void RollForSkillCheck(int skillValue, int advantageCount, int disadvantageCount, System.Action<int> callback)
    {
        ShowDice();

        if (isRolling)
        {
            Debug.LogWarning("Dice are already rolling!");
            return;
        }

        currentSkillValue = skillValue; 

        // Calculate net advantage/disadvantage
        int netAdvantage = advantageCount - disadvantageCount;

        if (netAdvantage > 0)
        {
            currentRollType = RollType.Advantage;
        }
        else if (netAdvantage < 0)
        {
            currentRollType = RollType.Disadvantage;
        }
        else
        {
            currentRollType = RollType.Normal;
        }

        StartCoroutine(PerformRollWithCounts(callback, advantageCount, disadvantageCount));
    }

    IEnumerator PerformRollWithCounts(System.Action<int> callback, int advantageCount, int disadvantageCount)
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

        // Apply advantage/disadvantage logic with counts
        yield return StartCoroutine(HandleAdvantageDisadvantageWithCounts(tensValue, onesValue, initialResult, advantageCount, disadvantageCount));

        isRolling = false;
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

        // Apply advantage/disadvantage logic with counts (0, 0 for normal roll)
        yield return StartCoroutine(HandleAdvantageDisadvantageWithCounts(tensValue, onesValue, initialResult, 0, 0));

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

        // Apply advantage/disadvantage logic with counts (0, 0 for normal roll)
        yield return StartCoroutine(HandleAdvantageDisadvantageWithCounts(tensValue, onesValue, initialResult, 0, 0));

        isRolling = false;
    }

    IEnumerator HandleAdvantageDisadvantageWithCounts(int tensValue, int onesValue, int initialResult, int advantageCount, int disadvantageCount)
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
            Color mirrorSuccessColor = mirrorSuccess ? GameUIManager.Instance.positiveText : GameUIManager.Instance.negativeText;

            if (rollResultText != null)
            {
                rollResultText.text = $"<color=green>Mirror Dice</color>\n<color=#{ColorUtility.ToHtmlStringRGB(mirrorSuccessColor)}>{mirrorSuccessText}</color>";
            }

            onRollComplete?.Invoke(initialResult);
            yield break;
        }

        int finalResult = initialResult;
        bool needSwap = false;

        // Calculate net advantage/disadvantage
        int netAdvantage = advantageCount - disadvantageCount;
        string rollTypeDisplayText = "";

        if (netAdvantage > 0)
        {
            // Net Advantage
            currentRollType = RollType.Advantage;

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
            }

            // Build display text
            if (disadvantageCount > 0)
            {
                rollTypeDisplayText = $"{advantageCount} Advantage - <s>{disadvantageCount} Disadvantage</s>";
            }
            else
            {
                rollTypeDisplayText = "Advantage";
            }

            if (rollResultText != null)
            {
                rollResultText.text = rollTypeDisplayText;
                rollResultText.color = GameUIManager.Instance.positiveText;
            }
        }
        else if (netAdvantage < 0)
        {
            // Net Disadvantage
            currentRollType = RollType.Disadvantage;

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
            }

            // Build display text
            if (advantageCount > 0)
            {
                rollTypeDisplayText = $"<s>{advantageCount} Advantage</s> - {disadvantageCount} Disadvantage";
            }
            else
            {
                rollTypeDisplayText = "Disadvantage";
            }

            if (rollResultText != null)
            {
                rollResultText.text = rollTypeDisplayText;
                rollResultText.color = Color.red;
            }
        }
        else if (advantageCount > 0 && disadvantageCount > 0)
        {
            // Tie - both cancel out
            currentRollType = RollType.Normal;

            rollTypeDisplayText = $"<s>{advantageCount} Advantage</s> - <s>{disadvantageCount} Disadvantage</s>";

            if (rollResultText != null)
            {
                rollResultText.text = rollTypeDisplayText;
                rollResultText.color = Color.white;
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
        Color finalSuccessColor = finalSuccess ? GameUIManager.Instance.positiveText : GameUIManager.Instance.negativeText;

        if (rollResultText != null)
        {
            if (currentRollType == RollType.Normal && advantageCount == 0 && disadvantageCount == 0)
            {
                rollResultText.text = finalSuccessText;
                rollResultText.color = finalSuccessColor;
            }
            else
            {
                Color rollTypeColor = currentRollType == RollType.Advantage ? GameUIManager.Instance.positiveText :
                                      currentRollType == RollType.Disadvantage ? GameUIManager.Instance.negativeText: GameUIManager.Instance.normalText;

                if (!string.IsNullOrEmpty(rollTypeDisplayText))
                {
                    rollResultText.text = $"{rollTypeDisplayText}\n<color=#{ColorUtility.ToHtmlStringRGB(finalSuccessColor)}>{finalSuccessText}</color>";
                }
                else
                {
                    rollResultText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(finalSuccessColor)}>{finalSuccessText}</color>";
                }
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