using UnityEngine;

public enum WeaponType
{
    Punch,
    Rifle,
    Pistol
}

[RequireComponent(typeof(PlayerManager), typeof(Animator))]
public class PlayerAttack : MonoBehaviour
{
    //SO로 분리하는게 좋을 수도
    [SerializeField] private float _rifleMaxDistance = 500.0f;
    [SerializeField] private int _maxRifleAmmo = 120;
    private int _remainRifleAmmo = 0;
    [SerializeField] private int _maxRifleMagazine = 30;
    private int _remainRifleMagazine = 0;

    [SerializeField] private float _rifleRecoilStrength = 2.0f;//반동
    [SerializeField] private float _rifleMaxRecoilAngle = 2.0f;
    [SerializeField] private float _rifleShakeDuration = 0.1f;//셰이킹
    [SerializeField] private float _rifleShakeMagnitude = 0.1f;

    [SerializeField] private float _pistolMaxDistance = 80.0f;
    [SerializeField] private int _maxPistolAmmo = 40;
    private int _remainPistolAmmo = 0;
    [SerializeField] private int _maxPistolMagazine = 10;
    private int _remainPistolMagazine = 0;

    [SerializeField] private float _punchMaxDistance = 2.0f;

    private float _punchDelay = 1.0f;
    private float _rifleFireDelay = 0.2f;
    private float _pistolFireDelay = 0.5f;

    [SerializeField] private Camera _mainCamera;
    [SerializeField] private LayerMask _targetLayerMask;

    [SerializeField] private Transform _rifleFirePosition;

    private PlayerManager _player;
    private PlayerAnimation _playerAnimation;

    private WeaponType _currentWeapon = WeaponType.Punch;

    public bool HasRifle = false;
    public bool HasPistol = false;

    private bool _isFiring = false;
    private bool _isPunching = false;

    public bool IsReloading => _isReloading;
    private bool _isReloading = false;

    void Awake()
    {
        _player = GetComponent<PlayerManager>();
        _playerAnimation = GetComponent<PlayerAnimation>();
    }

    private void Update()
    {

        
    }

    public bool CanZoomAim()
    {
        return _currentWeapon == WeaponType.Rifle || _currentWeapon == WeaponType.Pistol;
    }

    public void ChangeWeapon(WeaponType weaponType)
    {
        if (_isReloading || _isPunching)
        {
            return;
        }

        if (weaponType == WeaponType.Rifle)
        {
            if (!HasRifle)
            {
                return;
            }
            _playerAnimation.WeaponPullOut(() => 
            {
                _currentWeapon = weaponType;
                UIManager.Instance.SetWeaponUI(_currentWeapon);
            });
        }
        else if (weaponType == WeaponType.Pistol)
        {
            if (!HasPistol)
            {
                return;
            }

            _playerAnimation.WeaponPutAway(/*() => { _currentWeapon = weaponType; }*/);
            _currentWeapon = weaponType;
        }
        else
        {
            _currentWeapon = weaponType;
        }

        UIManager.Instance.SetWeaponUI(_currentWeapon);
    }

    public void Reload()
    {
        if (_isReloading)
        {
            return;
        }

        Debug.Log("Reload");
        if (_currentWeapon == WeaponType.Rifle)
        { 
            ReloadRifle();
        }
        else if (_currentWeapon == WeaponType.Pistol)
        {
            //ReloadPistol();
        }
    }

    public void ReloadRifle()
    {
        int reloadAmount = _maxRifleMagazine - _remainRifleMagazine;
        if (_remainRifleAmmo < reloadAmount)
        {
            reloadAmount = _remainRifleAmmo;
            _remainRifleAmmo = 0;
        }
        else
        {
            _remainRifleAmmo -= reloadAmount;
        }

        _isReloading = true;
        SoundManager.Instance.PlaySFX("Reloading", transform.position);

        _playerAnimation.Reloading(() => {
            _remainRifleMagazine += reloadAmount;
            UIManager.Instance.SetRemainRifleAmmoText(_remainRifleMagazine, _remainRifleAmmo);
            _isReloading = false;
        });
    }

    public void ChangeRifleAmmo(int count)
    {
        _remainRifleAmmo += count;
        if (_remainRifleAmmo > _maxRifleAmmo)
        {
            _remainRifleAmmo = _maxRifleAmmo;
        }

        Debug.Log($"Remain Rifle Ammo : {_remainRifleAmmo}");
        UIManager.Instance.SetRemainRifleAmmoText(_remainRifleMagazine, _remainRifleAmmo);
    }

