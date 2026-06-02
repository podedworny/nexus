using UnityEngine;
using UnityEngine.InputSystem;

public class TentShop : MonoBehaviour
{
    public float interactRange = 4f;

    [SerializeField] private GameObject shopUIContainer;

    [SerializeField] private GameObject bodyChestMesh;
    [SerializeField] private GameObject bodyLegsMesh;
    [SerializeField] private GameObject plateChestMesh;
    [SerializeField] private GameObject platePantsMesh;

    public ConsumableData bandageItem;
    public ItemData pistolItem;
    public ItemData akItem;

    private Transform _playerTransform;
    private InventoryManager _inventoryManager;
    private PlayerStats _playerStats;
    private bool _isShopOpen = false;

    private bool _hasChestArmor = false;
    private bool _hasPantsArmor = false;

    public bool HasChestArmor => _hasChestArmor;
    public bool HasPantsArmor => _hasPantsArmor;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerTransform = player.transform;
            _inventoryManager = player.GetComponent<InventoryManager>();
            _playerStats = player.GetComponent<PlayerStats>();
        }

        if (shopUIContainer != null) shopUIContainer.SetActive(false);
    }

    private void OnEnable()
    {
        if (Nexus.FinalCharacterController.PlayerInputManager.Instance?.PlayerControls != null)
        {
            Nexus.FinalCharacterController.PlayerInputManager.Instance.PlayerControls.PlayerActionMap.Interact.performed += OnWaveStartInput;
            Nexus.FinalCharacterController.PlayerInputManager.Instance.PlayerControls.PlayerActionMap.OpenShop.performed += OnShopInput;
        }
    }

    private void OnDisable()
    {
        if (Nexus.FinalCharacterController.PlayerInputManager.Instance?.PlayerControls != null)
        {
            Nexus.FinalCharacterController.PlayerInputManager.Instance.PlayerControls.PlayerActionMap.Interact.performed -= OnWaveStartInput;
            Nexus.FinalCharacterController.PlayerInputManager.Instance.PlayerControls.PlayerActionMap.OpenShop.performed -= OnShopInput;
        }
    }

    private void OnWaveStartInput(InputAction.CallbackContext context)
    {
        if (_playerTransform == null || WaveManager.Instance == null || _isShopOpen) return;

        float dist = Vector3.Distance(transform.position, _playerTransform.position);
        bool isDay = WaveManager.Instance.currentState == WaveManager.GameState.Day;

        if (dist <= interactRange && isDay)
        {
            WaveManager.Instance.StartWaveTransition();
        }
    }

    private void OnShopInput(InputAction.CallbackContext context)
    {
        if (_playerTransform == null || WaveManager.Instance == null) return;

        float dist = Vector3.Distance(transform.position, _playerTransform.position);
        bool isDay = WaveManager.Instance.currentState == WaveManager.GameState.Day;

        if (dist <= interactRange && isDay)
        {
            ToggleShop();
        }
    }

    private void Update()
    {
        if (_playerTransform == null || WaveManager.Instance == null) return;

        float dist = Vector3.Distance(transform.position, _playerTransform.position);
        bool isDay = WaveManager.Instance.currentState == WaveManager.GameState.Day;

        if (_isShopOpen)
        {
            bool escapePressed = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;

            if (dist > interactRange || !isDay || escapePressed)
            {
                ToggleShop();
            }
        }
    }

    public void ToggleShop()
    {
        _isShopOpen = !_isShopOpen;

        if (shopUIContainer != null)
        {
            shopUIContainer.SetActive(_isShopOpen);
        }

        if (_isShopOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (Nexus.FinalCharacterController.PlayerInputManager.Instance != null)
            {
                Nexus.FinalCharacterController.PlayerInputManager.Instance.PlayerControls.CombatMap.Disable();
                Nexus.FinalCharacterController.PlayerInputManager.Instance.PlayerControls.PlayerLocomotionMap.Disable();
                Nexus.FinalCharacterController.PlayerInputManager.Instance.PlayerControls.ThirdPersonMap.Disable();
                Nexus.FinalCharacterController.PlayerInputManager.Instance.PlayerControls.InventoryMap.Disable();
            }
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (Nexus.FinalCharacterController.PlayerInputManager.Instance != null)
            {
                Nexus.FinalCharacterController.PlayerInputManager.Instance.PlayerControls.CombatMap.Enable();
                Nexus.FinalCharacterController.PlayerInputManager.Instance.PlayerControls.PlayerLocomotionMap.Enable();
                Nexus.FinalCharacterController.PlayerInputManager.Instance.PlayerControls.ThirdPersonMap.Enable();
                Nexus.FinalCharacterController.PlayerInputManager.Instance.PlayerControls.InventoryMap.Enable();
            }
        }
    }

    private void OnGUI()
    {
        if (Cursor.visible) return;

        if (_playerTransform == null || WaveManager.Instance == null) return;

        float dist = Vector3.Distance(transform.position, _playerTransform.position);
        bool isDay = WaveManager.Instance.currentState == WaveManager.GameState.Day;

        if (dist <= interactRange && isDay && !_isShopOpen)
        {
            GUIStyle promptStyle = new GUIStyle(GUI.skin.label);
            promptStyle.fontSize = 24;
            promptStyle.alignment = TextAnchor.MiddleCenter;
            promptStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(0, Screen.height / 2 + 50, Screen.width, 50), "Press [F] to Start Night | Press [Q] for Shop", promptStyle);
        }
    }

    public bool CanReceiveWeapon()
    {
        return _inventoryManager != null && _inventoryManager.HasFreeSlot();
    }

    public bool CanReceiveBandage()
    {
        return _inventoryManager != null && _inventoryManager.CanAddStackableItem(bandageItem, 10);
    }

    public void BuyOrUpgradePistol()
    {
        if (_playerStats == null || _playerTransform == null) return;
        WeaponManager wm = _playerTransform.GetComponent<WeaponManager>();
        if (wm == null || _inventoryManager == null) return;

        int slotIndex = _inventoryManager.GetItemIndex(pistolItem);
        int level = slotIndex != -1 ? wm.GetWeaponLevel(slotIndex) : 0;
        int cost = level == 0 ? 200 : 150;

        if (level == 0 && !CanReceiveWeapon()) return;

        if (level < 3 && _playerStats.currency >= cost)
        {
            if (level == 0 && pistolItem != null)
            {
                slotIndex = _inventoryManager.AddItem(pistolItem);
            }

            if (slotIndex != -1)
            {
                _playerStats.SpendCurrency(cost);
                wm.UpgradeWeapon(slotIndex);
            }
        }
    }

    public void BuyOrUpgradeAK()
    {
        if (_playerStats == null || _playerTransform == null) return;
        WeaponManager wm = _playerTransform.GetComponent<WeaponManager>();
        if (wm == null || _inventoryManager == null) return;

        int slotIndex = _inventoryManager.GetItemIndex(akItem);
        int level = slotIndex != -1 ? wm.GetWeaponLevel(slotIndex) : 0;
        int cost = level == 0 ? 500 : 250;

        if (level == 0 && !CanReceiveWeapon()) return;

        if (level < 3 && _playerStats.currency >= cost)
        {
            if (level == 0 && akItem != null)
            {
                slotIndex = _inventoryManager.AddItem(akItem);
            }

            if (slotIndex != -1)
            {
                _playerStats.SpendCurrency(cost);
                wm.UpgradeWeapon(slotIndex);
            }
        }
    }

    public void BuyChestArmor()
    {
        if (_playerStats == null) return;

        if (!_hasChestArmor && _playerStats.SpendCurrency(300))
        {
            _hasChestArmor = true;

            if (bodyChestMesh != null) bodyChestMesh.SetActive(false);
            if (plateChestMesh != null) plateChestMesh.SetActive(true);

            _playerStats.AddArmor(0.2f);
        }
    }

    public void BuyPantsArmor()
    {
        if (_playerStats == null) return;

        if (!_hasPantsArmor && _playerStats.SpendCurrency(250))
        {
            _hasPantsArmor = true;

            if (bodyLegsMesh != null) bodyLegsMesh.SetActive(false);
            if (platePantsMesh != null) platePantsMesh.SetActive(true);

            _playerStats.AddArmor(0.15f);
        }
    }

    public void BuyBandage()
    {
        if (_playerStats == null || _inventoryManager == null || bandageItem == null) return;

        if (!CanReceiveBandage()) return;

        if (_playerStats.currency >= 75)
        {
            if (_inventoryManager.TryAddStackableItem(bandageItem, 10))
            {
                _playerStats.SpendCurrency(75);
            }
        }
    }

    public void BuyFullHeal()
    {
        if (_playerStats == null) return;
        if (_playerStats.currentHealth >= _playerStats.GetActualMaxHealth()) return;

        if (_playerStats.SpendCurrency(150))
        {
            _playerStats.Heal(9999f);
        }
    }

    public void BuyPistolAmmo()
    {
        if (_playerStats == null || _playerTransform == null) return;
        WeaponManager wm = _playerTransform.GetComponent<WeaponManager>();

        int slotIndex = _inventoryManager.GetItemIndex(pistolItem);
        if (slotIndex != -1 && wm.GetWeaponLevel(slotIndex) > 0 && wm.CanBuyAmmo(slotIndex))
        {
            if (_playerStats.SpendCurrency(50))
            {
                wm.BuyAmmo(slotIndex, 30);
            }
        }
    }

    public void BuyAKAmmo()
    {
        if (_playerStats == null || _playerTransform == null) return;
        WeaponManager wm = _playerTransform.GetComponent<WeaponManager>();

        int slotIndex = _inventoryManager.GetItemIndex(akItem);
        if (slotIndex != -1 && wm.GetWeaponLevel(slotIndex) > 0 && wm.CanBuyAmmo(slotIndex))
        {
            if (_playerStats.SpendCurrency(120))
            {
                wm.BuyAmmo(slotIndex, 90);
            }
        }
    }
}
