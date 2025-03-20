using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public enum ZombieState
{
    Chase,
    Attack,
    Patrol,
    Damage,
    Idle,
    Dead
}

public class ZombieManager : MonoBehaviour
{
    [SerializeField] private float _attackRange = 1.0f;
    [SerializeField] private float _attackDelay = 2.0f;
    [SerializeField] private float _moveSpeed = 2.0f;
    [SerializeField] private float _chaseRange = 3.0f;
    [SerializeField] private float _evadeRange = 1.0f;
    [SerializeField] private float _idleTime = 2.0f;

    [SerializeField] private Transform[] _patrolPoints;
    [SerializeField] private ZombieState _currentState;

    private Transform _target;


    private Animator _animator;
    private HPManager _hpManager;

    private bool _isDead = false;

    private int _currentPatrolPoints = 0;
    private float _nextAttackTime = 0.0f;
    private bool _isAttack = false;
    private bool _isWaiting = false;
    private float _distanceToTarget = 0.0f;

    private Coroutine _stateCoroutine;

    private NavMeshAgent _navMeshAgent;

    private void Awake()
    {
        _target = GameObject.FindGameObjectWithTag("Player").transform;

        _animator = GetComponent<Animator>();
        _hpManager = GetComponent<HPManager>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        GetComponent<HPManager>().DeadEvent.AddListener(() =>
        {
            _isDead = true;
        });


        _currentState = ZombieState.Idle;
        ChangeState(ZombieState.Idle);
    }


    public void ChangeState(ZombieState state)
    {
        Debug.Log($"{gameObject.name} State Change \"{state}\"");

        _currentState = state;
        if (_stateCoroutine != null)
        { 
            StopCoroutine(_stateCoroutine);
        }

        if (state == ZombieState.Attack)
        {
            _stateCoroutine = StartCoroutine(Attack());
        }
        else if (state == ZombieState.Chase)
        {
            _stateCoroutine = StartCoroutine(Chase(_target));
        }
        else if (state == ZombieState.Patrol)
        {
            _stateCoroutine = StartCoroutine(Patrol());
        }
        else if (state == ZombieState.Damage)
        {
            _stateCoroutine = StartCoroutine(Damage());
        }
        else if (state == ZombieState.Idle)
        {
            _stateCoroutine = StartCoroutine(Idle());
        }
        else if (state == ZombieState.Dead)
        {
            _stateCoroutine = StartCoroutine(Dead());
        }
    }

    private IEnumerator Attack()
    {
        _animator.SetTrigger("IsAttacking");
        SoundManager.Instance.PlaySFX("ZombieAttack", transform.position);
        _navMeshAgent.isStopped = true;

        yield return new WaitForSeconds(_attackDelay);

        Transform targetPoint = _patrolPoints[_currentPatrolPoints];
        //transform.LookAt(targetPoint.position);

        if (_target != null)
        {
            _distanceToTarget = Vector3.Distance(transform.position, _target.position);
        }

        if (_isDead)
        {
            ChangeState(ZombieState.Dead);
        }
        else if (_target != null && _chaseRange > _distanceToTarget)
        {
            ChangeState(ZombieState.Chase);
        }
        else if (_patrolPoints.Length > 0)
        {
            ChangeState(ZombieState.Patrol);
        }
        else
        {
            ChangeState(ZombieState.Idle);
        }

        yield return null;
    }

