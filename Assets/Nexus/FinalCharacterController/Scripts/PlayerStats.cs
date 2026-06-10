using System;
using UnityEngine;
using Nexus.FinalCharacterController;

public class PlayerStats : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float hitStunDuration = 0.4f;

    [Header("Armor")]
    public float armorReduction = 0f;

    [Header("Currency")]
    public int currency = 0;
    private int _savedCurrency = 0;
    public Action<int> OnCurrencyChanged;

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
    private PlayerAnimation _playerAnimation;
    private bool _isDead = false;

    private Vector3 _startPosition;
    private Quaternion _startRotation;

    public Vector3 StartPosition => _startPosition;

    private void Awake()
    {
        _playerState = GetComponent<PlayerState>();
        _input = GetComponent<PlayerLocomotionInput>();
        _playerController = GetComponent<PlayerController>();
        _weaponManager = GetComponent<WeaponManager>();
        _playerAnimation = GetComponent<PlayerAnimation>();

        _startPosition = transform.position;
        _startRotation = transform.rotation;
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerDied += RespawnPlayer;
            GameManager.Instance.OnSaveCheckpoint += SaveCurrency;
            GameManager.Instance.OnLoadCheckpoint += LoadCurrency;
        }

        UpdateMaxHealth();
        currentHealth = GetActualMaxHealth();
        currentStamina = maxStamina;

        if (PlayerPrefs.GetInt("LoadSave", 0) == 1)
        {
            CharacterController cc = GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            float x = PlayerPrefs.GetFloat("PlayerX", transform.position.x);
            float y = PlayerPrefs.GetFloat("PlayerY", transform.position.y);
            float z = PlayerPrefs.GetFloat("PlayerZ", transform.position.z);
            transform.position = new Vector3(x, y, z);
            Physics.SyncTransforms();

            if (cc != null) cc.enabled = true;

            currency = PlayerPrefs.GetInt("PlayerCurrency", currency);
            currentHealth = PlayerPrefs.GetFloat("PlayerHealth", currentHealth);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.currentLives = PlayerPrefs.GetInt("PlayerLives", GameManager.Instance.currentLives);
            }

            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.currentWave = PlayerPrefs.GetInt("CurrentWave", WaveManager.Instance.currentWave);
            }

            PlayerPrefs.SetInt("LoadSave", 0);
            PlayerPrefs.Save();
        }

        OnHealthChanged?.Invoke(currentHealth, GetActualMaxHealth());
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
        OnCurrencyChanged?.Invoke(currency);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerDied -= RespawnPlayer;
            GameManager.Instance.OnSaveCheckpoint -= SaveCurrency;
            GameManager.Instance.OnLoadCheckpoint -= LoadCurrency;
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

    public void AddCurrency(int amount)
    {
        currency += amount;
        OnCurrencyChanged?.Invoke(currency);
    }

    public bool SpendCurrency(int amount)
    {
        if (currency >= amount)
        {
            currency -= amount;
            OnCurrencyChanged?.Invoke(currency);
            return true;
        }
        return false;
    }

    private void SaveCurrency()
    {
        _savedCurrency = currency;
    }

    private void LoadCurrency()
    {
        currency = _savedCurrency;
        OnCurrencyChanged?.Invoke(currency);
    }

    public float GetActualMaxHealth()
    {
        float mult = GameManager.Instance != null ? GameManager.Instance.healthMultiplier : 1f;
        return maxHealth * mult;
    }

    public void UpdateMaxHealth()
    {
        float actualMax = GetActualMaxHealth();
        OnHealthChanged?.Invoke(currentHealth, actualMax);
    }

    public void Heal(float amount)
    {
        float actualMax = GetActualMaxHealth();
        currentHealth = Mathf.Min(currentHealth + amount, actualMax);
        OnHealthChanged?.Invoke(currentHealth, actualMax);
    }

    public void AddArmor(float amount)
    {
        armorReduction += amount;
        armorReduction = Mathf.Clamp(armorReduction, 0f, 0.8f);
    }

    public void TakeDamage(float amount)
    {
        if (_isDead) return;

        float reducedDamage = amount * (1f - armorReduction);
        currentHealth -= reducedDamage;

        float actualMax = GetActualMaxHealth();
        currentHealth = Mathf.Clamp(currentHealth, 0, actualMax);
        OnHealthChanged?.Invoke(currentHealth, actualMax);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            if (_playerAnimation != null) _playerAnimation.TriggerHit();
            if (_playerController != null) _playerController.ApplyStun(hitStunDuration);
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

        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        if (_playerController != null) _playerController.enabled = false;
        if (_weaponManager != null) _weaponManager.enabled = false;
        if (_playerAnimation != null) _playerAnimation.TriggerDie();

        Invoke(nameof(ShowDeathMenuDelayed), 3f);
    }

    private void ShowDeathMenuDelayed()
    {
        DeathMenuManager deathMenu = FindFirstObjectByType<DeathMenuManager>();
        if (deathMenu != null)
        {
            deathMenu.ShowDeathMenu();
        }
    }

    private void RespawnPlayer()
    {
        _isDead = false;

        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        transform.position = _startPosition;
        transform.rotation = _startRotation;
        Physics.SyncTransforms();

        if (cc != null) cc.enabled = true;

        if (_playerController != null) _playerController.enabled = true;
        if (_weaponManager != null) _weaponManager.enabled = true;
        if (_playerAnimation != null) _playerAnimation.TriggerRespawn();

        currentHealth = GetActualMaxHealth();
        UpdateMaxHealth();

        currentStamina = maxStamina;
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }
}
