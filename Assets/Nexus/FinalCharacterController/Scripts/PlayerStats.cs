using System;
using UnityEngine;
using Nexus.FinalCharacterController;

public class PlayerStats : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth;
    
    [Header("Stamina")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float staminaDrainRate = 20f; 
    public float staminaRegenRate = 15f; 
    public float regenDelay = 1.5f;      

    public Action<float, float> OnHealthChanged;
    public Action<float, float> OnStaminaChanged;

    private float _regenTimer;
    private PlayerState _playerState;
    private PlayerLocomotionInput _input;
    private PlayerController _playerController;
    private WeaponManager _weaponManager;
    private bool _isDead = false;
    
    private Vector3 _startPosition;
    private Quaternion _startRotation;

    private void Awake()
    {
        _playerState = GetComponent<PlayerState>();
        _input = GetComponent<PlayerLocomotionInput>();
        _playerController = GetComponent<PlayerController>();
        _weaponManager = GetComponent<WeaponManager>();

        _startPosition = transform.position;
        _startRotation = transform.rotation;
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerDied += RespawnPlayer;
        }
        
        UpdateMaxHealth();
        currentHealth = maxHealth * (GameManager.Instance != null ? GameManager.Instance.healthMultiplier : 1f);
        currentStamina = maxStamina;
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth * (GameManager.Instance != null ? GameManager.Instance.healthMultiplier : 1f));
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerDied -= RespawnPlayer;
        }
    }

    private void Update()
    {
        if (_isDead) return;
        HandleStamina();
    }

    private void HandleStamina()
    {
        if (_playerState == null) return;

        bool isSprinting = _playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;

        if (isSprinting)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
            _regenTimer = regenDelay;

            if (currentStamina <= 0f)
            {
                currentStamina = 0f;
                if (_input != null)
                {
                    _input.ForceStopSprint();
                }
            }

            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
        }
        else
        {
            if (currentStamina < maxStamina)
            {
                if (_regenTimer > 0)
                {
                    _regenTimer -= Time.deltaTime;
                }
                else
                {
                    currentStamina += staminaRegenRate * Time.deltaTime;
                    if (currentStamina > maxStamina)
                    {
                        currentStamina = maxStamina;
                    }
                    
                    OnStaminaChanged?.Invoke(currentStamina, maxStamina);
                }
            }
        }
    }

    public void UpdateMaxHealth()
    {
        float mult = GameManager.Instance != null ? GameManager.Instance.healthMultiplier : 1f;
        float actualMax = maxHealth * mult;
        OnHealthChanged?.Invoke(currentHealth, actualMax);
    }

    public void Heal(float amount)
    {
        float mult = GameManager.Instance != null ? GameManager.Instance.healthMultiplier : 1f;
        float actualMax = maxHealth * mult;
        currentHealth = Mathf.Min(currentHealth + amount, actualMax);
        OnHealthChanged?.Invoke(currentHealth, actualMax);
    }

    public void TakeDamage(float amount)
    {
        if (_isDead) return;

        currentHealth -= amount;
        
        float mult = GameManager.Instance != null ? GameManager.Instance.healthMultiplier : 1f;
        float actualMax = maxHealth * mult;
        
        currentHealth = Mathf.Clamp(currentHealth, 0, actualMax);
        OnHealthChanged?.Invoke(currentHealth, actualMax);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void ConsumeStamina(float amount)
    {
        if (_isDead) return;

        currentStamina -= amount;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }

    public void RestoreStamina(float amount)
    {
        if (_isDead) return;

        currentStamina += amount;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }

    private void Die()
    {
        _isDead = true;
        if (_playerController != null) _playerController.enabled = false;
        if (_weaponManager != null) _weaponManager.enabled = false;
        
        if (GameManager.Instance != null) GameManager.Instance.LoseLife();
    }

    private void RespawnPlayer()
    {
        _isDead = false;
        
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        
        transform.position = _startPosition;
        transform.rotation = _startRotation;
        
        if (cc != null) cc.enabled = true;

        if (_playerController != null) _playerController.enabled = true;
        if (_weaponManager != null) _weaponManager.enabled = true;

        float mult = GameManager.Instance != null ? GameManager.Instance.healthMultiplier : 1f;
        currentHealth = maxHealth * mult;
        UpdateMaxHealth();
        
        currentStamina = maxStamina;
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }

    private void OnGUI()
    {
        if (_isDead)
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = 100;
            style.normal.textColor = Color.red;
            style.alignment = TextAnchor.MiddleCenter;
            
            string msg = (GameManager.Instance != null && GameManager.Instance.currentLives <= 0) ? "GAME OVER" : "YOU DIED";
            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), msg, style);
        }
    }
}