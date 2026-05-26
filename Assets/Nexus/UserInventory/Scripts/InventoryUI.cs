using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
using Nexus.FinalCharacterController;
using System.Collections.Generic;

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

    [Header("Item Info UI")]
    [SerializeField] private TextMeshProUGUI _itemNameText;
    [SerializeField] private TextMeshProUGUI _itemTypeText;
    [SerializeField] private TextMeshProUGUI _itemDescriptionText;

    [Header("Dynamic Stats UI")]
    [SerializeField] private GameObject _statRowPrefab;
    [SerializeField] private Transform _statsContainer;

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
        ShowItemStats(index);
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

    private void ShowItemStats(int slotIndex)
    {
        if (_statsContainer != null)
        {
            foreach (Transform child in _statsContainer)
            {
                child.gameObject.SetActive(false);
                Destroy(child.gameObject);
            }
        }

        if (_inventoryManager == null) return;
        ItemData item = _inventoryManager.GetItem(slotIndex);

        if (item != null)
        {
            if (_itemNameText != null) _itemNameText.text = item.itemName.ToUpper();
            if (_itemDescriptionText != null) _itemDescriptionText.text = item.description;
            
            if (_itemTypeText != null) 
            {
                if (item is WeaponData) _itemTypeText.text = "WEAPON";
                else if (item is MeleeWeaponData) _itemTypeText.text = "MELEE";
                else _itemTypeText.text = "CONSUMABLE";
            }

            if (_statRowPrefab != null && _statsContainer != null)
            {
                List<ItemStat> stats = item.GetStats();
                for (int i = 0; i < stats.Count; i++)
                {
                    GameObject rowObj = Instantiate(_statRowPrefab, _statsContainer);
                    StatRowUI rowUI = rowObj.GetComponent<StatRowUI>();
                    if (rowUI != null) rowUI.Setup(stats[i]);

                    if (i < stats.Count - 1)
                    {
                        GameObject sep = new GameObject("StatSeparator", typeof(RectTransform), typeof(UnityEngine.UI.Image));
                        sep.transform.SetParent(_statsContainer, false);
                        UnityEngine.UI.Image sepImg = sep.GetComponent<UnityEngine.UI.Image>();
                        sepImg.color = new Color(0.35f, 0.47f, 0.63f, 0.4f);
                        UnityEngine.UI.LayoutElement le = sep.AddComponent<UnityEngine.UI.LayoutElement>();
                        le.minHeight = 1;
                        le.preferredHeight = 1;
                        le.flexibleWidth = 1;
                    }
                }

                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(_statsContainer.GetComponent<RectTransform>());
                if (_statsContainer.parent != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(_statsContainer.parent.GetComponent<RectTransform>());
                }
            }
        }
        else
        {
            if (_itemNameText != null) _itemNameText.text = "EMPTY";
            if (_itemTypeText != null) _itemTypeText.text = "";
            if (_itemDescriptionText != null) _itemDescriptionText.text = "-";
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