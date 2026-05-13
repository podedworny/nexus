using UnityEngine;
using UnityEngine.InputSystem;

public class TentShop : MonoBehaviour
{
    public float interactRange = 4f;
    private Transform _playerTransform;
    private bool _isShopOpen = false;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) _playerTransform = player.transform;
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

        if ((dist > interactRange || !isDay) && _isShopOpen)
        {
            ToggleShop();
        }
    }

    private void ToggleShop()
    {
        _isShopOpen = !_isShopOpen;
        if (_isShopOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            if (Nexus.FinalCharacterController.PlayerInputManager.Instance != null) 
            {
                Nexus.FinalCharacterController.PlayerInputManager.Instance.PlayerControls.CombatMap.Disable();
                Nexus.FinalCharacterController.PlayerInputManager.Instance.PlayerControls.PlayerLocomotionMap.Disable();
                Nexus.FinalCharacterController.PlayerInputManager.Instance.PlayerControls.PlayerActionMap.Disable();
                Nexus.FinalCharacterController.PlayerInputManager.Instance.PlayerControls.ThirdPersonMap.Disable();
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
                Nexus.FinalCharacterController.PlayerInputManager.Instance.PlayerControls.PlayerActionMap.Enable();
                Nexus.FinalCharacterController.PlayerInputManager.Instance.PlayerControls.ThirdPersonMap.Enable();
            }
        }
    }

    private void OnGUI()
    {
        if (_playerTransform == null || WaveManager.Instance == null) return;

        float dist = Vector3.Distance(transform.position, _playerTransform.position);
        bool isDay = WaveManager.Instance.currentState == WaveManager.GameState.Day;

        if (dist <= interactRange && isDay && !_isShopOpen)
        {
            GUIStyle promptStyle = new GUIStyle(GUI.skin.label);
            promptStyle.fontSize = 24;
            promptStyle.alignment = TextAnchor.MiddleCenter;
            promptStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(0, Screen.height / 2 + 50, Screen.width, 50), "Press [F] to Start Night | Press [T] for Shop", promptStyle);
        }

        if (_isShopOpen)
        {
            DrawShopUI();
        }
    }

    private void DrawShopUI()
    {
        int w = 450;
        int h = 350;
        Rect windowRect = new Rect((Screen.width - w) / 2, (Screen.height - h) / 2, w, h);
        
        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
        GUI.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        GUI.Box(windowRect, "", boxStyle);
        GUI.color = Color.white;

        GUILayout.BeginArea(windowRect);
        
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 26;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = Color.white;
        GUILayout.Label("CAMP UPGRADES", titleStyle);
        GUILayout.Space(10);
        
        int points = GameManager.Instance != null ? GameManager.Instance.currency : 0;
        GUIStyle pointsStyle = new GUIStyle(GUI.skin.label);
        pointsStyle.fontSize = 18;
        pointsStyle.normal.textColor = Color.yellow;
        pointsStyle.alignment = TextAnchor.MiddleCenter;
        GUILayout.Label("Balance: $" + points, pointsStyle);
        GUILayout.Space(20);

        if (GUILayout.Button("Full Heal ($100)", GUILayout.Height(45)))
        {
            if (GameManager.Instance != null && GameManager.Instance.SpendCurrency(100))
            {
                PlayerStats stats = _playerTransform.GetComponent<PlayerStats>();
                if (stats != null) stats.Heal(9999f);
            }
        }

        if (GUILayout.Button("Max Health +25% ($200)", GUILayout.Height(45)))
        {
            if (GameManager.Instance != null && GameManager.Instance.SpendCurrency(200))
            {
                GameManager.Instance.healthMultiplier += 0.25f;
                PlayerStats stats = _playerTransform.GetComponent<PlayerStats>();
                if (stats != null) stats.UpdateMaxHealth();
            }
        }

        if (GUILayout.Button("Global Damage +15% ($300)", GUILayout.Height(45)))
        {
            if (GameManager.Instance != null && GameManager.Instance.SpendCurrency(300))
            {
                GameManager.Instance.damageMultiplier += 0.15f;
            }
        }

        if (GUILayout.Button("Restock Ammo ($50)", GUILayout.Height(45)))
        {
            WeaponManager wm = _playerTransform.GetComponent<WeaponManager>();
            if (wm != null && wm.CurrentWeaponIndex > 0)
            {
                if (GameManager.Instance != null && GameManager.Instance.SpendCurrency(50))
                {
                    wm.BuyAmmo();
                }
            }
        }
        
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("EXIT SHOP", GUILayout.Height(35)))
        {
            ToggleShop();
        }

        GUILayout.EndArea();
    }
}