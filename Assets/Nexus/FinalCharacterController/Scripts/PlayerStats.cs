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

    private void Awake()
    {
        _playerState = GetComponent<PlayerState>();
        _input = GetComponent<PlayerLocomotionInput>();

        currentHealth = maxHealth;
        currentStamina = maxStamina;
    }

    private void Start()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }

    private void Update()
    {
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

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void ConsumeStamina(float amount)
    {
        currentStamina -= amount;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }

    public void RestoreStamina(float amount)
    {
        currentStamina += amount;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }

    private void Die()
    {
        Debug.Log("Dead");
    }
}