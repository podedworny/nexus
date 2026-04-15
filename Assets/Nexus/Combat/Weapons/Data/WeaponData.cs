using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public GameObject weaponPrefab;
    public Vector3 spawnPosition;
    public Vector3 spawnRotation;
    public Vector3 spawnScale = Vector3.one;
}