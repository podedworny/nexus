using UnityEngine;

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
    public string description;
}