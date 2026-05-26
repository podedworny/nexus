using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Melee Weapon", menuName = "Weapons/Melee Weapon Data")]
public class MeleeWeaponData : ItemData
{
    public GameObject weaponPrefab;
    public Vector3 spawnPosition;
    public Vector3 spawnRotation;
    public Vector3 spawnScale = Vector3.one;
    
    public float damage = 30f;
    public float attackRate = 1.0f;
    public float attackRange = 2.5f;
    public float hitRadius = 0.5f;

    public override List<ItemStat> GetStats()
    {
        List<ItemStat> stats = new List<ItemStat>();
        Color accentColor = new Color(0.91f, 0.78f, 0.25f, 1f); 
        Color defaultColor = new Color(0.85f, 0.89f, 0.94f, 0.7f);

        stats.Add(new ItemStat { statName = "DAMAGE", statValue = damage.ToString(), fillPercentage = damage / 100f, showBar = true, barColor = accentColor });
        stats.Add(new ItemStat { statName = "ATTACK RATE", statValue = attackRate.ToString("F1"), fillPercentage = attackRate / 5f, showBar = true, barColor = defaultColor });
        stats.Add(new ItemStat { statName = "RANGE", statValue = attackRange.ToString("F1"), fillPercentage = attackRange / 10f, showBar = true, barColor = defaultColor });
        return stats;
    }
}