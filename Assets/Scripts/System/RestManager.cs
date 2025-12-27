using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RestManager : MonoBehaviour
{
    public static RestManager Instance { get; private set; }

    [Header("Rest Settings")]
    public Sprite campfireSprite;
    public int healPerSecond = 1;

    [Header("References")]
    private PlayerCharacter[] partyMembers;
    
    private int currentDay = 1;
    private bool isResting = false;
    private bool waitingForRestInput = false;
    private bool hasOfferedRestToday = false;

    public bool IsResting => isResting;
    public bool WaitingForRestInput => waitingForRestInput;
    public int CurrentDay => currentDay;

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

    public void Initialize(PlayerCharacter[] party)
    {
        partyMembers = party;
    }

    void Update()
    {
        // Only process Return key if waiting for rest input and not in combat
        if (waitingForRestInput && 
            !isResting && 
            CombatManager.Instance != null && 
            !CombatManager.Instance.IsInCombat &&
            Input.GetKeyDown(KeyCode.Return))
        {
            StartRest();
        }
    }

    /// <summary>
    /// Offers the player a chance to rest (called after movement without encounter)
    /// </summary>
    public void OfferRest()
    {
        // Only offer rest once per movement
        if (hasOfferedRestToday || isResting || CombatManager.Instance.IsInCombat)
            return;

        hasOfferedRestToday = true;
        waitingForRestInput = true;

        // Update UI
        if (GameUIManager.Instance != null && GameUIManager.Instance.encounterText != null)
        {
            GameUIManager.Instance.encounterText.text = $"Day {currentDay} - Press <u>Return</u> to make camp";
        }
    }

    /// <summary>
    /// Clears the rest offer without resting
    /// </summary>
    public void ClearRestOffer()
    {
        /*if (waitingForRestInput && !isResting)
        {
            waitingForRestInput = false;
            
            if (GameUIManager.Instance != null && GameUIManager.Instance.encounterText != null)
            {
                GameUIManager.Instance.encounterText.text = "";
            }
        }*/     
    }

    /// <summary>
    /// Starts the resting sequence
    /// </summary>
    void StartRest()
    {
        if (isResting || CombatManager.Instance.IsInCombat)
            return;

        isResting = true;
        waitingForRestInput = false;

        // Disable player movement
        if (PlayerPartyController.Instance != null)
        {
            PlayerPartyController.Instance.enabled = false;
        }

        // Set rest mode in UI
        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.SetRestMode(campfireSprite);
        }

        StartCoroutine(RestSequence());
    }

    /// <summary>
    /// Handles the resting and healing process
    /// </summary>
    IEnumerator RestSequence()
    {
        // Display initial message
        if (GameUIManager.Instance != null && GameUIManager.Instance.encounterText != null)
        {
            GameUIManager.Instance.encounterText.text = "You are resting...";
        }

        // Wait a moment before starting healing
        yield return new WaitForSeconds(1f);

        // Heal all party members gradually
        bool anyoneNeedsHealing = true;

        while (anyoneNeedsHealing)
        {
            anyoneNeedsHealing = false;

            // Heal each party member
            foreach (var player in partyMembers)
            {
                if (player != null && player.healthPoints < player.maxHealthPoints)
                {
                    player.Heal(healPerSecond);
                    anyoneNeedsHealing = true;
                }
            }

            // Wait one second before next heal
            if (anyoneNeedsHealing)
            {
                yield return new WaitForSeconds(1f);
            }
        }

        // All players fully healed
        yield return new WaitForSeconds(0.5f);

        EndRest();
    }

    /// <summary>
    /// Ends the rest period and advances the day
    /// </summary>
    void EndRest()
    {
        isResting = false;
        currentDay++;
        hasOfferedRestToday = false;

        // Update monster spawn states based on new day
        if (MonsterSpawnManager.Instance != null)
        {
            MonsterSpawnManager.Instance.UpdateSpawnStatesForDay(currentDay);
            
            // Get any announcements about new monsters
            List<string> announcements = MonsterSpawnManager.Instance.GetAndClearAnnouncements();
            
            if (announcements.Count > 0)
            {
                // Show announcements
                StartCoroutine(ShowMonsterAnnouncements(announcements));
                return; // Don't return to dungeon yet
            }
        }

        // If no announcements, return to dungeon immediately
        ReturnToDungeon();
    }

    /// <summary>
    /// Shows monster spawn announcements one at a time
    /// </summary>
    IEnumerator ShowMonsterAnnouncements(List<string> announcements)
    {
        foreach (string announcement in announcements)
        {
            if (GameUIManager.Instance != null && GameUIManager.Instance.encounterText != null)
            {
                GameUIManager.Instance.encounterText.text = announcement + "\n\nPress <u>Space</u> to continue...";
            }

            // Wait for player to press space
            bool spacePressed = false;
            while (!spacePressed)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    spacePressed = true;
                }
                yield return null;
            }

            yield return new WaitForSeconds(0.2f); // Brief pause between announcements
        }

        // All announcements shown, return to dungeon
        ReturnToDungeon();
    }

    /// <summary>
    /// Returns player to dungeon mode after rest
    /// </summary>
    void ReturnToDungeon()
    {
        // Clear UI
        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.SetDungeonMode();
            
            if (GameUIManager.Instance.encounterText != null)
            {
                GameUIManager.Instance.encounterText.text = "";
            }
        }

        // Re-enable player movement
        if (PlayerPartyController.Instance != null)
        {
            PlayerPartyController.Instance.enabled = true;
        }
    }

    /// <summary>
    /// Resets the daily rest offer flag (called after each movement)
    /// </summary>
    public void ResetDailyRestOffer()
    {
        hasOfferedRestToday = false;
    }

    /// <summary>
    /// Checks if any party member needs healing
    /// </summary>
    public bool AnyPartyMemberNeedsHealing()
    {
        if (partyMembers == null)
            return false;

        return partyMembers.Any(p => p != null && p.healthPoints < p.maxHealthPoints);
    }
}