    private IEnumerator Patrol()
    {
        _animator.SetBool("IsWalking", true);

        while (_currentState == ZombieState.Patrol)
        {
            if (_patrolPoints.Length > 0)
            {
                Transform targetPoint = _patrolPoints[_currentPatrolPoints];
                Vector3 direction = (targetPoint.position - transform.position).normalized;
                //transform.position += _moveSpeed * Time.deltaTime * direction;
                //transform.LookAt(targetPoint.position);
                _navMeshAgent.speed = _moveSpeed;
                _navMeshAgent.isStopped = false;
                _navMeshAgent.SetDestination(targetPoint.position);

                if (Vector3.Distance(transform.position, targetPoint.position) < 0.3f)
                {
                    _currentPatrolPoints = (_currentPatrolPoints + 1) % _patrolPoints.Length;
                }

                if (_target != null)
                {
                    _distanceToTarget = Vector3.Distance(transform.position, _target.position);
                }

                if (_isDead)
                {
                    ChangeState(ZombieState.Dead);
                }
                else if (_target != null && _chaseRange > _distanceToTarget)
                {
                    ChangeState(ZombieState.Chase);
                }
            }
            else
            {
                ChangeState(ZombieState.Idle);
            }

            yield return null;
        }
    }

    private IEnumerator Chase(Transform target)
    {
        _animator.SetBool("IsWalking", true);
        SoundManager.Instance.PlaySFX("ZombieChase", transform.position);

        while (_currentState == ZombieState.Chase)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            //transform.position += _moveSpeed * Time.deltaTime * direction;
            //transform.LookAt(target.position);
            _navMeshAgent.speed = _moveSpeed;
            _navMeshAgent.isStopped = false;
            _navMeshAgent.SetDestination(target.position);

            if (_target != null)
            {
                _distanceToTarget = Vector3.Distance(transform.position, _target.position);
            }

            if (_isDead)
            {
                ChangeState(ZombieState.Dead);
            }
            else if (_target != null && _attackRange >= _distanceToTarget)
            {
                ChangeState(ZombieState.Attack);
            }
            else if (_chaseRange < _distanceToTarget && _patrolPoints.Length > 0)
            {
                ChangeState(ZombieState.Patrol);
            }
            else if (_chaseRange < _distanceToTarget)
            {
                ChangeState(ZombieState.Idle);
            }

            yield return null;
        }
    }

    private IEnumerator Damage()
    {
        if (_isDead)
        {
            ChangeState(ZombieState.Dead);
        }
        else
        {
            _animator.SetTrigger("IsHit");
            _navMeshAgent.isStopped = true;
            SoundManager.Instance.PlaySFX("ZombieHit", transform.position);

            yield return new WaitForSeconds(0.2f);

            if (_target != null && _attackRange >= _distanceToTarget)
            {
                ChangeState(ZombieState.Attack);
            }
            else if (_target != null && _chaseRange > _distanceToTarget)
            {
                ChangeState(ZombieState.Chase);
            }
            else if (_patrolPoints.Length > 0)
            {
                ChangeState(ZombieState.Patrol);
            }
            else
            {
                ChangeState(ZombieState.Idle);
            }
        }

        yield return null;
    }

    private IEnumerator Idle()
    {
        _animator.SetBool("IsWalking", false);
        while (_currentState == ZombieState.Idle)
        {
            _navMeshAgent.isStopped = true;

            if (_target != null)
            {
                _distanceToTarget = Vector3.Distance(transform.position, _target.position);
            }

            if (_isDead)
            {
                ChangeState(ZombieState.Dead);
            }
            else if (_target != null && _attackRange >= _distanceToTarget)
            {
                ChangeState(ZombieState.Attack);
            }
            else if (_target != null && _chaseRange > _distanceToTarget)
            {
                ChangeState(ZombieState.Chase);
            }
            else if (_patrolPoints.Length > 0)
            {
                ChangeState(ZombieState.Patrol);
            }

            yield return null;
        }
    }

    private IEnumerator Dead()
    {
        _navMeshAgent.isStopped = true;

        _animator.SetTrigger("IsDead");
        GetComponent<Collider>().enabled = false;

        yield return new WaitForSeconds(3.0f);
        
        Destroy(gameObject);

        yield return null;
    }

    public void GetDamage(int damage)
    {
        _hpManager.ChangeHP(-damage);
        ChangeState(ZombieState.Damage);
    }
}