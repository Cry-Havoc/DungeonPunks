using UnityEngine;

public class Monster : MonoBehaviour
{
    [Header("Monster Info")]
    public string monsterName;
    public Sprite monsterPicture;
    [Range(1, 10)] public int maxHealthPoints = 5;

    [HideInInspector] public int currentHealthPoints;
    [HideInInspector] public bool hasActedThisCycle = false;

    void Awake()
    {
        currentHealthPoints = maxHealthPoints;
    }

    public void ResetForCombat()
    {
        currentHealthPoints = maxHealthPoints;
        hasActedThisCycle = false;
    }

    public bool IsAlive()
    {
        return currentHealthPoints > 0;
    }

    public void TakeDamage(int damage)
    {
        currentHealthPoints = Mathf.Max(0, currentHealthPoints - damage);
    }

    public string GetHealthDisplay()
    {
        string healthDisplay = "";
        for (int i = 0; i < maxHealthPoints; i++)
        {
            if (i < currentHealthPoints)
            {
                healthDisplay += "■";
            }
            else
            {
                healthDisplay += "□";
            }
        }
        return healthDisplay;
    }

    public Monster CreateCopy()
    {
        GameObject copy = Instantiate(gameObject);
        Monster monsterCopy = copy.GetComponent<Monster>();
        monsterCopy.ResetForCombat();
        return monsterCopy;
    }
}