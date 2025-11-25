using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }

    [Header("Monster Prefabs")]
    public List<GameObject> monsterPrefabs;

    [Header("UI References")]
    public TextMeshProUGUI encounterText;
    public Image encounterImage;
    public GameObject monsterListUI;
    public GameObject characterButtonPrefab;
    public Transform monsterListContainer;

    private List<Monster> activeMonsters = new List<Monster>();
    private List<CharacterButton> characterButtons = new List<CharacterButton>();
    private PlayerCharacter[] partyMembers;

    private bool isInCombat = false;
    private bool waitingForSpace = false;
    private bool selectingMonster = false;
    private PlayerCharacter currentActingPlayer;
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
        // Only process space if we're waiting and NOT selecting a monster
        if (waitingForSpace && !selectingMonster && Input.GetKeyDown(KeyCode.Space))
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
        encounterText.text = "You are not alone ... press SPACE to continue";
        encounterImage.gameObject.SetActive(false);

        waitingForSpace = true;
        selectingMonster = false;
        onSpacePressed = InitiateCombat;
    }

    void InitiateCombat()
    {
        waitingForSpace = false;
        selectingMonster = false;

        // Switch UI to combat mode
        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.SetCombatMode();
        }

        // Spawn 1-8 monsters
        int monsterCount = Random.Range(1, 9);
        activeMonsters.Clear();

        for (int i = 0; i < monsterCount; i++)
        {
            GameObject prefab = monsterPrefabs[Random.Range(0, monsterPrefabs.Count)];
            Monster monsterCopy = prefab.GetComponent<Monster>().CreateCopy();
            monsterCopy.gameObject.SetActive(false); // Keep in scene but hidden
            activeMonsters.Add(monsterCopy);
        }

        // Show first monster
        currentActiveMonsterIndex = 0;
        UpdateMonsterDisplay();
        CreateCharacterButtons();

        // Hide monster list initially
        monsterListUI.SetActive(false);

        // Start first player turn
        StartPlayerTurn();
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

        encounterText.text = $"{currentActingPlayer.characterName} attacks! Select a monster (1-{activeMonsters.Count}) or click on monster name.";

        // Show monster list and enable selection
        monsterListUI.SetActive(true);
        selectingMonster = true;

        // Enable monster selection
        foreach (var btn in characterButtons)
        {
            btn.SetSelectable(true);
        }
    }

    public void OnMonsterSelected(int monsterIndex)
    {
        if (monsterIndex < 0 || monsterIndex >= activeMonsters.Count) return;
        if (!selectingMonster) return;

        selectingMonster = false;

        // Hide monster list
        monsterListUI.SetActive(false);

        // Disable further selection
        foreach (var btn in characterButtons)
        {
            btn.SetSelectable(false);
        }

        Monster target = activeMonsters[monsterIndex];
        target.TakeDamage(1);

        encounterText.text = $"{currentActingPlayer.characterName} attacks {target.monsterName} for 1 damage! Press SPACE to continue.";

        UpdateCharacterButtons();
        UpdateMonsterDisplay();

        waitingForSpace = true;

        // Check if all monsters dead
        if (activeMonsters.Count == 0)
        {
            onSpacePressed = () => EndCombat(true);
        }
        else
        {
            onSpacePressed = StartMonsterTurn;
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

        // Pick random alive player
        List<PlayerCharacter> alivePlayers = partyMembers.Where(p => p != null && p.healthPoints > 0).ToList();

        if (alivePlayers.Count == 0)
        {
            EndCombat(false);
            return;
        }

        PlayerCharacter target = alivePlayers[Random.Range(0, alivePlayers.Count)];
        target.TakeDamage(1);

        encounterText.text = $"{attackingMonster.monsterName} attacks {target.characterName} for 1 damage! Press SPACE to continue.";

        waitingForSpace = true;

        // Check if all players dead
        List<PlayerCharacter> remainingPlayers = partyMembers.Where(p => p != null && p.healthPoints > 0).ToList();
        if (remainingPlayers.Count == 0)
        {
            onSpacePressed = () => EndCombat(false);
        }
        else
        {
            onSpacePressed = StartPlayerTurn;
        }
    }

    void EndCombat(bool playerVictory)
    {
        waitingForSpace = false;
        selectingMonster = false;
        isInCombat = false;

        monsterListUI.SetActive(false);
        encounterImage.gameObject.SetActive(false);

        // Clean up monster instances
        foreach (var monster in activeMonsters)
        {
            if (monster != null) Destroy(monster.gameObject);
        }
        activeMonsters.Clear();

        if (playerVictory)
        {
            encounterText.text = "You were victorious! This time ... Press Space to continue";
            waitingForSpace = true;
            onSpacePressed = ReturnToDungeon;
        }
        else
        {
            encounterText.text = "You meet your fate. GAME OVER. Press Space to restart the game.";
            waitingForSpace = true;
            onSpacePressed = RestartGame;
        }
    }

    void ReturnToDungeon()
    {
        waitingForSpace = false;
        selectingMonster = false;

        // Reset player acted flags
        foreach (var player in partyMembers)
        {
            if (player != null) player.hasActedThisCycle = false;
        }

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