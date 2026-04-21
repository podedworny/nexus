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
    public Color selectedColor = Color.yellow;
    public Color normalColor = new Color(0.5f, 0.5f, 0.5f, 1f);

    [Header("Weapon Stats UI")]
    [SerializeField] private TextMeshProUGUI _weaponNameText;
    [SerializeField] private TextMeshProUGUI _weaponDescriptionText;
    [SerializeField] private TextMeshProUGUI _weaponDamageText;
    [SerializeField] private TextMeshProUGUI _weaponFireRateText;

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
            if (_inventorySlots[i] != null)
            {
                Image bg = _inventorySlots[i].GetComponent<Image>();
                if (bg != null) bg.color = (i == _selectedSlotIndex) ? selectedColor : normalColor;
            }
        }
    }

    private void ShowWeaponStats(int slotIndex)
    {
        if (_inventoryManager == null) return;
        ItemData item = _inventoryManager.GetItem(slotIndex);
        
        if (item is WeaponData weapon)
        {
            _weaponNameText.text = weapon.itemName;
            _weaponDescriptionText.text = weapon.description;
            _weaponDamageText.text = "Damage: " + weapon.damage;
            _weaponFireRateText.text = "Fire Rate: " + weapon.fireRate;
        }
        else
        {
            _weaponNameText.text = "Empty Slot";
            _weaponDescriptionText.text = "-";
            _weaponDamageText.text = "";
            _weaponFireRateText.text = "";
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
            Image iconImage = _inventorySlots[i].transform.GetChild(0).GetComponent<Image>();
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