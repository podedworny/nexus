using UnityEngine;
using System.Collections.Generic;

public struct ItemStat
{
    public string statName;
    public string statValue;
    public float fillPercentage;
    public bool showBar;
    public Color barColor;
}

[CreateAssetMenu(fileName = "ItemData", menuName = "Items/ItemData")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public string description;

    public virtual List<ItemStat> GetStats()
    {
        return new List<ItemStat>();
    }
}