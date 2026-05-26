using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Consumable", menuName = "Items/Consumable Data")]
public class ConsumableData : ItemData
{
    public float healAmount = 50f;
    public float useTime = 3.0f;

    public override List<ItemStat> GetStats()
    {
        List<ItemStat> stats = new List<ItemStat>();
        stats.Add(new ItemStat { statName = "HEALING", statValue = healAmount.ToString(), fillPercentage = healAmount / 100f, showBar = true });
        stats.Add(new ItemStat { statName = "USE TIME", statValue = useTime.ToString("F1") + "s", fillPercentage = 0f, showBar = false });
        return stats;
    }
}