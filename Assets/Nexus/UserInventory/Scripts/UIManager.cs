using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [Header("Health & Stamina Bars")]
    public Image healthBarFill;
    public Image staminaBarFill;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI staminaText;
    public PlayerStats playerStats;
    public InventoryManager inventoryManager;

    [Header("Lives")]
    public Image[] lifeIcons;
    public Color lifeActiveColor = new Color(0.88f, 0.34f, 0.34f, 1f);
    public Color lifeEmptyColor = new Color(0.88f, 0.34f, 0.34f, 0.2f);

    [Header("Bottom Hotbar Settings")]
    public GameObject[] hotbarSlots;

    [Header("Ammo UI")]
    public TextMeshProUGUI ammoText;
    public Transform ammoPipsContainer;
    public GameObject ammoPipPrefab;
    public Color pipActiveColor = new Color(0.85f, 0.9f, 0.95f, 0.7f);
    public Color pipUsedColor = new Color(0.85f, 0.9f, 0.95f, 0.15f);

    private List<Image> instantiatedPips = new List<Image>();
    private int currentMagCapacity = -1;

    [Header("Game State UI")]
    public TextMeshProUGUI currencyText;
    public TextMeshProUGUI waveText;

    private void OnEnable()
    {
        if (playerStats != null)
        {
            playerStats.OnHealthChanged += UpdateHealthBar;
            playerStats.OnStaminaChanged += UpdateStaminaBar;
            playerStats.OnCurrencyChanged += UpdateCurrencyDisplay;
        }
        if (inventoryManager != null) inventoryManager.OnInventoryChanged += UpdateHotbarIcons;
    }

    private void OnDisable()
    {
        if (playerStats != null)
        {
            playerStats.OnHealthChanged -= UpdateHealthBar;
            playerStats.OnStaminaChanged -= UpdateStaminaBar;
            playerStats.OnCurrencyChanged -= UpdateCurrencyDisplay;
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
            UpdateCurrencyDisplay(playerStats.currency);
        }

        UpdateHotbarIcons();
        UpdateAmmoDisplay(-1, -1, 0);
    }

    private void Update()
    {
        if (GameManager.Instance != null)
        {
            UpdateLivesIcons(GameManager.Instance.currentLives);
        }

        if (WaveManager.Instance != null && waveText != null)
        {
            waveText.text = WaveManager.Instance.currentWave.ToString("D2");
        }
    }

    private void UpdateCurrencyDisplay(int amount)
    {
        if (currencyText != null)
        {
            currencyText.text = "<color=#FACC15>$</color> " + amount;
        }
    }

    public void UpdateActiveSlot(int slotIndex)
    {
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            if (hotbarSlots[i] == null) continue;

            Transform borderObj = hotbarSlots[i].transform.Find("Border");
            if (borderObj != null)
            {
                borderObj.gameObject.SetActive(i == slotIndex);
            }
        }
    }

    public void UpdateHotbarIcons()
    {
        if (inventoryManager == null || hotbarSlots == null) return;

        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            if (hotbarSlots[i] == null || hotbarSlots[i].transform.childCount == 0) continue;

            Transform iconTransform = hotbarSlots[i].transform.Find("Icon");
            if (iconTransform != null)
            {
                Image iconImage = iconTransform.GetComponent<Image>();
                if (iconImage != null)
                {
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

            Transform countTransform = hotbarSlots[i].transform.Find("CountText");
            if (countTransform != null)
            {
                TextMeshProUGUI countText = countTransform.GetComponent<TextMeshProUGUI>();
                if (countText != null)
                {
                    int count = inventoryManager.GetItemCount(i);
                    if (count > 1)
                    {
                        countText.text = count.ToString();
                        countText.gameObject.SetActive(true);
                    }
                    else
                    {
                        countText.gameObject.SetActive(false);
                    }
                }
            }
        }
    }

    private void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthBarFill != null && maxHealth > 0)
            healthBarFill.fillAmount = currentHealth / maxHealth;

        if (healthText != null)
            healthText.text = Mathf.CeilToInt(currentHealth).ToString();
    }

    private void UpdateStaminaBar(float currentStamina, float maxStamina)
    {
        if (staminaBarFill != null && maxStamina > 0)
            staminaBarFill.fillAmount = currentStamina / maxStamina;

        if (staminaText != null)
            staminaText.text = Mathf.CeilToInt(currentStamina).ToString();
    }

    private void UpdateLivesIcons(int currentLives)
    {
        if (lifeIcons == null) return;
        for (int i = 0; i < lifeIcons.Length; i++)
        {
            if (lifeIcons[i] != null)
                lifeIcons[i].color = (i < currentLives) ? lifeActiveColor : lifeEmptyColor;
        }
    }

    public void UpdateAmmoDisplay(int currentAmmo, int reserveAmmo, int magCapacity)
    {
        if (ammoText == null) return;

        if (currentAmmo < 0)
        {
            ammoText.text = "";
            UpdateAmmoPips(0, 0);
            return;
        }

        ammoText.text = currentAmmo + " <color=#6B8EAD><size=60%>/ " + reserveAmmo + "</size></color>";
        UpdateAmmoPips(currentAmmo, magCapacity);
    }

    private void UpdateAmmoPips(int current, int capacity)
    {
        if (ammoPipsContainer == null || ammoPipPrefab == null) return;

        if (capacity != currentMagCapacity)
        {
            foreach (Transform child in ammoPipsContainer)
            {
                Destroy(child.gameObject);
            }
            instantiatedPips.Clear();

            for (int i = 0; i < capacity; i++)
            {
                GameObject newPip = Instantiate(ammoPipPrefab, ammoPipsContainer, false);
                newPip.SetActive(true);

                RectTransform rt = newPip.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.localScale = Vector3.one;
                    rt.localPosition = new Vector3(rt.localPosition.x, rt.localPosition.y, 0f);
                    rt.localRotation = Quaternion.identity;
                }

                LayoutElement le = newPip.GetComponent<LayoutElement>();
                if (le == null) le = newPip.AddComponent<LayoutElement>();
                if (le.preferredWidth <= 0) le.preferredWidth = 15f;
                if (le.preferredHeight <= 0) le.preferredHeight = 5f;

                Image pipImage = newPip.GetComponent<Image>();
                if (pipImage == null) pipImage = newPip.AddComponent<Image>();

                instantiatedPips.Add(pipImage);
            }
            currentMagCapacity = capacity;
        }

        for (int i = 0; i < instantiatedPips.Count; i++)
        {
            instantiatedPips[i].color = (i < current) ? pipActiveColor : pipUsedColor;
        }
    }
}
