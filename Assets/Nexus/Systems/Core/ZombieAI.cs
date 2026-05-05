using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ZombieAI : MonoBehaviour
{
    public float damage = 15f;
    public float attackCooldown = 2f;
    public float attackRange = 1.5f;

    private NavMeshAgent _agent;
    private Transform _playerTransform;
    private PlayerStats _playerStats;
    private Animator _animator;
    private float _nextAttackTime = 0f;

    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponentInChildren<Animator>();
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerTransform = player.transform;
            _playerStats = player.GetComponent<PlayerStats>();
        }

        _agent.stoppingDistance = attackRange;
    }

    private void Update()
    {
        if (_playerTransform == null || !_agent.isOnNavMesh) return;

        float distanceToPlayer = Vector3.Distance(transform.position, _playerTransform.position);

        if (distanceToPlayer <= attackRange)
        {
            _agent.isStopped = true;
            
            if (_animator != null)
            {
                _animator.SetBool("isWalking", false);
            }

            if (Time.time >= _nextAttackTime)
            {
                AttackPlayer();
            }
        }
        else
        {
            _agent.isStopped = false;
            _agent.SetDestination(_playerTransform.position);
            
            if (_animator != null)
            {
                bool isMoving = _agent.velocity.magnitude > 0.1f;
                _animator.SetBool("isWalking", isMoving);
            }
        }
    }

    private void AttackPlayer()
    {
        _nextAttackTime = Time.time + attackCooldown;
        
        if (_animator != null)
        {
            _animator.SetTrigger("Attack");
        }

        if (_playerStats != null)
        {
            _playerStats.TakeDamage(damage);
        }
    }
}