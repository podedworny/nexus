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

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerTransform = player.transform;
            _playerStats = player.GetComponent<PlayerStats>();
        }

        _agent.stoppingDistance = attackRange;
    }

    public void Initialize(float dmgAmount, float spdAmount)
    {
        damage = dmgAmount;
        if (_agent != null)
        {
            _agent.speed = spdAmount;
        }
    }

    private void Update()
    {
        if (_playerTransform == null || !_agent.isOnNavMesh) return;

        float distanceToPlayer = Vector3.Distance(transform.position, _playerTransform.position);
        bool isInAttackRange = distanceToPlayer <= attackRange;

        if (isInAttackRange)
        {
            _agent.isStopped = true;

            if (_animator != null)
            {
                _animator.SetBool("isWalking", false);
                _animator.speed = 1f;
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

                if (isMoving)
                {
                    _animator.speed = _agent.velocity.magnitude;
                }
                else
                {
                    _animator.speed = 1f;
                }
            }
        }
    }

    private void AttackPlayer()
    {
        _nextAttackTime = Time.time + attackCooldown;
        if (_animator != null) _animator.SetTrigger("Attack");
    }

    public void DealDamageToPlayer()
    {
        if (_playerStats == null || _playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, _playerTransform.position);
        if (distanceToPlayer <= attackRange * 1.2f)
        {
            _playerStats.TakeDamage(damage);
        }
    }
}
