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
        Color accentColor = new Color(0.91f, 0.78f, 0.25f, 1f);
        Color defaultColor = new Color(0.85f, 0.89f, 0.94f, 0.7f);

        stats.Add(new ItemStat { statName = "HEALING", statValue = healAmount.ToString(), fillPercentage = healAmount / 100f, showBar = true, barColor = accentColor });
        stats.Add(new ItemStat { statName = "USE TIME", statValue = useTime.ToString("F1"), fillPercentage = useTime / 10f, showBar = true, barColor = defaultColor });
        return stats;
    }
}
