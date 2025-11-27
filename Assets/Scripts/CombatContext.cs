using UnityEngine;

/// <summary>
/// Contains all context information needed to evaluate combat conditions
/// </summary>
public class CombatContext
{
    public PlayerCharacter actingPlayer;
    public Monster targetEnemy;
    public int enemyCount;
    public int aliveAllyCount;
    public PlayerAction currentAction;
    public ActionResult actionResult;

    public CombatContext(PlayerCharacter player, Monster target, int enemies, int allies)
    {
        actingPlayer = player;
        targetEnemy = target;
        enemyCount = enemies;
        aliveAllyCount = allies;
    }
}