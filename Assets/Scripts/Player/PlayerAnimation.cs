using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;

[RequireComponent(typeof(PlayerManager), typeof(Animator))]
public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private GameObject _gunObject;
    [SerializeField] private RigBuilder _rigBuilder;

    private PlayerManager _player;
    private Animator _animator;

    private Coroutine _zoomLayerCoroutine;
    private Coroutine _actionLayerCoroutine;

    private bool _zoomLock = false;

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

        if (!_zoomLock)
        {
            _animator.SetFloat("Horizontal", _player.HorizontalMove);
            _animator.SetFloat("Vertical", _player.VerticalMove);
        }
        else
        {
            _animator.SetFloat("Horizontal", 0);
            _animator.SetFloat("Vertical", 0);
        }
    }

    public void WeaponPullOut(Action onActionEnd)
    {
        _animator.SetLayerWeight(1, 0);
        _actionLayerCoroutine = StartCoroutine(FadeLayerWeight(2, 1, 0.2f));
        _animator.SetTrigger("IsWeaponPullOut");
        GameManager.Instance.PlayAfterCoroutine(() =>
        {
            if (_actionLayerCoroutine != null)
            {
                StopCoroutine(_actionLayerCoroutine);
            }

            onActionEnd?.Invoke();
            _actionLayerCoroutine = StartCoroutine(FadeLayerWeight(2, 0, 0.2f));
        }, 1.3f);
    }

    public void AnimationWeaponPullOutEvent()
    {
        _gunObject.SetActive(true);
        SoundManager.Instance.PlaySFX("ChangeWeapon", transform.position);
    }

    public void WeaponPutAway()
    {
        _animator.SetLayerWeight(1, 0);
        _actionLayerCoroutine = StartCoroutine(FadeLayerWeight(2, 1, 0.2f));
        _animator.SetTrigger("IsWeaponPutAway");
        GameManager.Instance.PlayAfterCoroutine(() =>
        {
            if (_actionLayerCoroutine != null)
            {
                StopCoroutine(_actionLayerCoroutine);
            }

            _actionLayerCoroutine = StartCoroutine(FadeLayerWeight(2, 0, 0.2f));
        }, 2.3f);
    }

    public void AnimationWeaponPutAwayEvent()
    {
        _gunObject.SetActive(false);
        SoundManager.Instance.PlaySFX("ChangeWeapon", transform.position);
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
        }, 1.4f);
    }

    public void Reloading(Action endAction)
    {
        if (_actionLayerCoroutine != null)
        {
            StopCoroutine(_actionLayerCoroutine);
        }

        _animator.SetLayerWeight(1, 0);
        _actionLayerCoroutine = StartCoroutine(FadeLayerWeight(2, 1, 0.2f));
        _animator.SetTrigger("IsReloading");
        GameManager.Instance.PlayAfterCoroutine(() =>
        {
            if (_actionLayerCoroutine != null)
            {
                StopCoroutine(_actionLayerCoroutine);
            }

            endAction?.Invoke();
            _actionLayerCoroutine = StartCoroutine(FadeLayerWeight(2, 0, 0.2f));
        }, 2.2f);
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

        _animator.Play("Idle");

        _zoomLock = true;

        GameManager.Instance.PlayAfterCoroutine(() =>
        {
            if (_zoomLayerCoroutine != null)
            {
                StopCoroutine(_zoomLayerCoroutine);
            }

            _animator.SetLayerWeight(2, 0);
            //_animator.SetLayerWeight(1, 1);

            _zoomLayerCoroutine = StartCoroutine(FadeLayerWeight(1, 1, 0.2f));
            GameManager.Instance.PlayAfterCoroutine(() =>
            {
                _zoomLock = false;
            }, 0.05f);
        }, 0.05f);
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

    public void Dead()
    {
        if (_zoomLayerCoroutine != null)
        {
            StopCoroutine(_zoomLayerCoroutine);
        }

        if (_actionLayerCoroutine != null)
        {
            StopCoroutine(_actionLayerCoroutine);
        }

        _animator.SetLayerWeight(0, 1);
        _animator.SetLayerWeight(1, 0);
        _animator.SetLayerWeight(2, 0);
        _animator.SetTrigger("IsDead");
    }
}
