using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private float _mouseSeneitivity = 100.0f;
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private Transform _playerHead;

    [SerializeField] private float _defaultFov = 60.0f;

    [SerializeField] private float _thridPersonDistance = 5.0f;
    [SerializeField] private Vector3 _thridPersonOffset = new Vector3(0f, 1.5f, 0f);
    [SerializeField] private Transform _playerLookObject;

    [SerializeField] private float _zoomDistance = 1.0f;
    [SerializeField] private float _zoomSpeed = 5.0f;
    [SerializeField] private float _zoomFov = 30.0f;

    [SerializeField] private float _walkSpeed = 2.0f;
    [SerializeField] private float _runSpeed = 4.0f;

    private float _currentDistance;
    private float _tartgetDistance;
    private float _targetFov;
    private bool _isZoomed = false;
    private Coroutine _zoomCoroutine;
    private Coroutine _zoomAnimationCoroutine;
    private Camera _mainCamera;

    private float _pitch = 0.0f;// 위아래 회전 값
    private float _yaw = 0.0f;//좌우 회전 값
    private bool _isFirstPerson = false;
    private bool _isRotaterAroundPlayer = false; //카메라가 플레이어 주위를 회전하는가

    public float Gravity = -9.81f;
    public float JumpHeight = 2.0f;
    public Vector3 _velocity;
    public bool _isGrounded = false;

    private float _moveSpeed = 0.0f;

    [SerializeField] private LayerMask _targetLayerMask;

    //[SerializeField] private MultiAimConstraint _multiAimConstraint;

    [SerializeField] private Vector3 _boxSize = Vector3.one;
    [SerializeField] private float _castDistance = 5.0f;
    [SerializeField] private LayerMask _itemLayerMask;
    [SerializeField] private Transform _itemGetPosition;

    // ---- Shoot으로 분리 ----
    private bool _isAiming = false;
    private bool _isFiring = false;

    private bool _isPunching = false;
    private float _punchDelay = 1.0f;

    private int _maxBullet = 120;
    private int _remainBullet = 0;

    private bool _hasGun = false;

    private bool _isCurrentWeaponGun = false;

    [SerializeField] ParticleSystem _gunParticle;

    private float _gunFireDelay = 0.2f;

    private float _weaponMaxDistance = 100.0f;
    // -------------------------------
    // ----------- 애니메이션으로 분리 -------
    [SerializeField] private Transform _aimTarget;
    //[SerializeField] private RigBuilder _rigBuilder;

    private PlayerAnimation _playerAnimation;

    public bool IsRunning => _isRunning;
    private bool _isRunning = false;
    public float HorizontalMove => _horizontalMove;
    private float _horizontalMove;
    public float VerticalMove => _verticalMove;
    private float _verticalMove;

    [SerializeField] ParticleSystem _damageParticle;
    [SerializeField] GameObject _flashLight;

    void Start()
    {
        _currentDistance = _thridPersonDistance;
        _tartgetDistance = _thridPersonDistance;
        _targetFov = _defaultFov;
        _mainCamera = _cameraTransform.GetComponent<Camera>();
        _mainCamera.fieldOfView = _defaultFov;

        _flashLight.SetActive(false);
        _playerAnimation = GetComponent<PlayerAnimation>();
    }

    void Update()
    {
        if (Time.timeScale == 0.0f)
        {
            return;
        }

        _isGrounded = _characterController.isGrounded;
        if (_isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2.0f;
        }

        GetMouseAxis();
        OnKeyDown();
        GetMouseClick();

        _moveSpeed = _isRunning ? _runSpeed : _walkSpeed;
    }

    private void LateUpdate()
    {
        if (Time.timeScale == 0.0f)
        {
            return;
        }

        if (_isFirstPerson)
        {
            FirstPersonMovement();
        }
        else
        {
            ThirdPersonMovement();
        }
    }

    private void OnKeyDown()
    {
        if (Input.GetKey(KeyCode.LeftShift) && !_isAiming)
        {
            _isRunning = true;
        }
        else
        {
            _isRunning = false;
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            _isFirstPerson = !_isFirstPerson;
            Debug.Log(_isFirstPerson ? "First Person View" : "Thrid Person View");
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            _isRotaterAroundPlayer = !_isRotaterAroundPlayer;
            Debug.Log(_isRotaterAroundPlayer ? "Rotate Around Player" : "Rotate Follow Player View");
        }
        
        if (Input.GetKeyDown(KeyCode.F))
        {
            _flashLight.SetActive(!_flashLight.activeSelf);
        }

        if (Input.GetKeyDown(KeyCode.Alpha1) && _hasGun)
        {
            _playerAnimation.WeaponPullOut(() => { _isCurrentWeaponGun = true; });
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            _playerAnimation.WeaponPutAway();
            _isCurrentWeaponGun = false;
            AimZoomOut();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            GetItem();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        { 
            Debug.Log("Pause Menu");
            UIManager.Instance.SetPauseMenu();
        }
    }

    private void GetMouseAxis()
    {
        float mouseX = Input.GetAxis("Mouse X") * _mouseSeneitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * _mouseSeneitivity * Time.deltaTime;

        _yaw += mouseX;
        _pitch -= mouseY;

        _pitch = Mathf.Clamp(_pitch, -45f, 45f);
    }

    private void GetMouseClick()
    {
        if (Input.GetMouseButtonDown(1))
        {
            AimZoomIn();
        }
        else if (Input.GetMouseButtonUp(1))
        {
            AimZoomOut();
        }

        if (Input.GetMouseButtonDown(0))
        {
            UseWeapon();
        }
    }


    private void FirstPersonMovement()
    {
        _horizontalMove = Input.GetAxis("Horizontal");
        _verticalMove = Input.GetAxis("Vertical");

        Vector3 moveDirection = _cameraTransform.right * _horizontalMove + _cameraTransform.forward * _verticalMove;
        moveDirection.y = 0;
        _characterController.Move(_moveSpeed * Time.deltaTime * moveDirection);


        _cameraTransform.position = _playerHead.position; //카메라를 머리 위치로
        _cameraTransform.rotation = Quaternion.Euler(_pitch, _yaw, 0);//카메라를 마우스 회전값에 따라

        transform.rotation = Quaternion.Euler(0, _cameraTransform.eulerAngles.y, 0);//플레이어가 항상 카메라 정면을 보도록
    }

    private void ThirdPersonMovement()
    {
        _horizontalMove = Input.GetAxis("Horizontal");
        _verticalMove = Input.GetAxis("Vertical");

        Vector3 move = transform.right * _horizontalMove + transform.forward * _verticalMove;
        _characterController.Move(_moveSpeed * Time.deltaTime * move);

        UpdateCameraPosition();
    }

    private void GetItem()
    {
        _playerAnimation.GetItem();
    }

    private void AnimationEventGetItme()
    {
        Vector3 origin = _itemGetPosition.position;
        Vector3 direction = _itemGetPosition.forward;

        RaycastHit[] hits = Physics.BoxCastAll(
            origin, _boxSize / 2, direction, Quaternion.identity, _castDistance, _itemLayerMask);
        DebugBox(origin, direction);

        foreach (RaycastHit hit in hits)
        {
            hit.collider.gameObject.SetActive(false);
            Debug.Log($"Get Item : {hit.collider.name}");
            if (hit.collider.name == "Gun")
            {
                _hasGun = true;
                UIManager.Instance.SetHasGunIconActive(true);
                ChangeBullet(20);
            }
            else if (hit.collider.name == "Ammo")
            {
                ChangeBullet(60);
            }
        }

        if (hits.Length != 0)
        {
            SoundManager.Instance.PlaySFX("GetItem", transform.position);
        }
    }

    private void ChangeBullet(int count)
    {
        _remainBullet += count;
        if (_remainBullet > _maxBullet)
        {
            _remainBullet = _maxBullet;
        }

        UIManager.Instance.SetRemainBulletCount(_remainBullet, _maxBullet);
    }

    private void UseWeapon()//shoot으로
    {
        if (_isAiming && !_isFiring)
        {
            if (_remainBullet > 0)
            {
                ChangeBullet(-1);

                _playerAnimation.Fire();

                _isFiring = true;
                GameManager.Instance.PlayAfterCoroutine(() =>
                {
                    _isFiring = false;
                }, _gunFireDelay);

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
                            if (hits[i].collider.gameObject.GetComponent<HPManager>() != null)
                            {
                                if (hits[i].collider.gameObject.TryGetComponent<ZombieManager>(out var zombie))
                                {
                                    zombie.GetDamage(10);
                                    ParticleSystem particle = Instantiate(_damageParticle, hits[i].point, Quaternion.identity);
                                    SoundManager.Instance.PlaySFX("Hit", hits[i].point);
                                }
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

        if (!_isAiming && !_isCurrentWeaponGun && !_isPunching)
        {
            _weaponMaxDistance = 4.0f;

            Ray ray = new Ray(_mainCamera.transform.position, _mainCamera.transform.forward);
            RaycastHit hit;

            _isPunching = true;
            GameManager.Instance.PlayAfterCoroutine(() =>
            {
                _isPunching = false;
            }, _punchDelay);

            _playerAnimation.Punch();

            if (Physics.Raycast(_mainCamera.transform.position, 
                _mainCamera.transform.forward, out hit, _weaponMaxDistance))
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
                    }
                }
            }
            else
            {
                Debug.DrawLine(ray.origin, ray.origin + ray.direction * _weaponMaxDistance, Color.green);
            }
        }
    }

    public void AnimationEventUseWeapon()
    {
        _gunParticle.Play();
        SoundManager.Instance.PlaySFX("Fire", transform.position);
    }

    // ------------ Camera 관련 -------------

    private void AimZoomIn()
    {
        if (!_hasGun || !_isCurrentWeaponGun)
        {
            return;
        }

        _isAiming = true;

        //기존 코루틴 제거
        if (_zoomCoroutine != null)
        {
            StopCoroutine(_zoomCoroutine);
        }

        if (_isFirstPerson)
        {
            //1인칭이면 pov를 조절
            SetTargetPov(_zoomFov);
            _zoomCoroutine = StartCoroutine(ZoomFieldOfView(_targetFov));
        }
        else
        {
            //3인칭이면 카메라 거리를 조절
            SetTargetDistance(_zoomDistance);
            _zoomCoroutine = StartCoroutine(ZoomCamera(_tartgetDistance));
        }

        _playerAnimation.ZoomIn();

        UIManager.Instance.SetCrosshairActive(true);
    }

    private void AimZoomOut()
    {
        //기존 코루틴 제거
        if (_zoomCoroutine != null)
        {
            StopCoroutine(_zoomCoroutine);
        }

        if (_isFirstPerson)
        {
            //1인칭이면 pov를 조절
            SetTargetPov(_defaultFov);
            _zoomCoroutine = StartCoroutine(ZoomFieldOfView(_targetFov));
        }
        else
        {
            //3인칭이면 카메라 거리를 조절
            SetTargetDistance(_thridPersonDistance);
            _zoomCoroutine = StartCoroutine(ZoomCamera(_tartgetDistance));
        }

        _isAiming = false;

        _playerAnimation.ZoomOut();

        UIManager.Instance.SetCrosshairActive(false);
    }

    private void UpdateCameraPosition()
    {
        if (_isRotaterAroundPlayer)
        {
            //카메라가 플레이어의 오른쪽에서 회전하도록 설정
            Vector3 direction = new(0, 0, -_currentDistance);
            Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0.0f);

            //카메라가 플레이어의 오른쪽에서 고정된 위치로 이동
            _cameraTransform.position = transform.position + _thridPersonOffset + rotation * direction;

            //카메라가 플레이어의 위치를 따라가도록 설정
            _cameraTransform.LookAt(transform.position + new Vector3(0, _thridPersonOffset.y, 0));
        }
        else
        {
            //플레이어가 직접 회전하는 상태
            transform.rotation = Quaternion.Euler(0f, _yaw, 0);
            Vector3 direction = new Vector3(0, 0, -_currentDistance);

            _cameraTransform.position = _playerLookObject.position + _thridPersonOffset +
                Quaternion.Euler(_pitch, _yaw, 0) * direction;
            _cameraTransform.LookAt(_playerLookObject.position + new Vector3(0, _thridPersonOffset.y, 0));

            UpdateAimTarget();
        }
    }

    public void SetTargetDistance(float distance)
    {
        _tartgetDistance = distance;
    }

    public void SetTargetPov(float pov)
    {
        _targetFov = pov;
    }

    private void UpdateAimTarget()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        _aimTarget.position = ray.GetPoint(10.0f);
    }

    private IEnumerator ZoomCamera(float targetDistance)
    {
        while (Mathf.Abs(_currentDistance - targetDistance) > 0.01f)
        {
            //카메라 거리를 조절해서 줌을 관리
            _currentDistance = Mathf.Lerp(_currentDistance, targetDistance, Time.deltaTime * _zoomSpeed);
            yield return null;
        }

        _currentDistance = targetDistance;
    }

    private IEnumerator ZoomFieldOfView(float targetFov)
    {
        while (Mathf.Abs(_mainCamera.fieldOfView - targetFov) > 0.01f)
        {
            //fov 조절해 줌을 관리
            _mainCamera.fieldOfView = Mathf.Lerp(_mainCamera.fieldOfView, targetFov, Time.deltaTime * _zoomSpeed);
            yield return null;
        }

        _mainCamera.fieldOfView = targetFov;
    }

    private void DebugBox(Vector3 origin, Vector3 direction)
    {
        Vector3 endPoint = origin + direction * _castDistance;
        Vector3[] corners = new Vector3[8];
        corners[0] = origin + new Vector3(-_boxSize.x, -_boxSize.y, -_boxSize.z) / 2;
        corners[1] = origin + new Vector3(_boxSize.x, -_boxSize.y, -_boxSize.z) / 2;
        corners[2] = origin + new Vector3(-_boxSize.x, _boxSize.y, -_boxSize.z) / 2;
        corners[3] = origin + new Vector3(_boxSize.x, _boxSize.y, -_boxSize.z) / 2;
        corners[4] = origin + new Vector3(-_boxSize.x, -_boxSize.y, _boxSize.z) / 2;
        corners[5] = origin + new Vector3(_boxSize.x, -_boxSize.y, _boxSize.z) / 2;
        corners[6] = origin + new Vector3(-_boxSize.x, _boxSize.y, _boxSize.z) / 2;
        corners[7] = origin + new Vector3(_boxSize.x, _boxSize.y, _boxSize.z) / 2;

        Debug.DrawLine(corners[0], corners[1], Color.green, 3.0f);
        Debug.DrawLine(corners[1], corners[3], Color.green, 3.0f);
        Debug.DrawLine(corners[3], corners[3], Color.green, 3.0f);
        Debug.DrawLine(corners[2], corners[0], Color.green, 3.0f);
        Debug.DrawLine(corners[4], corners[5], Color.green, 3.0f);
        Debug.DrawLine(corners[5], corners[7], Color.green, 3.0f);
        Debug.DrawLine(corners[7], corners[6], Color.green, 3.0f);
        Debug.DrawLine(corners[6], corners[4], Color.green, 3.0f);
        Debug.DrawLine(corners[0], corners[4], Color.green, 3.0f);
        Debug.DrawLine(corners[1], corners[5], Color.green, 3.0f);
        Debug.DrawLine(corners[2], corners[6], Color.green, 3.0f);
        Debug.DrawLine(corners[3], corners[7], Color.green, 3.0f);

        Debug.DrawRay(origin, direction * _castDistance, Color.green);
    }

    // ------------------------------------------

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Attack"))
        {
            _playerAnimation.Hit();
            GetComponent<HPManager>().ChangeHP(-10);
        }

        if (other.CompareTag("StageTrigger"))
        { 
            StageManager.Instance.CreateStage();
            other.gameObject.SetActive(false);
        }
    }
}