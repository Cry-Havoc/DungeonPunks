using UnityEngine;
using System;

[System.Serializable]
public class PlayerCharacter : MonoBehaviour
{
    [Header("Character Info")]
    public string characterName = "Hero";
    public string className = "Warrior";
    public Sprite characterPicture;
    public string characterPronoun = "They"; // "He", "She", "They"

    [Header("Primary Attributes (1-100)")]
    [Range(1, 100)] public int force = 50;
    [Range(1, 100)] public int perception = 50;
    [Range(1, 100)] public int reflexe = 50;
    [Range(1, 100)] public int stamina = 50;
    [Range(1, 100)] public int reason = 50;
    [Range(1, 100)] public int willPower = 50;
    [Range(1, 100)] public int heart = 50;

    [Header("Health (1-10)")]
    [Range(1, 10)] public int healthPoints = 10;
    [Range(1, 10)] public int maxHealthPoints = 10;

    // Combat tracking
    [HideInInspector] public bool hasActedThisCycle = false;

    // Event fired when any stat changes
    public event Action OnStatsChanged;

    private int _cachedHP;
    private int _cachedMaxHP;

    // Combat status effects
    [HideInInspector] public int advantageCount = 0;
    [HideInInspector] public int disadvantageCount = 0;
    [HideInInspector] public int advantageAttackCount = 0;
    [HideInInspector] public int disadvantageAttackCount = 0;
    [HideInInspector] public int advantageDefenseCount = 0;
    [HideInInspector] public int disadvantageDefenseCount = 0;
    [HideInInspector] public int exhaustionDamageLevel = 0;
    public int damageAmount = 1; // Default damage

    void Update()
    {
        // Check if health changed in inspector
        if (_cachedHP != healthPoints || _cachedMaxHP != maxHealthPoints)
        {
            _cachedHP = healthPoints;
            _cachedMaxHP = maxHealthPoints;
            TriggerStatsChanged();
        }
    }

    public void SetHealth(int hp, int maxHp)
    {
        healthPoints = Mathf.Clamp(hp, 0, 10);
        maxHealthPoints = Mathf.Clamp(maxHp, 1, 10);
        _cachedHP = healthPoints;
        _cachedMaxHP = maxHealthPoints;
        TriggerStatsChanged();
    }

    public void TakeDamage(int damage)
    {
        healthPoints = Mathf.Max(0, healthPoints - damage);
        _cachedHP = healthPoints;
        TriggerStatsChanged();
    }

    public void Heal(int amount)
    {
        healthPoints = Mathf.Min(maxHealthPoints, healthPoints + amount);
        _cachedHP = healthPoints;
        TriggerStatsChanged();
    }

    public void TriggerStatsChanged()
    {
        OnStatsChanged?.Invoke();
    }

    public string GetStatsText()
    {
        return $"<b>{characterName}</b>\n" +
               $"<i>{className}</i>\n\n" +
               $"<b>ATTRIBUTES</b>\n" +
               $"Force: {force}\n" +
               $"Sense: {perception}\n" +
               $"Haste: {reflexe}\n" +
               $"Stamina: {stamina}\n" +
               $"Reason: {reason}\n" +
               $"Will: {willPower}\n" +
               $"Heart: {heart}\n\n" +
               $"<b>Health</b>\n" +
               $"{healthPoints} / {maxHealthPoints}";
    }
}