using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : TSingleton<UIManager>
{
    [SerializeField] private Image _crosshairGunImage;
    [SerializeField] private Image _crosshairImage;
    [SerializeField] private Image _hasGunIconImage;
    [SerializeField] private TextMeshProUGUI _remainBulletText;

    [SerializeField] private GameObject _pauseMenu;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        SetCrosshairActive(false);
        SetHasGunIconActive(false);
    }

    public void SetCrosshairActive(bool active)
    {
        _crosshairGunImage.gameObject.SetActive(active);
        _crosshairImage.gameObject.SetActive(!active);
    }

    public void SetHasGunIconActive(bool active)
    {
        _hasGunIconImage.gameObject.SetActive(active);
    }

    public void SetRemainBulletCount(int count, int maxCount)
    { 
        _remainBulletText.text = $"{count}/{maxCount}";
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

    }
}
