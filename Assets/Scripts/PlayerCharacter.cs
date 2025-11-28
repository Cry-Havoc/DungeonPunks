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

    public int exhaustionDamageLevel { get;  set; }
    public int disadvantageDefenseCount { get;  set; }
    public int advantageDefenseCount { get;  set; }
    public int disadvantageAttackCount { get;  set; }
    public int advantageAttackCount { get;  set; }
    public int disadvantageCount { get;  set; }
    public int advantageCount { get;  set; }
    public int damageAmount { get; internal set; }

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
               $"FORCE: {force}\n" +
               $"PERCEPTION: {perception}\n" +
               $"REFLEXE: {reflexe}\n" +
               $"STAMINA: {stamina}\n" +
               $"REASON: {reason}\n" +
               $"WILL POWER: {willPower}\n" +
               $"HEART: {heart}\n\n" +
               $"<b>HEALTH</b>\n" +
               $"HP: {healthPoints} / {maxHealthPoints}";
    }
}