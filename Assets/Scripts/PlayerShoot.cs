using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
/*    [SerializeField] ParticleSystem _gunParticle;
    private bool _isAiming = false;
    private bool _isFiring = false;

    private int _remainBullet = 0;
    private bool _hasGun = false;
    private bool _isCurrentWeaponGun = false;

    private float _weaponMaxDistance = 100.0f;

    private float _gunFireDelay = 0.5f;


    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            UseWeapon();
        }
    }
    private void UseWeapon()
    {
        if (_isAiming && !_isFiring)
        {
            if (_remainBullet > 0)
            {
                _remainBullet--;
                UIManager.Instance.SetRemainBulletCount(_remainBullet);

                _animator.SetTrigger("IsFiring");

                _isFiring = true;
                GameManager.Instance.PlayAfterCoroutine(() =>
                {
                    _isFiring = false;
                }, 0.2f);

                _weaponMaxDistance = 1000.0f;

                Ray ray = new Ray(_mainCamera.transform.position, _mainCamera.transform.forward);
                RaycastHit[] hits = Physics.RaycastAll(ray, _weaponMaxDistance, _targetLayerMask);

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

                        //Debug.Log("HIT:" + hits[i].collider.gameObject.name);
                        Debug.DrawLine(ray.origin, hits[i].point, Color.red);

                        if (hits[i].collider.CompareTag("Enemy"))
                        {
                            HPManager hp = hits[i].collider.gameObject.GetComponent<HPManager>();
                            if (hp != null)
                            {
                                hp.ChangeHP(-10);
                            }
                        }
                    }
                }
                else
                {
                    Debug.DrawLine(ray.origin, ray.origin + ray.direction * _weaponMaxDistance, Color.green);
                }
            }
            else
            {

            }

        }
    }*/
}
