using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapons/Weapon Data")]
public class WeaponData : ItemData
{
    public GameObject weaponPrefab;
    public Vector3 spawnPosition;
    public Vector3 spawnRotation;
    public Vector3 spawnScale = Vector3.one;
    public Vector3 cameraAimOffset;
    
    public float damage = 20f;
    public float fireRate = 0.2f; 
    public float range = 100f;
    
    [Header("Ammo & Firing")]
    public bool isAutomatic = false;
    public int magazineSize = 15;
    public int maxReserveAmmo = 90;
    public float reloadTime = 1.5f;
    
    public string description;
}