using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.EventSystems;

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


    [SerializeField] private Vector3 _boxSize = Vector3.one;
    [SerializeField] private float _castDistance = 5.0f;
    [SerializeField] private LayerMask _itemLayerMask;
    [SerializeField] private Transform _itemGetPosition;

    // ----------- 애니메이션으로 분리 -------
    [SerializeField] private Transform _aimTarget;
    //[SerializeField] private RigBuilder _rigBuilder;

    private PlayerAnimation _playerAnimation;
    private PlayerAttack _playerAttack;

    public bool IsRunning => _isRunning;
    private bool _isRunning = false;

    public bool IsAiming => _isAiming;
    private bool _isAiming = false;

    public float HorizontalMove => _horizontalMove;
    private float _horizontalMove;

    public float VerticalMove => _verticalMove;
    private float _verticalMove;

    [SerializeField] GameObject _flashLight;

    private float _currentRecoil = 0.0f;

    private float _currentRecoilStrength = 0.0f;
    private float _currentMaxRecoilAngle = 0.0f;

    private float _currentShakeDuration = 0.0f;
    private float _currentShakeMagnitude = 0.0f;
    private Coroutine _cameraShakeCoroutine;

    private bool _isDead = false;

    private float _footstepDelay = 0.5f;
    private float _currentFootstepDelay = 0.0f;

    private float _maxStamina = 100.0f;
    private float _currentStamina = 100.0f;

    void Start()
    {
        _currentDistance = _thridPersonDistance;
        _tartgetDistance = _thridPersonDistance;
        _targetFov = _defaultFov;
        _mainCamera = _cameraTransform.GetComponent<Camera>();
        _mainCamera.fieldOfView = _defaultFov;

        _flashLight.SetActive(false);
        _playerAnimation = GetComponent<PlayerAnimation>();
        _playerAttack = GetComponent<PlayerAttack>();

        GetComponent<HPManager>().DeadEvent.AddListener(OnDead);
    }

    void Update()
    {
        if (_isDead)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Pause Menu");
            UIManager.Instance.SetPauseMenu();
        }

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

        _currentStamina += 12f * Time.deltaTime;

        if (_horizontalMove != 0 || _verticalMove != 0)
        {
            _currentFootstepDelay += Time.deltaTime;
            _currentStamina -= 10f * Time.deltaTime;
            if (_isRunning)
            {
                _currentFootstepDelay += Time.deltaTime;
                _currentStamina -= 10f * Time.deltaTime;
            }
        }
        _currentStamina = Mathf.Clamp(_currentStamina, 0, _maxStamina);
        if (_currentStamina < 5.0f)
        {
            _isRunning = false;
        }

        //Debug.Log(_currentStamina);
        UIManager.Instance.SetStamina(_currentStamina / _maxStamina);

        _moveSpeed = _isRunning ? _runSpeed : _walkSpeed;

        if (_currentFootstepDelay > _footstepDelay)
        { 
            PlayFootstep();
            _currentFootstepDelay = 0.0f;
        }

        /*if (_currentRecoil > 0)
        {
            _currentRecoil -= _currentRecoilStrength * Time.deltaTime;
            _currentRecoil = Mathf.Clamp(_currentRecoil, 0, _currentMaxRecoilAngle);

            Quaternion currentRotation = _mainCamera.transform.rotation;
            Quaternion recoilRotation = Quaternion.Euler(-_currentRecoil, 0, 0);

            _mainCamera.transform.rotation = currentRotation * recoilRotation;//카메라 제어 코드를 꺼야함
        }*/
        HPManager hp = GetComponent<HPManager>();
        hp.ChangeHP(0.1f);
        UIManager.Instance.SetBloodOverlayActive(1.0f - (hp.CurrentHP / hp.MaxHP));

    }

    private void LateUpdate()
    {
        if (Time.timeScale == 0.0f || _isDead)
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

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _playerAttack.ChangeWeapon(WeaponType.Rifle);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            _playerAttack.ChangeWeapon(WeaponType.Pistol);
            AimZoomOut();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            _playerAttack.ChangeWeapon(WeaponType.Punch);

            _playerAnimation.WeaponPutAway();
            AimZoomOut();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            GetItem();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            _playerAttack.Reload();
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
        if (_playerAttack.IsReloading)
        {
            return;
        }

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
        moveDirection.y = -1;
        _characterController.Move(_moveSpeed * Time.deltaTime * moveDirection);


        _cameraTransform.position = _playerHead.position; //카메라를 머리 위치로
        _cameraTransform.rotation = Quaternion.Euler(_pitch, _yaw, 0);//카메라를 마우스 회전값에 따라

        transform.rotation = Quaternion.Euler(0, _cameraTransform.eulerAngles.y, 0);//플레이어가 항상 카메라 정면을 보도록
    }

    private void ThirdPersonMovement()
    {
        _horizontalMove = Input.GetAxis("Horizontal");
        _verticalMove = Input.GetAxis("Vertical");

        Vector3 moveDirection = transform.right * _horizontalMove + transform.forward * _verticalMove;
        moveDirection.y = -1;

        _characterController.Move(_moveSpeed * Time.deltaTime * moveDirection);

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
            if (hit.collider.name == "RifleItem")
            {
                _playerAttack.HasRifle = true;
                _playerAttack.ChangeRifleAmmo(10);
            }
            else if (hit.collider.name == "RifleAmmoItem")
            {
                _playerAttack.ChangeRifleAmmo(30);
            }
            else if (hit.collider.name == "PistolItem")
            {
                _playerAttack.HasPistol = true;
                _playerAttack.ChangeRifleAmmo(10);
            }
            else if (hit.collider.name == "PistolAmmoItem")
            {
                _playerAttack.ChangeRifleAmmo(30);
            }
        }

        if (hits.Length != 0)
        {
            SoundManager.Instance.PlaySFX("GetItem", transform.position);
        }
    }

    private void UseWeapon()//shoot으로
    {
        _playerAttack.UseWeapon();
    }

    private void GetDamage(int damage)
    {
        if (_isDead)
        {
            return;
        }

        _playerAnimation.Hit();
        HPManager hp = GetComponent<HPManager>();
        hp.ChangeHP(-damage);
        SoundManager.Instance.PlaySFX("Hit", transform.position);
    }

    private void OnDead()
    {
        _isDead = true;
        _playerAnimation.Dead();
        UIManager.Instance.SetDeadMenu();
    }

    public void PlayFootstep()
    {
        SoundManager.Instance.PlaySFX($"Foorstep_{(int)UnityEngine.Random.Range(1, 5)}", transform.position);
    }

    // ------------ Camera 관련 -------------
    public void SetRecoil(float recoilStrength, float maxRecoilAngle)
    {
        Quaternion currentRotation = _mainCamera.transform.rotation;
        Quaternion recoilRotation = Quaternion.Euler(-_currentRecoil, 0, 0);

        _mainCamera.transform.rotation = currentRotation * recoilRotation;

        _currentRecoil += recoilStrength;
        _currentRecoil = Mathf.Clamp(_currentRecoil, 0, maxRecoilAngle);
        _currentRecoilStrength = recoilStrength;
        _currentMaxRecoilAngle = maxRecoilAngle;
    }

    public void SetShake(float shakeDuration, float shakeMagnitude)
    {
        _currentShakeDuration = shakeDuration;
        _currentShakeMagnitude = shakeMagnitude;

        if (_cameraShakeCoroutine != null)
        {
            StopCoroutine(_cameraShakeCoroutine);
        }

        _cameraShakeCoroutine = StartCoroutine(CameraShake(_currentShakeDuration, _currentShakeMagnitude));
    }

    IEnumerator CameraShake(float duration, float magnitude)
    {
        float elapsed = 0.0f;
        Vector3 originalPosition = _mainCamera.transform.position;
        while (elapsed < duration)
        {
            float offsetX = UnityEngine.Random.Range(-1.0f, 1.0f) * magnitude;
            float offsetY = UnityEngine.Random.Range(-1.0f, 1.0f) * magnitude;

            _mainCamera.transform.position = originalPosition + new Vector3(offsetX, offsetY, 0);

            elapsed += Time.deltaTime;

            yield return null;
        }

        _mainCamera.transform.position = originalPosition;
    }

    private void AimZoomIn()
    {
        if (!_playerAttack.CanZoomAim())
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
            GetDamage(40);
        }

        if (other.CompareTag("StageTrigger"))
        { 
            StageManager.Instance.CreateStage();
            other.gameObject.SetActive(false);
        }
    }
}