using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Nexus.FinalCharacterController;

public class WeaponManager : MonoBehaviour, PlayerControls.ICombatMapActions
{
    [SerializeField] private Transform _weaponSocket;
    [SerializeField] private Animator _animator;
    [SerializeField] private UIManager _uiManager;
    [SerializeField] private InventoryManager _inventoryManager;
    
    public bool naZywoUstawianieBroni = false;
    
    private GameObject _currentWeaponObject;
    private int _currentIndex = -1;
    private bool _isAiming = false;

    public int CurrentWeaponIndex => _currentIndex;

    private void OnEnable()
    {
        if (Nexus.FinalCharacterController.PlayerInputManager.Instance?.PlayerControls == null) return;
        Nexus.FinalCharacterController.PlayerInputManager.Instance.PlayerControls.CombatMap.Enable();
        Nexus.FinalCharacterController.PlayerInputManager.Instance.PlayerControls.CombatMap.SetCallbacks(this);

        if (_inventoryManager != null) _inventoryManager.OnItemsSwapped += HandleItemsSwapped;
    }

    private void OnDisable()
    {
        if (Nexus.FinalCharacterController.PlayerInputManager.Instance?.PlayerControls == null) return;
        Nexus.FinalCharacterController.PlayerInputManager.Instance.PlayerControls.CombatMap.RemoveCallbacks(this);

        if (_inventoryManager != null) _inventoryManager.OnItemsSwapped -= HandleItemsSwapped;
    }

    private void Start()
    {
        UnequipCurrent();
    }

    private void Update()
    {
        if (naZywoUstawianieBroni && _currentWeaponObject != null && _currentIndex >= 0 && _inventoryManager != null)
        {
            ItemData item = _inventoryManager.GetItem(_currentIndex);
            if (item != null && item is WeaponData data)
            {
                _currentWeaponObject.transform.localPosition = data.spawnPosition;
                _currentWeaponObject.transform.localRotation = Quaternion.Euler(data.spawnRotation);
                _currentWeaponObject.transform.localScale = data.spawnScale;
            }
        }
    }

    private void HandleItemsSwapped(int indexA, int indexB)
    {
        if (_currentIndex == indexA)
        {
            _currentIndex = indexB;
            if (_uiManager != null) _uiManager.UpdateActiveSlot(_currentIndex);
        }
        else if (_currentIndex == indexB)
        {
            _currentIndex = indexA;
            if (_uiManager != null) _uiManager.UpdateActiveSlot(_currentIndex);
        }
    }

    public void OnWeapon1(InputAction.CallbackContext context)
    {
        if (context.performed) 
        {
            UnequipCurrent();
            if (_uiManager != null) _uiManager.UpdateActiveSlot(0);
        }
    }

    public void OnWeapon2(InputAction.CallbackContext context)
    {
        if (context.performed && _inventoryManager != null && _inventoryManager.GetItem(1) != null)
        {
            EquipWeapon(1);
            if (_uiManager != null) _uiManager.UpdateActiveSlot(1);
        }
    }

    public void OnWeapon3(InputAction.CallbackContext context)
    {
        if (context.performed && _inventoryManager != null && _inventoryManager.GetItem(2) != null)
        {
            EquipWeapon(2);
            if (_uiManager != null) _uiManager.UpdateActiveSlot(2);
        }
    }

    public void OnWeapon4(InputAction.CallbackContext context)
    {
        if (context.performed && _inventoryManager != null && _inventoryManager.GetItem(3) != null)
        {
            EquipWeapon(3);
            if (_uiManager != null) _uiManager.UpdateActiveSlot(3);
        }
    }

    public void OnWeapon5(InputAction.CallbackContext context)
    {
        if (context.performed && _inventoryManager != null && _inventoryManager.GetItem(4) != null)
        {
            EquipWeapon(4);
            if (_uiManager != null) _uiManager.UpdateActiveSlot(4);
        }
    }

    public void OnAim(InputAction.CallbackContext context)
    {
        if (_currentIndex == -1 || _currentWeaponObject == null) return;

        if (context.started)
        {
            _isAiming = true;
            if (_animator != null) _animator.SetBool("isAiming", true);
        }
        else if (context.canceled)
        {
            _isAiming = false;
            if (_animator != null) _animator.SetBool("isAiming", false);
        }
    }

    private void EquipWeapon(int index)
    {
        if (index == _currentIndex || _inventoryManager == null) return;

        ItemData item = _inventoryManager.GetItem(index);
        if (item == null || !(item is WeaponData data)) return;

        UnequipCurrent();
        _currentIndex = index;

        if (data.weaponPrefab != null)
        {
            _currentWeaponObject = Instantiate(data.weaponPrefab, _weaponSocket);
            _currentWeaponObject.transform.localPosition = data.spawnPosition;
            _currentWeaponObject.transform.localRotation = Quaternion.Euler(data.spawnRotation);
            _currentWeaponObject.transform.localScale = data.spawnScale;
        }

        if (_animator != null) _animator.SetBool("HasWeapon", true);
    }

    private void UnequipCurrent()
    {
        if (_currentWeaponObject != null) Destroy(_currentWeaponObject);
        _currentIndex = 0;
        _isAiming = false;

        if (_animator != null)
        {
            _animator.SetBool("HasWeapon", false);
            _animator.SetBool("isAiming", false);
        }
        if (_uiManager != null) _uiManager.UpdateActiveSlot(0);
    }
}