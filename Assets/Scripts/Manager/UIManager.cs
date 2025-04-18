using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : TSingleton<UIManager>
{
    [SerializeField] private Image _crosshairGunImage;
    [SerializeField] private Image _crosshairImage;

    [SerializeField] private Image _bloodOverlayImage;


    [SerializeField] private GameObject _rifleUI;
    [SerializeField] private TextMeshProUGUI _remainRifleAmmoText;

    [SerializeField] private GameObject _pistolUI;
    [SerializeField] private TextMeshProUGUI _remainpistolAmmoText;

    [SerializeField] private GameObject _punchUI;

    [SerializeField] private GameObject _pauseMenu;

    [SerializeField] private GameObject _gameOverUI;
    [SerializeField] private TextMeshProUGUI _gameOverKillCount;

    [SerializeField] private Slider _staminaSlider;

    private int _zombieKillCount = 0;

    private Coroutine _timeCoroutine = null;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        SetCrosshairActive(false);
    }

    public void SetCrosshairActive(bool active)
    {
        _crosshairGunImage.gameObject.SetActive(active);
        _crosshairImage.gameObject.SetActive(!active);
    }

    public void SetPauseMenu()
    {
        if (_pauseMenu.activeSelf)
        {
            Cursor.lockState = CursorLockMode.Locked;
            _pauseMenu.SetActive(false);
            Time.timeScale = 1.0f;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            _pauseMenu.SetActive(true);
            Time.timeScale = 0.0f;
        }
    }

    public void OnPauseMenuContinueButtonClick()
    {
        SetPauseMenu();
    }

    public void OnPauseMenuMenuButtonClick()
    {
        if (_timeCoroutine != null)
        {
            StopCoroutine(_timeCoroutine);
        }

        Time.timeScale = 1.0f;
        SceneManager.LoadScene("StartScene");
    }

    public void SetWeaponUI(WeaponType currentWeapon)
    {
        if (currentWeapon == WeaponType.Punch)
        {
            _rifleUI.SetActive(false);
            _pistolUI.SetActive(false);
            _punchUI.SetActive(true);
        }
        else if (currentWeapon == WeaponType.Rifle)
        {
            _punchUI.SetActive(false);
            _pistolUI.SetActive(false);
            _rifleUI.SetActive(true);
        }
        else if (currentWeapon == WeaponType.Pistol)
        {
            _rifleUI.SetActive(false);
            _punchUI.SetActive(false);
            _pistolUI.SetActive(true);
        }
    }

    public void SetBloodOverlayActive(float hpPercentage)
    {
        _bloodOverlayImage.color = new Color(1.0f, 1.0f, 1.0f, hpPercentage);
    }

    public void SetRemainRifleAmmoText(int remainMagazine, int remainAmmo)
    {
        _remainRifleAmmoText.text = $"{remainMagazine}/{remainAmmo}";
    }

    public void SetRemainPistolAmmoText(int remainMagazine, int remainAmmo)
    {
        _remainpistolAmmoText.text = $"{remainMagazine}/{remainAmmo}";
    }

    public void SetDeadMenu()
    {
        Cursor.lockState = CursorLockMode.None;
        _gameOverUI.SetActive(true);
        _gameOverKillCount.text = $"Zombie Kill Count:\n{_zombieKillCount}";
        _timeCoroutine = StartCoroutine(PlayerDeadCoroutine());
    }

    private IEnumerator PlayerDeadCoroutine()
    {
        float duration = 4.0f;
        float startTime = Time.unscaledTime;
        float startScale = 0.2f;
        float targetScale = 1.0f;

        while (Time.unscaledTime < startTime + duration)
        {
            float t = (Time.unscaledTime - startTime) / duration;
            Time.timeScale = Mathf.Lerp(startScale, targetScale, t);
            yield return null; // 한 프레임 대기
        }

        Time.timeScale = targetScale; // 최종 값 보정
    }

    public void KillZombie()
    {
        _zombieKillCount++;
        Debug.Log($"Kill Zombie Count : {_zombieKillCount}");
    }

    public void OnRetryButtonClick()
    {
        if (_timeCoroutine != null)
        {
            StopCoroutine(_timeCoroutine);
        }

        Time.timeScale = 1.0f;

        SceneManager.LoadScene("TutorialScene");
    }

    public void SetStamina(float v)
    {
        //Debug.Log($"Stamina : {v}");
        _staminaSlider.value = v;
    }
}
