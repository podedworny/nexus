using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    [Header("Ammo UI")]
    public TextMeshProUGUI ammoText;

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
        UpdateAmmoDisplay(-1, -1);
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
                    targetColor.a = 1f; 
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

    public void UpdateAmmoDisplay(int currentAmmo, int reserveAmmo)
    {
        if (ammoText == null) return;

        if (currentAmmo < 0) 
        {
            ammoText.text = "";
        }
        else
        {
            ammoText.text = currentAmmo + " / " + reserveAmmo;
        }
    }
}