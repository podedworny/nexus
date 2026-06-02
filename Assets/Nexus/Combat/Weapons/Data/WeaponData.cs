using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapons/Weapon Data")]
public class WeaponData : ItemData
{
    public GameObject weaponPrefab;
    public Vector3 spawnPosition;
    public Vector3 spawnRotation;
    public Vector3 spawnScale = Vector3.one;
    public Vector3 cameraAimOffset;
    public Vector3 cameraAimRotation;

    public float damage = 20f;
    public float fireRate = 0.2f;
    public float range = 100f;

    [Header("Ammo & Firing")]
    public bool isAutomatic = false;
    public int magazineSize = 15;
    public int maxReserveAmmo = 90;
    public float reloadTime = 1.5f;
    public float recoilVertical = 1f;
    public float recoilHorizontal = 0.3f;

    public int animationWeaponType = 1;

    public override List<ItemStat> GetStats()
    {
        List<ItemStat> stats = new List<ItemStat>();
        Color accentColor = new Color(0.91f, 0.78f, 0.25f, 1f);
        Color defaultColor = new Color(0.85f, 0.89f, 0.94f, 0.7f);

        stats.Add(new ItemStat { statName = "DAMAGE", statValue = damage.ToString(), fillPercentage = damage / 100f, showBar = true, barColor = accentColor });
        stats.Add(new ItemStat { statName = "FIRE RATE", statValue = fireRate.ToString("F1"), fillPercentage = fireRate / 3f, showBar = true, barColor = defaultColor });
        stats.Add(new ItemStat { statName = "RANGE", statValue = range.ToString(), fillPercentage = range / 200f, showBar = true, barColor = defaultColor });
        return stats;
    }
}
