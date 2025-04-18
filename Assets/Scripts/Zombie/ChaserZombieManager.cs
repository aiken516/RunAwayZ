using System.Collections;
using UnityEngine;
using UnityEngine.AI;


public class ChaserZombieManager : MonoBehaviour
{
    [SerializeField] private float _attackRange = 1.0f;
    [SerializeField] private float _attackDelay = 2.0f;
    [SerializeField] private float _moveSpeed = 2.0f;

    [SerializeField] private ZombieState _currentState;

    private Transform _target;

    private Animator _animator;
    private HPManager _hpManager;

    private bool _isDead = false;

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
            _animator.SetBool("IsDie", true);
        });

        _currentState = ZombieState.Chase;
        ChangeState(ZombieState.Chase);
    }


    public void ChangeState(ZombieState state)
    {
        //Debug.Log($"{gameObject.name} State Change \"{state}\"");

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
        else if (state == ZombieState.Damage)
        {
            _stateCoroutine = StartCoroutine(Damage());
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

        if (_target != null)
        {
            _distanceToTarget = Vector3.Distance(transform.position, _target.position);
        }

        if (_isDead)
        {
            ChangeState(ZombieState.Dead);
        }
        else
        {
            ChangeState(ZombieState.Chase);
        }

        yield return null;
    }

    private IEnumerator Chase(Transform target)
    {
        _animator.SetBool("IsRunning", true);
        SoundManager.Instance.PlaySFX("ZombieChase", transform.position);

        while (_currentState == ZombieState.Chase)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            //transform.position += _moveSpeed * Time.deltaTime * direction;
            //transform.LookAt(target.position);
            _navMeshAgent.speed = _moveSpeed;
            _navMeshAgent.isStopped = false;
            _navMeshAgent.SetDestination(target.position);

            if (_navMeshAgent.isOnOffMeshLink)
            {
                _navMeshAgent.speed = _moveSpeed * 0.3f; // 속도를 절반으로 감소
            }
            else
            {
                _navMeshAgent.speed = _moveSpeed; // 원래 속도로 복구
            }

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
            else
            {
                ChangeState(ZombieState.Chase);
            }
        }

        yield return null;
    }

    private IEnumerator Dead()
    {
        UIManager.Instance.KillZombie();

        _navMeshAgent.isStopped = true;

        _animator.SetTrigger("IsDead");
        GetComponent<Collider>().enabled = false;
        _isDead = true;

        yield return new WaitForSeconds(30.0f);

        //Destroy(gameObject);

        _isDead = false;
        ChangeState(ZombieState.Chase);
        GetComponent<Collider>().enabled = true;
        transform.position = ChaserManager.Instance.RespawnPoint.position;
        _animator.Play("Idle");
        GetComponent<HPManager>().ChangeHP(GetComponent<HPManager>().MaxHP);
        _animator.SetBool("IsDie", false);

        _navMeshAgent.isStopped = false;

        yield return null;
    }

    public void GetDamage(int damage)
    {
        _hpManager.ChangeHP(-damage);
        ChangeState(ZombieState.Damage);
    }

    public void GetShot(int damage, Vector3 hitPoint)
    {
        GetDamage(damage);
        ParticleManager.Instance.PlayParticle(ParticleManager.ParticleType.EnemyHit, hitPoint);
        //ParticleSystem particle = Instantiate(_shotHitParticle, hitPoint, Quaternion.identity);
        SoundManager.Instance.PlaySFX("EnemyHit", hitPoint);
    }

    public void AnimationEventPlayFootstep()
    {
        SoundManager.Instance.PlaySFX($"Foorstep_{(int)UnityEngine.Random.Range(1, 5)}", transform.position);
    }
}