using UnityEngine;
using System;

public class InventoryManager : MonoBehaviour
{
    [Header("Starting Items")]
    public ItemData handsItem;
    public ItemData startingWeapon;
    public ItemData startingMeleeWeapon;

    public ItemData[] hotbarSlots = new ItemData[5];
    public Action OnInventoryChanged;
    public Action<int, int> OnItemsSwapped;

    private void Awake()
    {
        hotbarSlots[0] = handsItem;
        
        if (startingWeapon != null)
        {
            hotbarSlots[1] = startingWeapon;
        }

        if (startingMeleeWeapon != null)
        {
            hotbarSlots[2] = startingMeleeWeapon;
        }
    }

    private void Start()
    {
        OnInventoryChanged?.Invoke();
    }

    public void AddItem(ItemData item)
    {
        for (int i = 1; i < hotbarSlots.Length; i++)
        {
            if (hotbarSlots[i] == null)
            {
                hotbarSlots[i] = item;
                OnInventoryChanged?.Invoke();
                return;
            }
        }
    }

    public ItemData GetItem(int index)
    {
        if (index >= 0 && index < hotbarSlots.Length)
        {
            return hotbarSlots[index];
        }
        return null;
    }

    public void SwapItems(int indexA, int indexB)
    {
        if (indexA == 0 || indexB == 0 || indexA == indexB) return;

        (hotbarSlots[indexA], hotbarSlots[indexB]) = (hotbarSlots[indexB], hotbarSlots[indexA]);

        OnItemsSwapped?.Invoke(indexA, indexB);
        OnInventoryChanged?.Invoke();
    }
}