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
    [SerializeField] private Transform _firstPersonAimCamera;
    
    public bool naZywoUstawianieBroni = false;
    public float cameraTransitionSpeed = 15f;
    public LineRenderer bulletTrailPrefab;
    public float combatReadyDuration = 2.0f; 
    
    private GameObject _currentWeaponObject;
    private int _currentIndex = -1;
    private bool _isAimingInput = false;
    private bool _isShooting = false;
    private bool _hasFiredThisClick = false;
    private bool _pendingShot = false;
    private Vector3 _defaultCameraLocalPos;
    private Quaternion _defaultCameraLocalRot;

    public int CurrentAnimWeaponType { get; private set; } = 0;
    public bool IsShootingIntent => _isShooting || _pendingShot;
    private float _nextFireTime = 0f;
    private bool _isReloading = false;
    private float _reloadTimer = 0f;
    private float _combatReadyTimer = 0f;
    private int[] _ammoInSlots = new int[5];
    private int[] _reserveAmmo = new int[5];
    private bool[] _initializedAmmo = new bool[5];

    private PlayerController _playerController;
    private PlayerState _playerState;

    public int CurrentWeaponIndex => _currentIndex;
    public bool IsReloading => _isReloading;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _playerState = GetComponent<PlayerState>();
    }

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
        if (_firstPersonAimCamera != null) _defaultCameraLocalPos = _firstPersonAimCamera.localPosition;
        if (_firstPersonAimCamera != null) _defaultCameraLocalRot = _firstPersonAimCamera.localRotation;
        UnequipCurrent();
    }

    private void Update()
    {
        if (_combatReadyTimer > 0f) _combatReadyTimer -= Time.deltaTime;

        bool isSprinting = _playerState != null && _playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;
        bool bodyAimState = (_isAimingInput && !isSprinting && !_isReloading) || _combatReadyTimer > 0f;

        if (_animator != null && _currentIndex > 0) 
        {
            if (_animator.GetBool("isAiming") != bodyAimState) _animator.SetBool("isAiming", bodyAimState);
        }

        if (_playerController != null) _playerController.IsAiming = _isAimingInput && !isSprinting && !_isReloading;
        if (_playerController != null) _playerController.IsFiring = _combatReadyTimer > 0f;

        if (naZywoUstawianieBroni && _currentWeaponObject != null && _currentIndex >= 0 && _inventoryManager != null)
        {
            ItemData item = _inventoryManager.GetItem(_currentIndex);
            if (item != null)
            {
                if (item is WeaponData data)
                {
                    _currentWeaponObject.transform.localPosition = data.spawnPosition;
                    _currentWeaponObject.transform.localRotation = Quaternion.Euler(data.spawnRotation);
                    _currentWeaponObject.transform.localScale = data.spawnScale;
                }
                else if (item is MeleeWeaponData meleeData)
                {
                    _currentWeaponObject.transform.localPosition = meleeData.spawnPosition;
                    _currentWeaponObject.transform.localRotation = Quaternion.Euler(meleeData.spawnRotation);
                    _currentWeaponObject.transform.localScale = meleeData.spawnScale;
                }
            }
        }

        if (_firstPersonAimCamera != null)
        {
            if (_isAimingInput && !isSprinting && !_isReloading && _inventoryManager != null && _currentIndex > 0)
            {
                ItemData item = _inventoryManager.GetItem(_currentIndex);
                if (item is WeaponData data)
                {
                    _firstPersonAimCamera.localPosition = Vector3.Lerp(_firstPersonAimCamera.localPosition, data.cameraAimOffset, Time.deltaTime * cameraTransitionSpeed);
                    _firstPersonAimCamera.localRotation = Quaternion.Lerp(_firstPersonAimCamera.localRotation, Quaternion.Euler(data.cameraAimRotation), Time.deltaTime * cameraTransitionSpeed);
                }
            }
            else
            {
                _firstPersonAimCamera.localPosition = Vector3.Lerp(_firstPersonAimCamera.localPosition, _defaultCameraLocalPos, Time.deltaTime * cameraTransitionSpeed);
                _firstPersonAimCamera.localRotation = Quaternion.Lerp(_firstPersonAimCamera.localRotation, _defaultCameraLocalRot, Time.deltaTime * cameraTransitionSpeed);
            }
        }

        HandleCombatLogic();
    }

    private void HandleCombatLogic()
    {
        if (_animator != null && _animator.GetBool("isAttacking") && Time.time >= _nextFireTime)
            _animator.SetBool("isAttacking", false);
        if (_currentIndex <= 0) return;
        if (_isReloading)
        {
            _reloadTimer -= Time.deltaTime;
            if (_reloadTimer <= 0) FinishReload();
            return;
        }
        if (_isShooting || _pendingShot)
        {
            if (_playerController != null && !_playerController.IsAlignedForShooting) return;
            Shoot();
        }
    }

    private void Shoot()
    {
        if (Time.time < _nextFireTime || _isReloading) return;
        ItemData item = _inventoryManager.GetItem(_currentIndex);

        if (item is WeaponData weaponData)
        {
            if (!weaponData.isAutomatic && _hasFiredThisClick)
            {
                _pendingShot = false;
                return;
            }
            if (_ammoInSlots[_currentIndex] <= 0)
            {
                _pendingShot = false;
                if (_reserveAmmo[_currentIndex] > 0) StartReload();
                return;
            }
            
            _ammoInSlots[_currentIndex]--;
            _hasFiredThisClick = true;
            _pendingShot = false;
            _combatReadyTimer = combatReadyDuration;
            if (_animator != null) _animator.SetTrigger("Shoot");
            if (_playerController != null)
            {
                _playerController.AddRecoil(weaponData.recoilVertical, weaponData.recoilHorizontal);
            }
            _nextFireTime = Time.time + Mathf.Max(0.01f, weaponData.fireRate); 
            UpdateUIAmmo();
            
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            Vector3 hitPoint;

            float actualDamage = weaponData.damage;
            if (GameManager.Instance != null) actualDamage *= GameManager.Instance.damageMultiplier;

            if (Physics.Raycast(ray, out RaycastHit hit, weaponData.range))
            {
                hitPoint = hit.point;
                
                ZombieHealth zombie = hit.collider.GetComponentInParent<ZombieHealth>();
                if (zombie != null)
                {
                    zombie.TakeDamage(actualDamage);
                }
            }
            else
            {
                hitPoint = ray.GetPoint(weaponData.range);
            }

            Transform muzzle = GetMuzzle();
            if (muzzle != null && bulletTrailPrefab != null)
            {
                LineRenderer trail = Instantiate(bulletTrailPrefab);
                trail.SetPosition(0, muzzle.position);
                trail.SetPosition(1, hitPoint);
                Destroy(trail.gameObject, 0.05f);
            }
            
            if (_animator != null)
            {
                AnimationClip recoilClip = GetAnimClipByName("AK47_Fire");
                if (recoilClip != null)
                    _animator.SetFloat("RecoilSpeed", recoilClip.length / Mathf.Max(0.01f, weaponData.fireRate));
            
                _animator.SetTrigger("Shoot");
            }
        }
        else if (item is MeleeWeaponData meleeData)
        {
            if (_hasFiredThisClick)
            {
                _pendingShot = false;
                return;
            }
            
            _hasFiredThisClick = true;
            _pendingShot = false;
            _combatReadyTimer = combatReadyDuration;
            if (_animator != null) _animator.SetTrigger("MeleeAttack");
            _animator.SetBool("isAttacking", true);
            _nextFireTime = Time.time + Mathf.Max(0.01f, meleeData.attackRate);
        }
    }
    
    private AnimationClip GetAnimClipByName(string clipName)
    {
        foreach (AnimationClip clip in _animator.runtimeAnimatorController.animationClips)
            if (clip.name == clipName) return clip;
        return null;
    }

    public void PerformMeleeStrike()
    {
        if (_currentIndex <= 0) { return; }
        ItemData item = _inventoryManager.GetItem(_currentIndex);
        if (!(item is MeleeWeaponData meleeWeapon)) { return; }

        float actualDamage = meleeWeapon.damage;
        if (GameManager.Instance != null) actualDamage *= GameManager.Instance.damageMultiplier;

        Vector3 strikeOrigin = transform.position + Vector3.up * 1f + transform.forward * (meleeWeapon.attackRange * 0.5f);
        Collider[] hits = Physics.OverlapSphere(strikeOrigin, meleeWeapon.hitRadius);
        foreach (Collider col in hits)
        {
            ZombieHealth zombie = col.GetComponentInParent<ZombieHealth>();
            if (zombie != null) zombie.TakeDamage(actualDamage);
        }
    }

    private Transform GetMuzzle()
    {
        if (_currentWeaponObject == null) return null;
        foreach (Transform child in _currentWeaponObject.GetComponentsInChildren<Transform>())
            if (child.name.Equals("Muzzle")) return child;
        return _currentWeaponObject.transform;
    }

    private void StartReload()
    {
        ItemData item = _inventoryManager.GetItem(_currentIndex);
        if (!(item is WeaponData weaponData) || _ammoInSlots[_currentIndex] == weaponData.magazineSize || _reserveAmmo[_currentIndex] <= 0) return;
        _isReloading = true;
        _reloadTimer = weaponData.reloadTime;
        if (_animator != null) _animator.SetTrigger("Reload");
        _animator.SetBool("isReloading", true);
    }

    private void FinishReload()
    {
        _isReloading = false;
        _animator.SetBool("isReloading", false);
        ItemData item = _inventoryManager.GetItem(_currentIndex);
        if (item is WeaponData weaponData)
        {
            int ammoToLoad = Mathf.Min(weaponData.magazineSize - _ammoInSlots[_currentIndex], _reserveAmmo[_currentIndex]);
            _ammoInSlots[_currentIndex] += ammoToLoad;
            _reserveAmmo[_currentIndex] -= ammoToLoad;
            UpdateUIAmmo();
        }
    }

    public void BuyAmmo()
    {
        if (_currentIndex > 0 && _inventoryManager != null)
        {
            ItemData item = _inventoryManager.GetItem(_currentIndex);
            if (item is WeaponData weaponData)
            {
                _reserveAmmo[_currentIndex] += weaponData.magazineSize * 3;
                UpdateUIAmmo();
            }
        }
    }

    private void UpdateUIAmmo() 
    { 
        if (_uiManager != null) 
        {
            ItemData item = _inventoryManager.GetItem(_currentIndex);
            if (item is WeaponData weaponData)
            {
                _uiManager.UpdateAmmoDisplay(_ammoInSlots[_currentIndex], _reserveAmmo[_currentIndex], weaponData.magazineSize);
            }
            else
            {
                _uiManager.UpdateAmmoDisplay(-1, -1, 0);
            }
        }
    }

    private void HandleItemsSwapped(int indexA, int indexB)
    {
        if (_currentIndex == indexA) { _currentIndex = indexB; if (_uiManager != null) _uiManager.UpdateActiveSlot(_currentIndex); }
        else if (_currentIndex == indexB) { _currentIndex = indexA; if (_uiManager != null) _uiManager.UpdateActiveSlot(_currentIndex); }
    }

    public void OnWeapon1(InputAction.CallbackContext context) { if (context.performed) { UnequipCurrent(); if (_uiManager != null) _uiManager.UpdateActiveSlot(0); } }
    public void OnWeapon2(InputAction.CallbackContext context) { if (context.performed && _inventoryManager != null && _inventoryManager.GetItem(1) != null) { EquipWeapon(1); if (_uiManager != null) _uiManager.UpdateActiveSlot(1); } }
    public void OnWeapon3(InputAction.CallbackContext context) { if (context.performed && _inventoryManager != null && _inventoryManager.GetItem(2) != null) { EquipWeapon(2); if (_uiManager != null) _uiManager.UpdateActiveSlot(2); } }
    public void OnWeapon4(InputAction.CallbackContext context) { if (context.performed && _inventoryManager != null && _inventoryManager.GetItem(3) != null) { EquipWeapon(3); if (_uiManager != null) _uiManager.UpdateActiveSlot(3); } }
    public void OnWeapon5(InputAction.CallbackContext context) { if (context.performed && _inventoryManager != null && _inventoryManager.GetItem(4) != null) { EquipWeapon(4); if (_uiManager != null) _uiManager.UpdateActiveSlot(4); } }

    public void OnAim(InputAction.CallbackContext context)
    {
        if (_currentIndex <= 0 || _currentWeaponObject == null) return;
        ItemData item = _inventoryManager.GetItem(_currentIndex);
        if (item is MeleeWeaponData) return;

        if (context.started) _isAimingInput = true;
        else if (context.canceled) _isAimingInput = false;
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (_currentIndex <= 0 || _currentWeaponObject == null) return;
        if (context.started) 
        { 
            _isShooting = true; 
            _hasFiredThisClick = false; 
            _pendingShot = true;
        }
        else if (context.canceled) 
        {
            _isShooting = false;
        }
    }

    public void OnReload(InputAction.CallbackContext context) { if (_currentIndex <= 0 || _currentWeaponObject == null) return; if (context.performed && !_isReloading) StartReload(); }

    private void EquipWeapon(int index)
    {
        if (index == _currentIndex || _inventoryManager == null) return;
        ItemData item = _inventoryManager.GetItem(index);
        if (item == null) return;
        UnequipCurrent();
        _currentIndex = index;
        
        if (item is WeaponData data)
        {
            if (!_initializedAmmo[index]) { _ammoInSlots[index] = data.magazineSize; _reserveAmmo[index] = data.maxReserveAmmo; _initializedAmmo[index] = true; }
            if (data.weaponPrefab != null)
            {
                _currentWeaponObject = Instantiate(data.weaponPrefab, _weaponSocket);
                _currentWeaponObject.transform.localPosition = data.spawnPosition;
                _currentWeaponObject.transform.localRotation = Quaternion.Euler(data.spawnRotation);
                _currentWeaponObject.transform.localScale = data.spawnScale;
            }
            if (_animator != null) { 
                _animator.SetBool("HasWeapon", true); 
                _animator.SetInteger("WeaponType", data.animationWeaponType);
                CurrentAnimWeaponType = data.animationWeaponType;
            }
        }
        else if (item is MeleeWeaponData meleeData)
        {
            if (meleeData.weaponPrefab != null)
            {
                _currentWeaponObject = Instantiate(meleeData.weaponPrefab, _weaponSocket);
                _currentWeaponObject.transform.localPosition = meleeData.spawnPosition;
                _currentWeaponObject.transform.localRotation = Quaternion.Euler(meleeData.spawnRotation);
                _currentWeaponObject.transform.localScale = meleeData.spawnScale;
            }
            if (_animator != null) { 
                _animator.SetBool("HasWeapon", true); 
                _animator.SetInteger("WeaponType", 2);
                CurrentAnimWeaponType = 2;
            }
        }
        UpdateUIAmmo();
    }

    private void UnequipCurrent()
    {
        CurrentAnimWeaponType = 0;
        if (_currentWeaponObject != null) Destroy(_currentWeaponObject);
        _currentIndex = 0; _isAimingInput = false; _isShooting = false; _pendingShot = false; _isReloading = false; _combatReadyTimer = 0f;
        if (_playerController != null) _playerController.IsFiring = false;
        if (_animator != null) { _animator.SetBool("HasWeapon", false); _animator.SetBool("isAiming", false); _animator.SetInteger("WeaponType", 0); _animator.SetBool("isReloading", false);}
        if (_uiManager != null) { _uiManager.UpdateActiveSlot(0); _uiManager.UpdateAmmoDisplay(-1, -1, 0); }
    }
}