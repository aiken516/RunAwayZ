using UnityEngine;
using UnityEngine.Events;

public class HPManager : MonoBehaviour
{
    [SerializeField] public UnityEvent DeadEvent = new UnityEvent();
    [SerializeField] private float _maxHP;
    [SerializeReference] private float _currentHP;

    public float MaxHP => _maxHP;
    public float CurrentHP => _currentHP;

    private void Awake()
    {
        _currentHP = _maxHP;
    }

    public void ChangeHP(float changeAmount)
    {
        _currentHP = Mathf.Clamp(_currentHP + changeAmount, 0.0f, _maxHP);

        if (_currentHP <= 0.0f)
        {
            DeadEvent?.Invoke();
        }

        if (gameObject.name != "Player")
        {
            Debug.Log($"{gameObject.name} HP Change : {changeAmount}, Current HP : {_currentHP}");
        }
    }
}