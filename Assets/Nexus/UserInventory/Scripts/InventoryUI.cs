using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
using Nexus.FinalCharacterController;

public class InventoryUI : MonoBehaviour, PlayerControls.IInventoryMapActions
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private InventoryManager _inventoryManager;
    [SerializeField] private WeaponManager _weaponManager;
    [SerializeField] private GameObject[] _inventorySlots;

    [Header("Selection Colors")]
    public Color selectedBgColor = new Color(0.91f, 0.78f, 0.25f, 0.15f);
    public Color normalBgColor = new Color(0.08f, 0.12f, 0.19f, 0.7f);
    public Color selectedBorderColor = new Color(0.91f, 0.78f, 0.25f, 1f);
    public Color normalBorderColor = new Color(0.35f, 0.47f, 0.63f, 0.35f);

    [Header("Slot Border Images")]
    [SerializeField] private Image[] _slotBorderImages;

    [Header("Weapon Stats UI")]
    [SerializeField] private TextMeshProUGUI _weaponNameText;
    [SerializeField] private TextMeshProUGUI _weaponTypeText;
    [SerializeField] private TextMeshProUGUI _weaponDescriptionText;
    [SerializeField] private TextMeshProUGUI _weaponDamageText;
    [SerializeField] private TextMeshProUGUI _weaponFireRateText;

    [Header("Stat Bars")]
    [SerializeField] private Image _damageBarFill;
    [SerializeField] private Image _fireRateBarFill;
    [SerializeField] private Image _rangeBarFill;

    [Header("Stat Bar Max Values")]
    [SerializeField] private float _maxDamage = 200f;
    [SerializeField] private float _maxFireRate = 20f;

    private bool _isOpen = false;
    private int _selectedSlotIndex = 0;

    private void Awake()
    {
        for (int i = 0; i < _inventorySlots.Length; i++)
        {
            if (_inventorySlots[i] != null)
            {
                InventorySlotUI slotUI = _inventorySlots[i].GetComponent<InventorySlotUI>();
                if (slotUI == null) slotUI = _inventorySlots[i].AddComponent<InventorySlotUI>();

                slotUI.slotIndex = i;
                slotUI.inventoryUI = this;
            }
        }
    }

    private void OnEnable()
    {
        if (Nexus.FinalCharacterController.PlayerInputManager.Instance?.PlayerControls == null) return;
        Nexus.FinalCharacterController.PlayerInputManager.Instance.PlayerControls.InventoryMap.Enable();
        Nexus.FinalCharacterController.PlayerInputManager.Instance.PlayerControls.InventoryMap.SetCallbacks(this);

        if (_inventoryManager != null) _inventoryManager.OnInventoryChanged += UpdateInventoryIcons;
    }

    private void OnDisable()
    {
        if (Nexus.FinalCharacterController.PlayerInputManager.Instance?.PlayerControls == null) return;
        Nexus.FinalCharacterController.PlayerInputManager.Instance.PlayerControls.InventoryMap.RemoveCallbacks(this);

        if (_inventoryManager != null) _inventoryManager.OnInventoryChanged -= UpdateInventoryIcons;
    }

    public void OnToggleInventory(InputAction.CallbackContext context)
    {
        if (context.performed) ToggleInventory();
    }

    private void ToggleInventory()
    {
        _isOpen = !_isOpen;
        _panel.SetActive(_isOpen);

        if (_isOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            Nexus.FinalCharacterController.PlayerInputManager.Instance.PlayerControls.CombatMap.Disable();
            Nexus.FinalCharacterController.PlayerInputManager.Instance.PlayerControls.PlayerLocomotionMap.Disable();
            Nexus.FinalCharacterController.PlayerInputManager.Instance.PlayerControls.ThirdPersonMap.Disable();
            Nexus.FinalCharacterController.PlayerInputManager.Instance.PlayerControls.PlayerActionMap.Disable();

            _selectedSlotIndex = _weaponManager != null ? _weaponManager.CurrentWeaponIndex : 0;
            SelectSlot(_selectedSlotIndex);
            UpdateInventoryIcons();
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            Nexus.FinalCharacterController.PlayerInputManager.Instance.PlayerControls.CombatMap.Enable();
            Nexus.FinalCharacterController.PlayerInputManager.Instance.PlayerControls.PlayerLocomotionMap.Enable();
            Nexus.FinalCharacterController.PlayerInputManager.Instance.PlayerControls.ThirdPersonMap.Enable();
            Nexus.FinalCharacterController.PlayerInputManager.Instance.PlayerControls.PlayerActionMap.Enable();
        }
    }

    public void SelectSlot(int index)
    {
        if (_inventoryManager == null) return;

        ItemData item = _inventoryManager.GetItem(index);
        if (item == null) return;

        _selectedSlotIndex = index;
        UpdateSelectionUI();
        ShowWeaponStats(index);
    }

    private void UpdateSelectionUI()
    {
        for (int i = 0; i < _inventorySlots.Length; i++)
        {
            if (_inventorySlots[i] == null) continue;

            Image bg = _inventorySlots[i].GetComponent<Image>();
            if (bg != null)
                bg.color = (i == _selectedSlotIndex) ? selectedBgColor : normalBgColor;

            if (_slotBorderImages != null && i < _slotBorderImages.Length && _slotBorderImages[i] != null)
                _slotBorderImages[i].color = (i == _selectedSlotIndex) ? selectedBorderColor : normalBorderColor;
        }
    }

    private void ShowWeaponStats(int slotIndex)
    {
        if (_inventoryManager == null) return;
        ItemData item = _inventoryManager.GetItem(slotIndex);

        if (item is WeaponData weapon)
        {
            if (_weaponNameText != null) _weaponNameText.text = weapon.itemName.ToUpper();
            if (_weaponTypeText != null) _weaponTypeText.text = "semi-auto · sidearm";
            if (_weaponDescriptionText != null) _weaponDescriptionText.text = weapon.description;
            if (_weaponDamageText != null) _weaponDamageText.text = weapon.damage.ToString();
            if (_weaponFireRateText != null) _weaponFireRateText.text = weapon.fireRate.ToString("F1");

            if (_damageBarFill != null)
                _damageBarFill.fillAmount = Mathf.Clamp01(weapon.damage / _maxDamage);

            if (_fireRateBarFill != null)
                _fireRateBarFill.fillAmount = Mathf.Clamp01(weapon.fireRate / _maxFireRate);

            if (_rangeBarFill != null)
                _rangeBarFill.fillAmount = 0.48f;
        }
        else
        {
            if (_weaponNameText != null) _weaponNameText.text = item != null ? item.itemName.ToUpper() : "EMPTY";
            if (_weaponTypeText != null) _weaponTypeText.text = "";
            if (_weaponDescriptionText != null) _weaponDescriptionText.text = "-";
            if (_weaponDamageText != null) _weaponDamageText.text = "";
            if (_weaponFireRateText != null) _weaponFireRateText.text = "";

            if (_damageBarFill != null) _damageBarFill.fillAmount = 0f;
            if (_fireRateBarFill != null) _fireRateBarFill.fillAmount = 0f;
            if (_rangeBarFill != null) _rangeBarFill.fillAmount = 0f;
        }
    }

    public void RequestSwap(int indexA, int indexB)
    {
        if (_inventoryManager != null) _inventoryManager.SwapItems(indexA, indexB);
        SelectSlot(indexB);
    }

    private void UpdateInventoryIcons()
    {
        if (_inventoryManager == null || _inventorySlots == null) return;

        for (int i = 0; i < _inventorySlots.Length; i++)
        {
            if (_inventorySlots[i] == null || _inventorySlots[i].transform.childCount == 0) continue;
            
            Transform iconTransform = _inventorySlots[i].transform.Find("Icon");
            if (iconTransform == null) continue;

            Image iconImage = iconTransform.GetComponent<Image>();
            if (iconImage == null) continue;

            ItemData item = _inventoryManager.GetItem(i);

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
}