    public void ChangePistolAmmo(int count)
    {
        _remainPistolAmmo += count;
        if (_remainPistolAmmo > _maxPistolAmmo)
        {
            _remainPistolAmmo = _maxPistolAmmo;
        }

        UIManager.Instance.SetRemainPistolAmmoText(_remainRifleMagazine, _remainPistolAmmo);
    }

    public void UseWeapon()//shoot으로
    {
        if (_currentWeapon == WeaponType.Punch)
        {
            UsePunch();
        }
        else if (_currentWeapon == WeaponType.Rifle)
        {
            UseRifle();
        }
        else if (_currentWeapon == WeaponType.Pistol)
        {
            UsePistol();
        }
    }

    private void UsePunch()
    {
        if (!_player.IsAiming && !_isPunching)
        {
            Ray ray = new Ray(_mainCamera.transform.position, _mainCamera.transform.forward);
            RaycastHit hit;

            _isPunching = true;
            GameManager.Instance.PlayAfterCoroutine(() =>
            {
                _isPunching = false;
            }, _punchDelay);

            _playerAnimation.Punch();

            if (Physics.Raycast(_mainCamera.transform.position,
                _mainCamera.transform.forward, out hit, _punchMaxDistance))
            {
                Debug.DrawLine(ray.origin, hit.point, Color.red);

                if (hit.collider.CompareTag("Enemy"))
                {
                    if (hit.collider.gameObject.GetComponent<HPManager>() != null)
                    {
                        if (hit.collider.gameObject.TryGetComponent<ZombieManager>(out var zombie))
                        {
                            zombie.GetDamage(5);
                        }
                        else if (hit.collider.gameObject.TryGetComponent<ChaserZombieManager>(out var chaserZombie))
                        {
                            chaserZombie.GetDamage(5);
                        }
                    }
                }
            }
            else
            {
                Debug.DrawLine(ray.origin, ray.origin + ray.direction * _punchMaxDistance, Color.green);
            }
        }
    }

    private void UseRifle()
    {
        if (_player.IsAiming && !_isFiring && !_isReloading)
        {
            GameManager.Instance.PlayAfterCoroutine(() =>
            {
                Debug.Log("ReFire : " + Input.GetMouseButton(0));
                if (Input.GetMouseButton(0))
                {
                    UseRifle();
                }
            }, _rifleFireDelay * 1.1f);

            if (_remainRifleMagazine > 0)
            {
                _remainRifleMagazine--;
                UIManager.Instance.SetRemainRifleAmmoText(_remainRifleMagazine, _remainRifleAmmo);

                _player.SetRecoil(_rifleRecoilStrength, _rifleMaxRecoilAngle);
                _player.SetShake(_rifleShakeDuration, _rifleShakeMagnitude);

                _playerAnimation.Fire();

                _isFiring = true;
                GameManager.Instance.PlayAfterCoroutine(() =>
                {
                    _isFiring = false;
                }, _rifleFireDelay);

                Ray ray = new Ray(_mainCamera.transform.position, _mainCamera.transform.forward);
                RaycastHit[] hits = Physics.RaycastAll(ray, _rifleMaxDistance, _targetLayerMask);

                if (hits.Length > 0)
                {
                    System.Array.Sort(hits,
                        (a, b) => (a.distance.CompareTo(b.distance)));

                    for (int i = 0; i < hits.Length; i++)
                    {
                        if (i == 2)
                        {
                            break;
                        }

                        Debug.DrawLine(ray.origin, hits[i].point, Color.red);

                        if (hits[i].collider.CompareTag("Enemy"))
                        {
                            if (hits[i].collider.gameObject.TryGetComponent<ZombieManager>(out var zombie))
                            {
                                zombie.GetShot(30, hits[i].point);
                            }
                            else if (hits[i].collider.gameObject.TryGetComponent<ChaserZombieManager>(out var chaserZombie))
                            {
                                chaserZombie.GetShot(30, hits[i].point);
                            }
                        }
                    }
                }
                else
                {
                    Debug.DrawLine(ray.origin, ray.origin + ray.direction * _rifleMaxDistance, Color.green);
                }
            }
            else
            {
                //총알이 없음
                SoundManager.Instance.PlaySFX("EmptyBullet", transform.position);
            }
        }
    }

    public void AnimationEventUseWeapon()
    {
        ParticleManager.Instance.PlayParticle(ParticleManager.ParticleType.GunFire, _rifleFirePosition);
        SoundManager.Instance.PlaySFX("Fire", transform.position);
    }

    private void UsePistol()
    {

    }
}
