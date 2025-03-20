using UnityEngine;
using UnityEngine.Events;

public class HPManager : MonoBehaviour
{
    [SerializeField] private float _maxHP;
    private float _currentHP = 0.0f;
    
    public float MaxHP => _maxHP;
    public float CurrentHP => _currentHP;

    public UnityEvent DeadEvent;

    private void Start()
    {
        _currentHP = _maxHP;
    }

    public void ChangeHP(float changeAmount)
    {
        _currentHP += changeAmount;
        if (_currentHP > _maxHP)
        {
            _currentHP = _maxHP;
        }
        else if (_currentHP <= 0.0f)
        {
            DeadEvent.Invoke();
        }

        Debug.Log($"{gameObject.name} HP Change : {changeAmount}, Current HP : {_currentHP}");
    }
}
