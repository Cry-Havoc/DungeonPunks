using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using static DiceRoller;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }

    [Header("Monster Prefabs")]
    public List<GameObject> monsterPrefabs;

    [Header("Available Player Actions")]
    public List<PlayerAction> availableActions;

    [Header("UI References")]
    public TextMeshProUGUI encounterText;
    public Image encounterImage;
    public GameObject monsterListUI;
    public GameObject characterButtonPrefab;
    public GameObject actionButtonPrefab;
    public GameObject actionResultButtonPrefab;
    public Transform monsterListContainer;
    public Transform actionListContainer;
    public Transform actionResultListContainer;

    private List<Monster> activeMonsters = new List<Monster>();
    private List<CharacterButton> characterButtons = new List<CharacterButton>();
    private List<ActionButton> actionButtons = new List<ActionButton>();
    private List<ActionResultButton> actionResultButtons = new List<ActionResultButton>();
    private PlayerCharacter[] partyMembers;

    private bool isInCombat = false;
    private bool waitingForSpace = false;

    public ActionTriggerType currentActionType { get; private set; }

    private bool selectingMonster = false;
    private bool selectingAction = false;
    private bool selectingActionResult = false;
    private PlayerCharacter currentActingPlayer;
    private PlayerCharacter randomAllyPlayer;
    private PlayerAction currentAction;
    private Monster currentTarget;
    private ActionResult currentResult;
    private int currentActiveMonsterIndex = 0;

    private System.Action onSpacePressed;

    public bool IsInCombat => isInCombat;

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
        // Block input when menu is open
        if (MainMenuManager.IsGamePaused())
            return;

        // Only process space if we're waiting and not selecting anything
        if (waitingForSpace && !selectingMonster && !selectingAction && !selectingActionResult && Input.GetKeyDown(KeyCode.Space))
        {
            onSpacePressed?.Invoke();
        }
    }

    public void Initialize(PlayerCharacter[] party)
    {
        partyMembers = party;
    }

    public void StartRandomEncounter()
    {
        if (isInCombat) return;

        isInCombat = true;

        // Disable player movement immediately
        if (PlayerPartyController.Instance != null)
        {
            PlayerPartyController.Instance.enabled = false;
        }

        // Show encounter warning
        encounterText.text = "You are not alone ... press <u>Space</u> to continue";
        encounterImage.gameObject.SetActive(false);

        waitingForSpace = true;
        selectingMonster = false;
        selectingAction = false;
        selectingActionResult = false;
        onSpacePressed = InitiateCombat;
    }

    void InitiateCombat()
    {
        waitingForSpace = false;

        // Switch UI to combat mode
        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.SetCombatMode();
        }

        // Switch UI to combat mode
        GameUIManager.Instance.SetCombatMode();
         
        List<string> monsterNames = new List<string>();

        // ✅ USES MONSTERSPAWNMANAGER
        if (MonsterSpawnManager.Instance != null)
        {
            activeMonsters = MonsterSpawnManager.Instance.SpawnEncounter();

            foreach(Monster monster in activeMonsters)
            {
                if (!monsterNames.Contains(monster.baseMonsterName))
                {
                    monsterNames.Add(monster.baseMonsterName);
                }
            }
        }
        else
        {
            // ⚠️ Fallback only if MonsterSpawnManager missing
            Debug.LogWarning("MonsterSpawnManager not found, using fallback spawn system");
            // Spawn 1-8 monsters
            int monsterCount = Random.Range(1, 9);
            activeMonsters.Clear();

            Monster monsterCopy = null;
            for (int i = 0; i < monsterCount; i++)
            {
                int monsterTypeCount = monsterNames.Distinct().Count();

                if (monsterTypeCount < 3)
                {
                    GameObject prefab = monsterPrefabs[Random.Range(0, monsterPrefabs.Count)];
                    monsterCopy = prefab.GetComponent<Monster>().CreateCopy();
                }
                else
                {
                    monsterCopy = GetRandomMatchingPrefab(monsterNames).CreateCopy();
                }

                monsterCopy.gameObject.SetActive(false);
                activeMonsters.Add(monsterCopy);

                if (!monsterNames.Contains(monsterCopy.baseMonsterName))
                {
                    monsterNames.Add(monsterCopy.baseMonsterName);
                }
            }
        }
        

        

        // Show first monster
        currentActiveMonsterIndex = 0;
        UpdateMonsterDisplay();
        CreateCharacterButtons();

        // Hide all UI initially
        monsterListUI.SetActive(false);

        encounterText.text = "You are not alone ... \n\n";

        if (activeMonsters.Count == 1)
        {
            encounterText.text += "A single " + activeMonsters[0].monsterName + " is attacking you!\n\nPress <u>Space</u> to continue..";
        }
        else
        {
            if (monsterNames.Count > 0)
            {
                // Add all but the last monster
                for (int i = 0; i < monsterNames.Count - 1; i++)
                {
                    encounterText.text += monsterNames[i] + "s, ";
                }

                // Add the last monster with the final sentence
                string lastMonster = monsterNames[monsterNames.Count - 1];
                encounterText.text += "and " + lastMonster + "s are attacking you\n\nPress <u>Space</u> to continue..";
            }
        }
        encounterImage.gameObject.SetActive(true);

        waitingForSpace = true;
        selectingMonster = false;
        selectingAction = false;
        selectingActionResult = false;
        onSpacePressed = StartPlayerTurn;
    }

    Monster GetRandomMatchingPrefab(List<string> monsterNames)
    {
        var matches = monsterPrefabs
            .Where(prefab => monsterNames.Contains(prefab.GetComponent<Monster>().baseMonsterName))
            .ToList();

        if (matches.Count == 0)
            return null;

        return matches[Random.Range(0, matches.Count)].GetComponent<Monster>();
    }

    void CreateCharacterButtons()
    {
        // Clear existing buttons
        foreach (var btn in characterButtons)
        {
            if (btn != null) Destroy(btn.gameObject);
        }
        characterButtons.Clear();

        // Create new buttons
        for (int i = 0; i < activeMonsters.Count; i++)
        {
            GameObject btnObj = Instantiate(characterButtonPrefab, monsterListContainer);
            CharacterButton characterBtn = btnObj.GetComponent<CharacterButton>();
            characterBtn.Initialize(i + 1, activeMonsters[i], this);
            characterButtons.Add(characterBtn);
        }
    }

    void UpdateCharacterButtons()
    {
        // Remove dead monsters and their buttons
        for (int i = characterButtons.Count - 1; i >= 0; i--)
        {
            if (!activeMonsters[i].IsAlive())
            {
                Destroy(characterButtons[i].gameObject);
                Destroy(activeMonsters[i].gameObject);
                characterButtons.RemoveAt(i);
                activeMonsters.RemoveAt(i);
            }
        }

        // Renumber remaining buttons
        for (int i = 0; i < characterButtons.Count; i++)
        {
            characterButtons[i].SetNumber(i + 1);
            characterButtons[i].UpdateDisplay();
        }

        // Update active monster index if needed
        if (currentActiveMonsterIndex >= activeMonsters.Count && activeMonsters.Count > 0)
        {
            currentActiveMonsterIndex = 0;
        }
    }

    void UpdateMonsterDisplay()
    {
        if (activeMonsters.Count > 0 && currentActiveMonsterIndex < activeMonsters.Count)
        {
            encounterImage.sprite = activeMonsters[currentActiveMonsterIndex].monsterPicture;
            encounterImage.gameObject.SetActive(true);
        }
    }

    void StartPlayerTurn()
    {
        waitingForSpace = false;
        selectingMonster = false;
        selectingAction = false;
        selectingActionResult = false;

        // Find available players
        List<PlayerCharacter> availablePlayers = partyMembers
            .Where(p => p != null && p.healthPoints > 0 && !p.hasActedThisCycle)
            .ToList();

        // If no available players, reset cycle
        if (availablePlayers.Count == 0)
        {
            foreach (var player in partyMembers)
            {
                if (player != null) player.hasActedThisCycle = false;
            }
            availablePlayers = partyMembers.Where(p => p != null && p.healthPoints > 0).ToList();
        }

        // Check if all players dead
        if (availablePlayers.Count == 0)
        {
            EndCombat(false);
            return;
        }

        // Pick random player
        currentActingPlayer = availablePlayers[Random.Range(0, availablePlayers.Count)];
        currentActingPlayer.hasActedThisCycle = true;

        encounterText.text = $"It's time for {currentActingPlayer.characterName} to act...";

        currentActionType = ActionTriggerType.ActiveCombat;

        // Show action selection
        ShowActionSelection();
    }

    void ShowActionSelection()
    {
        selectingAction = true;

        // Clear existing action buttons
        foreach (var btn in actionButtons)
        {
            if (btn != null) Destroy(btn.gameObject);
        }
        actionButtons.Clear();

        // Get available ActiveCombat actions
        List<PlayerAction> availableActiveCombatActions = availableActions
            .Where(a => a.triggerType == currentActionType)
            .ToList();

        // Create action buttons
        for (int i = 0; i < availableActiveCombatActions.Count; i++)
        {
            GameObject btnObj = Instantiate(actionButtonPrefab, actionListContainer);
            ActionButton actionBtn = btnObj.GetComponent<ActionButton>();
            actionBtn.InitializeActionButton(i + 1, availableActiveCombatActions[i], currentActingPlayer, OnActionSelected);
            actionBtn.SetSelectable(true);
            actionButtons.Add(actionBtn);
        }

        actionListContainer.gameObject.SetActive(true);
    }

    void OnActionSelected(PlayerAction action)
    {
        selectingAction = false;
        currentAction = action;

        // Hide action list
        actionListContainer.gameObject.SetActive(false);

        // Disable action buttons
        foreach (var btn in actionButtons)
        {
            btn.SetSelectable(false);
        }

        // Show target selection
        string selectionText = action.targetSelectionText.Replace("{CHARACTER}", currentActingPlayer.characterName);
        encounterText.text = selectionText;

        if(currentActionType == ActionTriggerType.ReactionCombat) 
        {
            StartActionResolution();
        }
        else
        {
            // Show monster list and enable selection
            monsterListUI.SetActive(true);
            selectingMonster = true;

            foreach (var btn in characterButtons)
            {
                btn.SetSelectable(true);
            }
        } 
    }
    public void OnMonsterSelected(int monsterIndex)
    {
        if (monsterIndex < 0 || monsterIndex >= activeMonsters.Count) return;
        if (!selectingMonster) return;

        selectingMonster = false;
        currentTarget = activeMonsters[monsterIndex];

        // Hide monster list
        monsterListUI.SetActive(false);

        // Disable further selection
        foreach (var btn in characterButtons)
        {
            btn.SetSelectable(false);
        }

        // Start action resolution
        StartActionResolution();
    }

    void StartActionResolution()
    {
        // Build context
        CombatContext context = new CombatContext(
            currentActingPlayer,
            currentTarget,
            activeMonsters.Count,
            partyMembers.Count(p => p != null && p.healthPoints > 0)
        );

        // Show action text
        encounterText.text = $"{currentActingPlayer.characterName} is {currentAction.actionVerb} {currentTarget.monsterName}...\n";

        StartCoroutine(ResolveActionWithDice(context));
    }

    System.Collections.IEnumerator ResolveActionWithDice(CombatContext context)
    {
        // Success Check
        int successValue = currentAction.GetAttributeValue(currentActingPlayer, currentAction.successCheckAttribute);
        string successAttr = currentAction.GetAttributeName(currentAction.successCheckAttribute);

        // Apply advantage/disadvantage if player has status effects
        DiceRoller.RollTypeData successRollType = GetModifiedRollType(currentAction.successRollType, true);

        encounterText.text += $"\nRoll lower than {successAttr} ({successValue})...";

        bool successPassed = false;
        int successRoll = 0;

        bool rollComplete = false;
        DiceRoller.Instance.RollForSkillCheck(successValue, successRollType.advantages, successRollType.disadvantages, (result) => {
            successRoll = result;
            successPassed = result <= successValue;
            rollComplete = true;
        });

        while (!rollComplete) yield return null;

        string rollTypeText = GetRollTypeText(successRollType.rollType);

        encounterText.text += $" - Rolled {successRoll} {rollTypeText}";
        encounterText.text += $"\n\n{currentActingPlayer.characterPronoun} {(successPassed ? "successful" : "unsuccessful")}\n\nPress <u>Space</u> to continue ...";

        waitingForSpace = true;
        selectingMonster = false;
        selectingAction = false;
        selectingActionResult = false;
        onSpacePressed = SpacePressedDuringCombat;

        while (waitingForSpace) yield return null;

        DiceRoller.Instance.HideDice();

        // Critical or Fumble Check
        if (successPassed)
        {
            // Critical Check
            int criticalValue = currentAction.GetAttributeValue(currentActingPlayer, currentAction.criticalCheckAttribute);
            string criticalAttr = currentAction.GetAttributeName(currentAction.criticalCheckAttribute);
            DiceRoller.RollTypeData criticalRollType = GetModifiedRollType(currentAction.criticalRollType, true);

            encounterText.text += $"\nFor a Critical roll lower than {criticalAttr} ({criticalValue})...";

            bool criticalPassed = false;
            int criticalRoll = 0;

            rollComplete = false;
            DiceRoller.Instance.RollForSkillCheck(criticalValue, criticalRollType.advantages, criticalRollType.disadvantages, (result) => {
                criticalRoll = result;
                criticalPassed = result <= criticalValue;
                rollComplete = true;
            });

            while (!rollComplete) yield return null;

            rollTypeText = GetRollTypeText(criticalRollType.rollType);
            encounterText.text += $" - Rolled {criticalRoll} {rollTypeText}";

            currentResult = criticalPassed ? ActionResult.CriticalSuccess : ActionResult.PartlySuccess;
        }
        else
        {
            // Fumble Check
            int fumbleValue = currentAction.GetAttributeValue(currentActingPlayer, currentAction.fumbleCheckAttribute);
            string fumbleAttr = currentAction.GetAttributeName(currentAction.fumbleCheckAttribute);
            DiceRoller.RollTypeData fumbleRollType = GetModifiedRollType(currentAction.fumbleRollType, false);

            encounterText.text += $"\nTo avoid a Fumble roll lower than {fumbleAttr} ({fumbleValue})...";

            bool fumblePassed = false;
            int fumbleRoll = 0;

            rollComplete = false;
            DiceRoller.Instance.RollForSkillCheck(fumbleValue, fumbleRollType.advantages, fumbleRollType.disadvantages, (result) => {
                fumbleRoll = result;
                fumblePassed = result <= fumbleValue;
                rollComplete = true;
            });

            while (!rollComplete) yield return null;

            rollTypeText = GetRollTypeText(fumbleRollType.rollType);
            encounterText.text += $" - Rolled {fumbleRoll} {rollTypeText}";

            currentResult = fumblePassed ? ActionResult.PartlyFailure : ActionResult.Fumble;
        }

        encounterText.text += $"\n\n<b>{GetResultText(currentResult)}</b>\n\nPress <u>Space</u> to continue ...";

        waitingForSpace = true;
        selectingMonster = false;
        selectingAction = false;
        selectingActionResult = false;
        onSpacePressed = ShowActionResultSelection;
    }
    string GetRollTypeText(DiceRoller.RollType rollType)
    {
        switch (rollType)
        {
            case DiceRoller.RollType.Advantage: return "(Advantage)";
            case DiceRoller.RollType.Disadvantage: return "(Disadvantage)";
            default: return "";
        }
    } 

    DiceRoller.RollTypeData GetModifiedRollType(DiceRoller.RollType baseType, bool isAttack)
    {
        int countDisadvantages = 0;
        int countAdvantages = 0;

        // Check player status effects
        if (currentActingPlayer.advantageCount > 0)
        {
            currentActingPlayer.advantageCount--;
            countAdvantages++;
        }
        if (currentActingPlayer.disadvantageCount > 0)
        {
            currentActingPlayer.disadvantageCount--;
            countDisadvantages++;
        }
        if (isAttack && currentActingPlayer.advantageAttackCount > 0)
        {
            currentActingPlayer.advantageAttackCount--;
            countAdvantages++;
        }
        if (isAttack && currentActingPlayer.disadvantageAttackCount > 0)
        {
            currentActingPlayer.disadvantageAttackCount--;
            countDisadvantages++;
        }
        if (!isAttack && currentActingPlayer.advantageDefenseCount > 0)
        {
            currentActingPlayer.advantageDefenseCount--;
            countAdvantages++;
        }
        if (!isAttack && currentActingPlayer.disadvantageDefenseCount > 0)
        {
            currentActingPlayer.disadvantageCount--;
            countDisadvantages++;
        }

        RollTypeData rollTypeData = new RollTypeData();

        if (countDisadvantages > countAdvantages)
        {
            rollTypeData.rollType = DiceRoller.RollType.Disadvantage;
        }
        else if (countAdvantages > countDisadvantages)
        {
            rollTypeData.rollType = DiceRoller.RollType.Advantage;
        }
        else
        {
            rollTypeData.rollType = baseType;
        }

        rollTypeData.advantages = countAdvantages;
        rollTypeData.disadvantages = countDisadvantages;
        return rollTypeData;
    }
    string GetResultText(ActionResult result)
    {
        switch (result)
        {
            case ActionResult.CriticalSuccess: return "A Critical Success";
            case ActionResult.PartlySuccess: return "A Partial Success";
            case ActionResult.PartlyFailure: return "A Partial Failure";
            case ActionResult.Fumble: return "A Fumble";
            default: return "";
        }
    }

    void ShowActionResultSelection()
    {
        encounterText.text = $"<b>{GetResultText(currentResult)}!</b> Choose an option for {currentActingPlayer.characterName} ...";

        selectingActionResult = true;

        // Clear existing result buttons
        foreach (var btn in actionResultButtons)
        {
            if (btn != null) Destroy(btn.gameObject);
        }
        actionResultButtons.Clear();

        // Find matching action results
        List<PlayerActionResult> matchingResults = ActionResolver.Instance.allActionResults
            .Where(r => r.AppliesTo(currentAction, currentResult))
            .ToList();

        if (matchingResults.Count == 0)
        {
            Debug.LogWarning($"No results found for {currentAction.actionName} with {currentResult}");
            ContinueCombat();
            return;
        }

        // Create result buttons
        for (int i = 0; i < matchingResults.Count; i++)
        {
            GameObject btnObj = Instantiate(actionResultButtonPrefab, actionResultListContainer);
            ActionResultButton resultBtn = btnObj.GetComponent<ActionResultButton>();
            resultBtn.InitializeResultButton(i + 1, matchingResults[i], OnActionResultSelected);
            resultBtn.SetSelectable(true);
            actionResultButtons.Add(resultBtn);
        }

        actionResultListContainer.gameObject.SetActive(true);
    }

    void OnActionResultSelected(PlayerActionResult selectedResult)
    {
        DiceRoller.Instance.HideDice();

        selectingActionResult = false;

        // Hide result list
        actionResultListContainer.gameObject.SetActive(false);

        // Disable result buttons
        foreach (var btn in actionResultButtons)
        {
            btn.SetSelectable(false);
        }

        // Execute outcomes
        ExecuteOutcomes(selectedResult);
    }

    void ExecuteOutcomes(PlayerActionResult result)
    {
        encounterText.text += $"\n\n{result.buttonText} : ( {result.description} )\n"; //TODO remove description

        List<PlayerCharacter> eligiblePlayers = partyMembers
                              .Where(p => p != currentActingPlayer && p.healthPoints > 0)
                             .ToList();

        // Pick one randomly if any exist
        randomAllyPlayer = null;
        if (eligiblePlayers.Count > 0)
        {
            randomAllyPlayer = eligiblePlayers[Random.Range(0, eligiblePlayers.Count)];
        }

        foreach (var outcome in result.outcomes)
        {
            ExecuteOutcome(outcome, result);
        }

        // Clear temporary status effects used in this action
        ClearTemporaryStatusEffects();

        // Update UI
        UpdateCharacterButtons();
        UpdateMonsterDisplay();

        // Check if combat should end
        if (activeMonsters.Count == 0)
        {
            EndCombat(true);
            return;
        }

        List<PlayerCharacter> alivePlayers = partyMembers.Where(p => p != null && p.healthPoints > 0).ToList();
        if (alivePlayers.Count == 0)
        {
            EndCombat(false);
            return;
        }

        // Check if Combat is over combat
        waitingForSpace = true;

        if (currentActionType == ActionTriggerType.ReactionCombat) 
        { 
            onSpacePressed = () => StartPlayerTurn();
        }
        else
        { 
            onSpacePressed = () => StartMonsterTurn();
        }

        encounterText.text += $"\n\nPress SPACE to continue...";
    }
    void ExecuteOutcome(ActionOutcome outcome, PlayerActionResult result)
    {  
        switch (outcome)
        {
            case ActionOutcome.DealNormalDamage:
                currentTarget.TakeDamage(currentActingPlayer.damageAmount);
                encounterText.text += $"\n{currentTarget.monsterName} suffers {currentActingPlayer.damageAmount} damage";
                break;

            case ActionOutcome.DealDoubleDamage:
                currentTarget.TakeDamage(currentActingPlayer.damageAmount * 2);
                encounterText.text += $"\n{currentTarget.monsterName} suffers {currentActingPlayer.damageAmount * 2} damage";
                break;

            case ActionOutcome.DealTripleDamage:
                currentTarget.TakeDamage(currentActingPlayer.damageAmount * 3);
                encounterText.text += $"\n{currentTarget.monsterName} suffers {currentActingPlayer.damageAmount * 3} damage";
                break;

            case ActionOutcome.AllyRedirectDamage:
                // Deals monster's damage (currently 1) to ally
                int monsterAllyDamage = 1;
                randomAllyPlayer.TakeDamage(monsterAllyDamage);
                encounterText.text += $"\n{randomAllyPlayer.characterName} suffers {monsterAllyDamage} damage";
                break;

            case ActionOutcome.TakeDamage:
                // Deals monster's damage (currently 1)
                int monsterDamage = 1;
                currentActingPlayer.TakeDamage(monsterDamage);
                encounterText.text += $"\n{currentActingPlayer.characterName} suffers {monsterDamage} damage";
                break;

            case ActionOutcome.TakeExhaustionDamage:
                // Escalating damage based on how many times chosen
                currentActingPlayer.exhaustionDamageLevel++;
                currentActingPlayer.TakeDamage(currentActingPlayer.exhaustionDamageLevel);
                encounterText.text += $"\n{currentActingPlayer.characterName} suffers {currentActingPlayer.exhaustionDamageLevel} exhaustion damage";
                break;

            case ActionOutcome.DealDamageToAlly:
                // Random ally takes player's damage
                List<PlayerCharacter> otherAllies = partyMembers
                    .Where(p => p != null && p != currentActingPlayer && p.healthPoints > 0)
                    .ToList();

                if (otherAllies.Count > 0)
                {
                    PlayerCharacter randomAlly = otherAllies[Random.Range(0, otherAllies.Count)];
                    randomAlly.TakeDamage(currentActingPlayer.damageAmount);
                    encounterText.text += $"\n{randomAlly.characterName} suffers {currentActingPlayer.damageAmount} damage";
                }
                break;

            case ActionOutcome.GainAdvantageNextRoll:
                currentActingPlayer.advantageCount++;
                break;

            case ActionOutcome.GainDisadvantageNextRoll:
                currentActingPlayer.disadvantageCount++;
                break;

            case ActionOutcome.GainAdvantageNextAttack:
                currentActingPlayer.advantageAttackCount++;
                break;

            case ActionOutcome.GainDisadvantageNextAttack:
                currentActingPlayer.disadvantageAttackCount++;
                break;

            case ActionOutcome.GainAdvantageNextDefense:
                currentActingPlayer.advantageDefenseCount++;
                break;

            case ActionOutcome.GainDisadvantageNextDefense:
                currentActingPlayer.disadvantageDefenseCount++;
                break;

            case ActionOutcome.EnemyAttacksAreDefendedWithAdvantage:
                currentTarget.advantageWhenDefendedAgainstCount++;
                encounterText.text += $"\nDefending against {currentTarget.monsterName} gains advantage";
                break;

            case ActionOutcome.EnemyAttacksAreDefendedWithDisadvantage:
                currentTarget.disadvantageWhenDefendedAgainstCount++;
                encounterText.text += $"\nDefending against {currentTarget.monsterName} gets disadvantage";
                break;

            case ActionOutcome.EnemyIsAttackedWithAdvantage:
                currentTarget.advantageWhenAttackedCount++;
                encounterText.text += $"\nAttacking {currentTarget.monsterName} gains advantage";
                break;

            case ActionOutcome.EnemyIsAttackedWithDisadvantage:
                currentTarget.disadvantageWhenAttackedCount++;
                encounterText.text += $"\nAttacking {currentTarget.monsterName} gets disadvantage";
                break;

            case ActionOutcome.TauntEnemy:
                currentTarget.isTaunted = true;
                currentTarget.tauntedBy = currentActingPlayer;
                encounterText.text += $"\n{currentTarget.monsterName} is taunted by {currentActingPlayer.characterName}";
                break;

            case ActionOutcome.TauntAllEnemies:
                foreach (var monster in activeMonsters)
                {
                    if (monster != null && monster.IsAlive())
                    {
                        monster.isTaunted = true;
                        monster.tauntedBy = currentActingPlayer;
                    }
                }
                encounterText.text += $"\nAll enemies are taunted by {currentActingPlayer.characterName}";
                break;

            case ActionOutcome.AllyDealsNormalDamage:
                currentTarget.TakeDamage(randomAllyPlayer.damageAmount);
                encounterText.text += $"\n{currentTarget.monsterName} suffers {randomAllyPlayer.damageAmount} damage";
                break;

                // Add more outcomes as needed
        }

        encounterText.text += "\n";
    }

    void ClearTemporaryStatusEffects()
    {
        // Clear single-use status effects
        currentActingPlayer.advantageCount = 0;
        currentActingPlayer.disadvantageCount = 0;
        currentActingPlayer.advantageAttackCount = 0;
        currentActingPlayer.disadvantageAttackCount = 0;

        // Clear used enemy modifiers
        if (currentTarget != null)
        {
            currentTarget.advantageWhenAttackedCount = 0;
            currentTarget.disadvantageWhenAttackedCount = 0;
        }
    }

    void StartMonsterTurn()
    {
        waitingForSpace = false;
        selectingMonster = false;

        // Hide monster list
        monsterListUI.SetActive(false);

        // Find available monsters
        List<Monster> availableMonsters = activeMonsters.Where(m => m.IsAlive() && !m.hasActedThisCycle).ToList();

        // If no available monsters, reset cycle
        if (availableMonsters.Count == 0)
        {
            foreach (var monster in activeMonsters)
            {
                monster.hasActedThisCycle = false;
            }
            availableMonsters = activeMonsters.Where(m => m.IsAlive()).ToList();
        }

        if (availableMonsters.Count == 0)
        {
            EndCombat(true);
            return;
        }

        // Pick random monster
        Monster attackingMonster = availableMonsters[Random.Range(0, availableMonsters.Count)];
        attackingMonster.hasActedThisCycle = true;

        // Update display to show attacking monster
        currentActiveMonsterIndex = activeMonsters.IndexOf(attackingMonster);
        UpdateMonsterDisplay();

        // Pick target based on taunt
        PlayerCharacter target;
        if (attackingMonster.isTaunted && attackingMonster.tauntedBy != null && attackingMonster.tauntedBy.healthPoints > 0)
        {
            target = attackingMonster.tauntedBy;
        }
        else
        {
            List<PlayerCharacter> alivePlayers = partyMembers.Where(p => p != null && p.healthPoints > 0).ToList();
            if (alivePlayers.Count == 0)
            {
                EndCombat(false);
                return;
            }
            target = alivePlayers[Random.Range(0, alivePlayers.Count)];
        } 

        encounterText.text = $"{attackingMonster.monsterName} attacks {target.characterName}. What now?\n\n";
        //\n\nWhat will our {target.className} do?\n\n"; 

        waitingForSpace = true;

        currentActionType = ActionTriggerType.ReactionCombat;

        ShowActionSelection();
        // Check if all players dead
        /*List<PlayerCharacter> remainingPlayers = partyMembers.Where(p => p != null && p.healthPoints > 0).ToList();
        if (remainingPlayers.Count == 0)
        {
            onSpacePressed = () => EndCombat(false);
        }
        else
        {
            onSpacePressed = StartPlayerTurn;
        }*/
    }
    void SpacePressedDuringCombat()
    {
        encounterText.text = encounterText.text.Replace("Press <u>Space</u> to continue ...", "");
        waitingForSpace = false;
    }
    void ContinueCombat()
    {
        StartMonsterTurn();
    }

    void EndCombat(bool playerVictory)
    {
        waitingForSpace = false;
        selectingMonster = false;
        selectingAction = false;
        selectingActionResult = false;
        isInCombat = false;

        monsterListUI.SetActive(false);
        actionListContainer.gameObject.SetActive(false);
        actionResultListContainer.gameObject.SetActive(false);
        encounterImage.gameObject.SetActive(false);

        // Clean up monster instances
        foreach (var monster in activeMonsters)
        {
            if (monster != null) Destroy(monster.gameObject);
        }
        activeMonsters.Clear();

        // Reset all status effects
        foreach (var player in partyMembers)
        {
            if (player != null)
            {
                player.hasActedThisCycle = false;
                player.advantageCount = 0;
                player.disadvantageCount = 0;
                player.advantageAttackCount = 0;
                player.disadvantageAttackCount = 0;
                player.advantageDefenseCount = 0;
                player.disadvantageDefenseCount = 0;
                player.exhaustionDamageLevel = 0;
            }
        }

        if (playerVictory)
        {
            encounterText.text = "You were victorious!\n\nThis time ... Press <u>Space</u> to continue";
            waitingForSpace = true;
            onSpacePressed = ReturnToDungeon;
        }
        else
        {
            encounterText.text = "You meet your fate.\n\nGAME OVER.\n\nPress <u>Space</u> to restart the game.";
            waitingForSpace = true;
            onSpacePressed = RestartGame;
        }
    }

    void ReturnToDungeon()
    {
        waitingForSpace = false;
        selectingMonster = false;
        selectingAction = false;
        selectingActionResult = false;

        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.SetDungeonMode();
        }
    }

    void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
}