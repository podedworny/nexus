using UnityEngine;
using System;

public class InventoryManager : MonoBehaviour
{
    [Header("Starting Items")]
    public ItemData handsItem;
    public ItemData startingMeleeWeapon;

    public ItemData[] hotbarSlots = new ItemData[5];
    public int[] itemCounts = new int[5];
    public Action OnInventoryChanged;
    public Action<int, int> OnItemsSwapped;

    private void Awake()
    {
        hotbarSlots[0] = handsItem;
        itemCounts[0] = 1;

        if (startingMeleeWeapon != null)
        {
            hotbarSlots[1] = startingMeleeWeapon;
            itemCounts[1] = 1;
        }
    }

    private void Start()
    {
        OnInventoryChanged?.Invoke();
    }

    public int AddItem(ItemData item)
    {
        for (int i = 1; i < hotbarSlots.Length; i++)
        {
            if (hotbarSlots[i] == null)
            {
                hotbarSlots[i] = item;
                itemCounts[i] = 1;
                OnInventoryChanged?.Invoke();
                return i;
            }
        }
        return -1;
    }

    public bool TryAddStackableItem(ItemData item, int maxStack)
    {
        for (int i = 1; i < hotbarSlots.Length; i++)
        {
            if (hotbarSlots[i] == item && itemCounts[i] < maxStack)
            {
                itemCounts[i]++;
                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        for (int i = 1; i < hotbarSlots.Length; i++)
        {
            if (hotbarSlots[i] == null)
            {
                hotbarSlots[i] = item;
                itemCounts[i] = 1;
                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        return false;
    }

    public void ConsumeItem(int index)
    {
        if (index > 0 && index < hotbarSlots.Length && hotbarSlots[index] != null)
        {
            itemCounts[index]--;
            if (itemCounts[index] <= 0)
            {
                hotbarSlots[index] = null;
                itemCounts[index] = 0;
            }
            OnInventoryChanged?.Invoke();
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

    public int GetItemCount(int index)
    {
        if (index >= 0 && index < itemCounts.Length)
        {
            return itemCounts[index];
        }
        return 0;
    }

    public int GetItemIndex(ItemData item)
    {
        if (item == null) return -1;
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            if (hotbarSlots[i] == item) return i;
        }
        return -1;
    }

    public void SwapItems(int indexA, int indexB)
    {
        if (indexA == 0 || indexB == 0 || indexA == indexB) return;

        (hotbarSlots[indexA], hotbarSlots[indexB]) = (hotbarSlots[indexB], hotbarSlots[indexA]);
        (itemCounts[indexA], itemCounts[indexB]) = (itemCounts[indexB], itemCounts[indexA]);

        OnItemsSwapped?.Invoke(indexA, indexB);
        OnInventoryChanged?.Invoke();
    }

    public bool HasFreeSlot()
    {
        for (int i = 1; i < hotbarSlots.Length; i++)
        {
            if (hotbarSlots[i] == null) return true;
        }
        return false;
    }

    public bool CanAddStackableItem(ItemData item, int maxStack)
    {
        for (int i = 1; i < hotbarSlots.Length; i++)
        {
            if (hotbarSlots[i] == item && itemCounts[i] < maxStack) return true;
            if (hotbarSlots[i] == null) return true;
        }
        return false;
    }
}
