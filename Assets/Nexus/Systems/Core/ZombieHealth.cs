using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class ZombieHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float hitPauseDuration = 0.5f;
    public float deathDestroyDelay = 3f;

    private float currentHealth;
    private bool isDead = false;
    
    private Animator _animator;
    private NavMeshAgent _agent;
    private ZombieAI _zombieAI;

    private void Start()
    {
        currentHealth = maxHealth;
        _animator = GetComponentInChildren<Animator>();
        _agent = GetComponent<NavMeshAgent>();
        _zombieAI = GetComponent<ZombieAI>();
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            if (_animator != null)
            {
                _animator.SetTrigger("Hit");
            }
            StartCoroutine(HitPauseRoutine());
        }
    }

    private IEnumerator HitPauseRoutine()
    {
        if (_agent != null) _agent.isStopped = true;
        if (_zombieAI != null) _zombieAI.enabled = false;
        
        yield return new WaitForSeconds(hitPauseDuration);
        
        if (!isDead)
        {
            if (_agent != null) _agent.isStopped = false;
            if (_zombieAI != null) _zombieAI.enabled = true;
        }
    }

    private void Die()
    {
        isDead = true;
        
        if (_animator != null)
        {
            _animator.SetTrigger("Death");
        }
        
        if (_agent != null)
        {
            _agent.isStopped = true;
            _agent.enabled = false;
        }

        if (_zombieAI != null)
        {
            _zombieAI.enabled = false;
        }

        Collider coll = GetComponent<Collider>();
        if (coll != null)
        {
            coll.enabled = false;
        }

        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.ZombieDied();
        }
        
        Destroy(gameObject, deathDestroyDelay);
    }
}