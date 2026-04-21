using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Slider healthSlider;
    public Slider staminaSlider;
    public PlayerStats playerStats;
    public InventoryManager inventoryManager;

    [Header("Bottom Hotbar Settings")]
    public GameObject[] hotbarSlots;
    public Color activeColor = Color.yellow;
    public Color inactiveColor = Color.gray;

    private void OnEnable()
    {
        if (playerStats != null)
        {
            playerStats.OnHealthChanged += UpdateHealthBar;
            playerStats.OnStaminaChanged += UpdateStaminaBar;
        }
        if (inventoryManager != null) inventoryManager.OnInventoryChanged += UpdateHotbarIcons;
    }

    private void OnDisable()
    {
        if (playerStats != null)
        {
            playerStats.OnHealthChanged -= UpdateHealthBar;
            playerStats.OnStaminaChanged -= UpdateStaminaBar;
        }
        if (inventoryManager != null) inventoryManager.OnInventoryChanged -= UpdateHotbarIcons;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        UpdateActiveSlot(0);

        if (playerStats != null)
        {
            UpdateHealthBar(playerStats.currentHealth, playerStats.maxHealth);
            UpdateStaminaBar(playerStats.currentStamina, playerStats.maxStamina);
        }

        UpdateHotbarIcons();
    }

    public void UpdateActiveSlot(int slotIndex)
    {
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            if (hotbarSlots[i] != null)
            {
                Image bg = hotbarSlots[i].GetComponent<Image>();
                if (bg != null) 
                {
                    Color targetColor = (i == slotIndex) ? activeColor : inactiveColor;
                    targetColor.a = 1f; // Wymuszamy pełną widoczność tła
                    bg.color = targetColor;
                }
            }
        }
    }

    public void UpdateHotbarIcons()
    {
        if (inventoryManager == null || hotbarSlots == null) return;

        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            if (hotbarSlots[i] == null || hotbarSlots[i].transform.childCount == 0) continue;
            
            Image iconImage = hotbarSlots[i].transform.GetChild(0).GetComponent<Image>();
            if (iconImage == null) continue;

            ItemData item = inventoryManager.GetItem(i);
            
            if (item != null && item.icon != null)
            {
                iconImage.sprite = item.icon;
                iconImage.color = new Color(1, 1, 1, 1);
            }
            else
            {
                iconImage.sprite = null;
                iconImage.color = new Color(1, 1, 1, 0);
            }
        }
    }

    private void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    private void UpdateStaminaBar(float currentStamina, float maxStamina)
    {
        if (staminaSlider != null)
        {
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = currentStamina;
        }
    }
}