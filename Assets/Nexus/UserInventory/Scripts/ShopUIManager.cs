using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUIManager : MonoBehaviour
{
    public TentShop tentShop;
    public WeaponManager weaponManager;
    public PlayerStats playerStats;

    [Header("UI Text References - Weapons")]
    public TextMeshProUGUI balanceText;
    public TextMeshProUGUI pistolLevelText;
    public TextMeshProUGUI pistolPriceText;
    public TextMeshProUGUI akLevelText;
    public TextMeshProUGUI akPriceText;

    [Header("UI Text References - Items")]
    public TextMeshProUGUI chestArmorPriceText;
    public TextMeshProUGUI pantsArmorPriceText;
    public TextMeshProUGUI pistolAmmoPriceText;
    public TextMeshProUGUI akAmmoPriceText;
    public TextMeshProUGUI bandagePriceText;
    public TextMeshProUGUI fullHealPriceText;

    [Header("Button References")]
    public Button pistolButton;
    public Button akButton;
    public Button chestArmorButton;
    public Button pantsArmorButton;
    public Button pistolAmmoButton;
    public Button akAmmoButton;
    public Button bandageButton;
    public Button fullHealButton;

    private void OnEnable()
    {
        UpdateShopUI();

        ShopButtonHover[] buttons = GetComponentsInChildren<ShopButtonHover>(true);
        foreach (var btn in buttons)
        {
            btn.SnapToCurrentState();
        }

        if (chestArmorPriceText != null) chestArmorPriceText.text = "$400";
        if (pantsArmorPriceText != null) pantsArmorPriceText.text = "$250";
        if (pistolAmmoPriceText != null) pistolAmmoPriceText.text = "$60";
        if (akAmmoPriceText != null) akAmmoPriceText.text = "$150";
        if (bandagePriceText != null) bandagePriceText.text = "$50";
        if (fullHealPriceText != null) fullHealPriceText.text = "$200";
    }

    private void Update()
    {
        UpdateShopUI();
    }

    private void UpdateShopUI()
    {
        int currentCurrency = playerStats != null ? playerStats.currency : 0;

        if (balanceText != null)
        {
            balanceText.text = "BALANCE: $" + currentCurrency.ToString();
        }

        bool canReceiveWeapon = tentShop != null && tentShop.CanReceiveWeapon();
        bool canReceiveBandage = tentShop != null && tentShop.CanReceiveBandage();

        InventoryManager invManager = playerStats != null ? playerStats.GetComponent<InventoryManager>() : null;

        if (weaponManager != null && invManager != null && tentShop != null)
        {
            int pSlot = invManager.GetItemIndex(tentShop.pistolItem);
            int pLevel = pSlot != -1 ? weaponManager.GetWeaponLevel(pSlot) : 0;

            if (pistolLevelText != null && pistolPriceText != null && pistolButton != null)
            {
                pistolLevelText.text = "LVL: " + pLevel + "/3";
                int pCost = pLevel == 0 ? 250 : 150;
                pistolPriceText.text = pLevel < 3 ? pCost + "$" : "MAX";

                bool hasSpace = pLevel > 0 || canReceiveWeapon;
                pistolButton.interactable = (pLevel < 3 && currentCurrency >= pCost && hasSpace);
            }

            int aSlot = invManager.GetItemIndex(tentShop.akItem);
            int aLevel = aSlot != -1 ? weaponManager.GetWeaponLevel(aSlot) : 0;

            if (akLevelText != null && akPriceText != null && akButton != null)
            {
                akLevelText.text = "LVL: " + aLevel + "/3";
                int aCost = aLevel == 0 ? 800 : 400;
                akPriceText.text = aLevel < 3 ? aCost + "$" : "MAX";

                bool hasSpace = aLevel > 0 || canReceiveWeapon;
                akButton.interactable = (aLevel < 3 && currentCurrency >= aCost && hasSpace);
            }

            if (pistolAmmoButton != null)
                pistolAmmoButton.interactable = (pLevel > 0 && currentCurrency >= 60 && weaponManager.CanBuyAmmo(pSlot));

            if (akAmmoButton != null)
                akAmmoButton.interactable = (aLevel > 0 && currentCurrency >= 150 && weaponManager.CanBuyAmmo(aSlot));
        }

        if (bandageButton != null)
            bandageButton.interactable = (currentCurrency >= 50 && canReceiveBandage);

        if (fullHealButton != null)
        {
            bool canHeal = playerStats != null && playerStats.currentHealth < playerStats.GetActualMaxHealth();
            fullHealButton.interactable = (currentCurrency >= 200 && canHeal);
        }

        if (tentShop != null)
        {
            if (chestArmorButton != null)
                chestArmorButton.interactable = (!tentShop.HasChestArmor && currentCurrency >= 400);

            if (pantsArmorButton != null)
                pantsArmorButton.interactable = (!tentShop.HasPantsArmor && currentCurrency >= 250);
        }
    }
}
