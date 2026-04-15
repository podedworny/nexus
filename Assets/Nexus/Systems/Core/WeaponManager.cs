using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Nexus.FinalCharacterController;

public class WeaponManager : MonoBehaviour, PlayerControls.ICombatMapActions
{
    [SerializeField] private Transform _weaponSocket;
    [SerializeField] private List<WeaponData> _availableWeapons;
    [SerializeField] private Animator _animator;
    
    public bool naZywoUstawianieBroni = false;
    
    private GameObject _currentWeaponObject;
    private int _currentIndex = -1;
    private bool _isAiming = false;

    private void OnEnable()
    {
        if (Nexus.FinalCharacterController.PlayerInputManager.Instance?.PlayerControls == null) return;
        Nexus.FinalCharacterController.PlayerInputManager.Instance.PlayerControls.CombatMap.Enable();
        Nexus.FinalCharacterController.PlayerInputManager.Instance.PlayerControls.CombatMap.SetCallbacks(this);
    }

    private void OnDisable()
    {
        if (Nexus.FinalCharacterController.PlayerInputManager.Instance?.PlayerControls == null) return;
        Nexus.FinalCharacterController.PlayerInputManager.Instance.PlayerControls.CombatMap.RemoveCallbacks(this);
    }

    private void Start()
    {
        if (_availableWeapons.Count > 0)
        {
            EquipWeapon(0);
        }
    }

    private void Update()
    {
        if (naZywoUstawianieBroni && _currentWeaponObject != null && _currentIndex >= 0)
        {
            WeaponData data = _availableWeapons[_currentIndex];
            _currentWeaponObject.transform.localPosition = data.spawnPosition;
            _currentWeaponObject.transform.localRotation = Quaternion.Euler(data.spawnRotation);
            _currentWeaponObject.transform.localScale = data.spawnScale;
        }
    }

    public void OnWeapon1(InputAction.CallbackContext context)
    {
        if (context.performed) EquipWeapon(0);
    }

    public void OnWeapon2(InputAction.CallbackContext context)
    {
        if (context.performed) EquipWeapon(1);
    }

    public void OnWeapon3(InputAction.CallbackContext context)
    {
        if (context.performed) UnequipCurrent();
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
        if (index < 0 || index >= _availableWeapons.Count || index == _currentIndex) return;

        UnequipCurrent();

        _currentIndex = index;
        WeaponData data = _availableWeapons[index];

        _currentWeaponObject = Instantiate(data.weaponPrefab, _weaponSocket);
        _currentWeaponObject.transform.localPosition = data.spawnPosition;
        _currentWeaponObject.transform.localRotation = Quaternion.Euler(data.spawnRotation);
        _currentWeaponObject.transform.localScale = data.spawnScale;

        if (_animator != null)
        {
            _animator.SetBool("HasWeapon", true);
        }
    }

    private void UnequipCurrent()
    {
        if (_currentWeaponObject != null)
        {
            Destroy(_currentWeaponObject);
        }
        
        _currentIndex = -1;
        _isAiming = false;

        if (_animator != null)
        {
            _animator.SetBool("HasWeapon", false);
            _animator.SetBool("isAiming", false);
        }
    }
}