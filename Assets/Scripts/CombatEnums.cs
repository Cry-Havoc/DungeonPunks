/// <summary>
/// Player character attributes that can be checked
/// </summary>
public enum PlayerAttribute
{
    FORCE,
    PERCEPTION,
    REFLEXE,
    STAMINA,
    REASON,
    WILLPOWER,
    HEART
}

/// <summary>
/// When a player action can be triggered
/// </summary>
public enum ActionTriggerType
{
    ActiveCombat,
    ReactionCombat
}

/// <summary>
/// Result of a skill check
/// </summary>
public enum ActionResult
{
    CriticalSuccess,
    PartlySuccess,
    PartlyFailure,
    Fumble
}

/// <summary>
/// Conditions that must be met for an action result to apply
/// </summary>
public enum ActionCondition
{
    PlayerHasMeleeWeapon,
    PlayerHasRangedWeapon,
    PlayerCanCastSpell,
    PlayerHasMultipleEnemies,
    PlayerInjured,
    PlayerHealthy,
    EnemyWeakened,
    EnemyStrong,
    AllyNearby,
    PlayerAlone
}

/// <summary>
/// Specific outcomes that can occur from action results
/// </summary>
public enum ActionOutcome
{
    DealNormalDamage,
    DealDoubleDamage,
    DealTripleDamage,
    DealHalfDamage,
    AttackAgain,
    HealSelf,
    StunEnemy,
    KnockbackEnemy,
    TakeDamage,
    LoseHealth,
    SkipNextTurn,
    GainAdvantageNextRoll,
    GainDisadvantageNextRoll,
    DrawEnemyAttention,
    DefendAlly,
    BreakWeapon,
    Nothing
}