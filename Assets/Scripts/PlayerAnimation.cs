using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using static UnityEditor.Experimental.GraphView.GraphView;

[RequireComponent(typeof(PlayerManager), typeof(Animator))]
public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private GameObject _gunObject;
    [SerializeField] private RigBuilder _rigBuilder;

    private PlayerManager _player;
    private Animator _animator;

    private Coroutine _zoomLayerCoroutine;
    private Coroutine _actionLayerCoroutine;


    void Start()
    {
        _player = GetComponent<PlayerManager>();
        _animator = GetComponent<Animator>();
    }

    IEnumerator FadeLayerWeight(int layerIndex, float targetWeight, float duration)//애니메이션으로
    {
        float startWeight = _animator.GetLayerWeight(layerIndex);
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newWeight = Mathf.Lerp(startWeight, targetWeight, elapsedTime / duration);
            _animator.SetLayerWeight(layerIndex, newWeight);
            yield return null;
        }

        _animator.SetLayerWeight(layerIndex, targetWeight);
    }

    void Update()
    {
        _animator.SetBool("IsRunning", _player.IsRunning);
        _animator.SetFloat("Horizontal", _player.HorizontalMove);
        _animator.SetFloat("Vertical", _player.VerticalMove);
    }

    public void WeaponPullOut()
    {
        _animator.SetLayerWeight(1, 0);
        _actionLayerCoroutine = StartCoroutine(FadeLayerWeight(2, 1, 0.2f));
        _animator.SetTrigger("IsWeaponPullOut");
        _gunObject.SetActive(true);
        GameManager.Instance.PlayAfterCoroutine(() =>
        {
            if (_actionLayerCoroutine != null)
            {
                StopCoroutine(_actionLayerCoroutine);
            }

            _actionLayerCoroutine = StartCoroutine(FadeLayerWeight(2, 0, 0.2f));
        }, 2.3f);
    }

    public void WeaponPutAway()
    {
        _animator.SetLayerWeight(1, 0);
        _actionLayerCoroutine = StartCoroutine(FadeLayerWeight(2, 1, 0.2f));
        _animator.SetTrigger("IsWeaponPutAway");
        _gunObject.SetActive(false);
        GameManager.Instance.PlayAfterCoroutine(() =>
        {
            if (_actionLayerCoroutine != null)
            {
                StopCoroutine(_actionLayerCoroutine);
            }

            _actionLayerCoroutine = StartCoroutine(FadeLayerWeight(2, 0, 0.2f));
        }, 2.3f);
    }

    public void GetItem()
    {
        _animator.SetLayerWeight(1, 0);
        _actionLayerCoroutine = StartCoroutine(FadeLayerWeight(2, 1, 0.2f));
        _animator.SetTrigger("IsPicking");
        GameManager.Instance.PlayAfterCoroutine(() =>
        {
            if (_actionLayerCoroutine != null)
            {
                StopCoroutine(_actionLayerCoroutine);
            }

            _actionLayerCoroutine = StartCoroutine(FadeLayerWeight(2, 0, 0.2f));
        }, 2.8f);
    }

    public void Fire()
    {
        _animator.SetTrigger("IsFiring");
    }

    public void Punch()
    {
        if (_actionLayerCoroutine != null)
        { 
            StopCoroutine(_actionLayerCoroutine);
        }

        _animator.SetLayerWeight(1, 0);
        _actionLayerCoroutine = StartCoroutine(FadeLayerWeight(2, 1, 0.2f));
        _animator.SetTrigger("IsPunching");
        GameManager.Instance.PlayAfterCoroutine(() =>
        {
            if (_actionLayerCoroutine != null)
            {
                StopCoroutine(_actionLayerCoroutine);
            }

            _actionLayerCoroutine = StartCoroutine(FadeLayerWeight(2, 0, 0.2f));
        }, 0.8f);
    }

    public void Hit()
    {
        _animator.SetTrigger("IsHit");
    }

    public void ZoomIn()
    {
        _rigBuilder.enabled = true;

        if (_zoomLayerCoroutine != null)
        {
            StopCoroutine(_zoomLayerCoroutine);
        }

        _animator.SetLayerWeight(2, 0);
        _zoomLayerCoroutine = StartCoroutine(FadeLayerWeight(1, 1, 0.2f));
    }

    public void ZoomOut()
    {
        _rigBuilder.enabled = false;

        if (_zoomLayerCoroutine != null)
        {
            StopCoroutine(_zoomLayerCoroutine);
        }

        _animator.SetLayerWeight(2, 0);
        _zoomLayerCoroutine = StartCoroutine(FadeLayerWeight(1, 0, 0.2f));
    }
}
