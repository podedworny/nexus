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
    private int[] _weaponLevels = new int[5];

    private PlayerController _playerController;
    private PlayerState _playerState;
    private PlayerStats _playerStats;
    private PlayerLocomotionInput _locomotionInput;

    private bool _isConsuming = false;
    private float _consumableTimer = 0f;

    public int CurrentWeaponIndex => _currentIndex;
    public bool IsReloading => _isReloading;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _playerState = GetComponent<PlayerState>();
        _playerStats = GetComponent<PlayerStats>();
        _locomotionInput = GetComponent<PlayerLocomotionInput>();
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
            ItemData currentItem = _inventoryManager.GetItem(_currentIndex);
            if (currentItem != null)
            {
                if (currentItem is WeaponData weaponData)
                {
                    _currentWeaponObject.transform.localPosition = weaponData.spawnPosition;
                    _currentWeaponObject.transform.localRotation = Quaternion.Euler(weaponData.spawnRotation);
                    _currentWeaponObject.transform.localScale = weaponData.spawnScale;
                }
                else if (currentItem is MeleeWeaponData meleeWeaponData)
                {
                    _currentWeaponObject.transform.localPosition = meleeWeaponData.spawnPosition;
                    _currentWeaponObject.transform.localRotation = Quaternion.Euler(meleeWeaponData.spawnRotation);
                    _currentWeaponObject.transform.localScale = meleeWeaponData.spawnScale;
                }
            }
        }

        if (_firstPersonAimCamera != null)
        {
            if (_isAimingInput && !isSprinting && !_isReloading && _inventoryManager != null && _currentIndex > 0)
            {
                ItemData currentItem = _inventoryManager.GetItem(_currentIndex);
                if (currentItem is WeaponData weaponData)
                {
                    _firstPersonAimCamera.localPosition = Vector3.Lerp(_firstPersonAimCamera.localPosition, weaponData.cameraAimOffset, Time.deltaTime * cameraTransitionSpeed);
                    _firstPersonAimCamera.localRotation = Quaternion.Lerp(_firstPersonAimCamera.localRotation, Quaternion.Euler(weaponData.cameraAimRotation), Time.deltaTime * cameraTransitionSpeed);
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

        if (_currentIndex <= 0 || _inventoryManager == null) return;

        ItemData currentItem = _inventoryManager.GetItem(_currentIndex);

        if (currentItem is ConsumableData consumableData)
        {
            if (_pendingShot && !_isConsuming)
            {
                _pendingShot = false;
                _hasFiredThisClick = true;

                if (_playerStats != null && _playerStats.currentHealth < _playerStats.GetActualMaxHealth())
                {
                    _isConsuming = true;
                    _consumableTimer = 0f;

                    if (_animator != null)
                    {
                        _animator.SetBool("HasWeapon", true);
                        _animator.SetInteger("WeaponType", -1);
                        _animator.SetTrigger("UseConsumable");
                        _animator.SetBool("isUsingConsumable", true);
                    }
                }
            }

            if (_isConsuming)
            {
                if (_locomotionInput != null && _locomotionInput.MovementInput != Vector2.zero)
                {
                    CancelConsumable();
                    return;
                }

                _consumableTimer += Time.deltaTime;

                if (_consumableTimer >= consumableData.useTime)
                {
                    _playerStats.Heal(consumableData.healAmount);
                    _inventoryManager.ConsumeItem(_currentIndex);
                    CancelConsumable();

                    if (_inventoryManager.GetItem(_currentIndex) == null)
                    {
                        UnequipCurrent();
                    }
                }
            }
            return;
        }

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

    private void CancelConsumable()
    {
        _isConsuming = false;
        _consumableTimer = 0f;
        if (_animator != null)
        {
            _animator.SetBool("isUsingConsumable", false);
            _animator.SetBool("HasWeapon", false);
            _animator.SetInteger("WeaponType", 0);
        }
    }

    private void Shoot()
    {
        if (Time.time < _nextFireTime || _isReloading) return;
        ItemData currentItem = _inventoryManager.GetItem(_currentIndex);

        if (currentItem is WeaponData weaponData)
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

            int level = _weaponLevels[_currentIndex];
            float bonusMultiplier = level > 1 ? (level - 1) * 0.3f : 0f;
            actualDamage *= (1f + bonusMultiplier);

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
        else if (currentItem is MeleeWeaponData meleeWeaponData)
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
            _nextFireTime = Time.time + Mathf.Max(0.01f, meleeWeaponData.attackRate);
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
        if (_currentIndex <= 0 || _inventoryManager == null) return;
        ItemData currentItem = _inventoryManager.GetItem(_currentIndex);
        if (!(currentItem is MeleeWeaponData meleeWeapon)) return;

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
        if (_inventoryManager == null) return;
        ItemData currentItem = _inventoryManager.GetItem(_currentIndex);
        if (!(currentItem is WeaponData weaponData) || _ammoInSlots[_currentIndex] == weaponData.magazineSize || _reserveAmmo[_currentIndex] <= 0) return;

        _isReloading = true;
        _reloadTimer = weaponData.reloadTime;
        if (_animator != null) _animator.SetTrigger("Reload");
        _animator.SetBool("isReloading", true);
    }

    private void FinishReload()
    {
        _isReloading = false;
        if (_animator != null) _animator.SetBool("isReloading", false);

        if (_inventoryManager == null) return;
        ItemData currentItem = _inventoryManager.GetItem(_currentIndex);
        if (currentItem is WeaponData weaponData)
        {
            int ammoToLoad = Mathf.Min(weaponData.magazineSize - _ammoInSlots[_currentIndex], _reserveAmmo[_currentIndex]);
            _ammoInSlots[_currentIndex] += ammoToLoad;
            _reserveAmmo[_currentIndex] -= ammoToLoad;
            UpdateUIAmmo();
        }
    }

    public bool CanBuyAmmo(int index)
    {
        if (index > 0 && index < _reserveAmmo.Length && _inventoryManager != null)
        {
            ItemData currentItem = _inventoryManager.GetItem(index);
            if (currentItem is WeaponData weaponData)
            {
                return _reserveAmmo[index] < weaponData.maxReserveAmmo;
            }
        }
        return false;
    }

    public void BuyAmmo(int index, int amount)
    {
        if (index > 0 && index < _reserveAmmo.Length && _inventoryManager != null)
        {
            ItemData currentItem = _inventoryManager.GetItem(index);
            if (currentItem is WeaponData weaponData)
            {
                _reserveAmmo[index] += amount;
                if (_reserveAmmo[index] > weaponData.maxReserveAmmo)
                {
                    _reserveAmmo[index] = weaponData.maxReserveAmmo;
                }
                if (index == _currentIndex) UpdateUIAmmo();
            }
        }
    }

    public int GetWeaponLevel(int index)
    {
        if (index >= 0 && index < _weaponLevels.Length) return _weaponLevels[index];
        return 0;
    }

    public void UpgradeWeapon(int index)
    {
        if (index >= 0 && index < _weaponLevels.Length) _weaponLevels[index]++;
    }

    private void UpdateUIAmmo()
    {
        if (_uiManager != null && _inventoryManager != null)
        {
            ItemData currentItem = _inventoryManager.GetItem(_currentIndex);
            if (currentItem is WeaponData weaponData)
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
        int tempAmmo = _ammoInSlots[indexA];
        _ammoInSlots[indexA] = _ammoInSlots[indexB];
        _ammoInSlots[indexB] = tempAmmo;

        int tempRes = _reserveAmmo[indexA];
        _reserveAmmo[indexA] = _reserveAmmo[indexB];
        _reserveAmmo[indexB] = tempRes;

        bool tempInit = _initializedAmmo[indexA];
        _initializedAmmo[indexA] = _initializedAmmo[indexB];
        _initializedAmmo[indexB] = tempInit;

        int tempLvl = _weaponLevels[indexA];
        _weaponLevels[indexA] = _weaponLevels[indexB];
        _weaponLevels[indexB] = tempLvl;

        if (_currentIndex == indexA) { _currentIndex = indexB; if (_uiManager != null) _uiManager.UpdateActiveSlot(_currentIndex); }
        else if (_currentIndex == indexB) { _currentIndex = indexA; if (_uiManager != null) _uiManager.UpdateActiveSlot(_currentIndex); }
    }

    public void OnWeapon1(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        UnequipCurrent();
        if (_uiManager != null) _uiManager.UpdateActiveSlot(0);
    }

    public void OnWeapon2(InputAction.CallbackContext context) => TryEquipSlot(context, 1);
    public void OnWeapon3(InputAction.CallbackContext context) => TryEquipSlot(context, 2);
    public void OnWeapon4(InputAction.CallbackContext context) => TryEquipSlot(context, 3);
    public void OnWeapon5(InputAction.CallbackContext context) => TryEquipSlot(context, 4);

    public void OnAim(InputAction.CallbackContext context)
    {
        if (_currentIndex <= 0 || _inventoryManager == null) return;
        ItemData currentItem = _inventoryManager.GetItem(_currentIndex);
        if (currentItem is MeleeWeaponData || currentItem is ConsumableData) return;
        if (_currentWeaponObject == null) return;

        if (context.started) _isAimingInput = true;
        else if (context.canceled) _isAimingInput = false;
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (_currentIndex <= 0 || _inventoryManager == null) return;
        ItemData currentItem = _inventoryManager.GetItem(_currentIndex);

        if (!(currentItem is ConsumableData) && _currentWeaponObject == null) return;

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

    public void OnReload(InputAction.CallbackContext context)
    {
        if (_currentIndex <= 0 || _inventoryManager == null) return;
        ItemData currentItem = _inventoryManager.GetItem(_currentIndex);
        if (!(currentItem is ConsumableData) && _currentWeaponObject == null) return;

        if (context.performed && !_isReloading) StartReload();
    }

    private void TryEquipSlot(InputAction.CallbackContext context, int slotIndex)
    {
        if (!context.performed || _inventoryManager == null || _inventoryManager.GetItem(slotIndex) == null) return;

        EquipWeapon(slotIndex);
        if (_uiManager != null) _uiManager.UpdateActiveSlot(slotIndex);
    }

    private void EquipWeapon(int index)
    {
        if (index == _currentIndex || _inventoryManager == null) return;
        ItemData selectedItem = _inventoryManager.GetItem(index);
        if (selectedItem == null) return;

        UnequipCurrent();
        _currentIndex = index;

        if (selectedItem is WeaponData weaponData)
        {
            if (!_initializedAmmo[index]) { _ammoInSlots[index] = weaponData.magazineSize; _reserveAmmo[index] = weaponData.maxReserveAmmo; _initializedAmmo[index] = true; }
            if (weaponData.weaponPrefab != null)
            {
                _currentWeaponObject = Instantiate(weaponData.weaponPrefab, _weaponSocket);
                _currentWeaponObject.transform.localPosition = weaponData.spawnPosition;
                _currentWeaponObject.transform.localRotation = Quaternion.Euler(weaponData.spawnRotation);
                _currentWeaponObject.transform.localScale = weaponData.spawnScale;
            }
            if (_animator != null) {
                _animator.SetBool("HasWeapon", true);
                _animator.SetInteger("WeaponType", weaponData.animationWeaponType);
                CurrentAnimWeaponType = weaponData.animationWeaponType;
            }
        }
        else if (selectedItem is MeleeWeaponData meleeWeaponData)
        {
            if (meleeWeaponData.weaponPrefab != null)
            {
                _currentWeaponObject = Instantiate(meleeWeaponData.weaponPrefab, _weaponSocket);
                _currentWeaponObject.transform.localPosition = meleeWeaponData.spawnPosition;
                _currentWeaponObject.transform.localRotation = Quaternion.Euler(meleeWeaponData.spawnRotation);
                _currentWeaponObject.transform.localScale = meleeWeaponData.spawnScale;
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
        CancelConsumable();
        if (_playerController != null) _playerController.IsFiring = false;
        if (_animator != null) {
            _animator.SetBool("HasWeapon", false);
            _animator.SetBool("isAiming", false);
            _animator.SetInteger("WeaponType", 0);
            _animator.SetBool("isReloading", false);
            _animator.SetBool("isUsingConsumable", false);
        }
        if (_uiManager != null) { _uiManager.UpdateActiveSlot(0); _uiManager.UpdateAmmoDisplay(-1, -1, 0); }
    }

    private void OnGUI()
    {
        if (_isConsuming)
        {
            float useTime = 1f;
            if (_inventoryManager != null && _inventoryManager.GetItem(_currentIndex) is ConsumableData cd)
            {
                useTime = cd.useTime;
            }

            float progress = Mathf.Clamp01(_consumableTimer / useTime);

            float width = 200f;
            float height = 20f;
            float x = (Screen.width - width) / 2f;
            float y = Screen.height / 2f + 50f;

            Rect bgRect = new Rect(x, y, width, height);
            Rect fgRect = new Rect(x, y, width * progress, height);

            Texture2D tex = Texture2D.whiteTexture;

            GUI.color = new Color(0.08f, 0.12f, 0.19f, 0.9f);
            GUI.DrawTexture(bgRect, tex);

            GUI.color = new Color(0.91f, 0.78f, 0.25f, 1f);
            GUI.DrawTexture(fgRect, tex);

            GUI.color = Color.white;
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
                fontStyle = FontStyle.Bold
            };
            GUI.Label(bgRect, "HEALING...", style);
        }
    }
}
