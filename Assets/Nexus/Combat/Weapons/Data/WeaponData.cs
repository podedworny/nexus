using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapons/Weapon Data")]
public class WeaponData : ItemData
{
    public GameObject weaponPrefab;
    public Vector3 spawnPosition;
    public Vector3 spawnRotation;
    public Vector3 spawnScale = Vector3.one;
    
    public float damage;
    public float fireRate;
    public string description;
}