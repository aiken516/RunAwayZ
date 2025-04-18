using TMPro;
using UnityEngine;

public class GameSettingUIManager : MonoBehaviour
{
    [SerializeField] private GameObject _gameSettingUI;

    [SerializeField] private TextMeshProUGUI _resolutionText;
    [SerializeField] private TextMeshProUGUI _graphicsText;
    [SerializeField] private TextMeshProUGUI _fullScreenText;

    private int _resolutionIndex = 1;
    private int _graphicsIndex = 0;
    private bool _isFullScreen = true;

    private string[] _resolutions = new string[]
    {
        "1280x720",
        "1920x1080",
        "2560x1440",
        "3640x2160"
    };

    private string[] _graphics = new string[]
    {
        "Low",
        "Medium",
        "High"
    };

    private void Awake()
    {
        //LoadSetting();
        //ApplySetting();
    }

    public void OnSetting()
    {
        SoundManager.Instance.PlaySFX("Button", transform.position);
        _gameSettingUI.SetActive(!_gameSettingUI.activeSelf);
    }

    public void OnResolutionLeftClick()
    {
        _resolutionIndex = Mathf.Max(0, _resolutionIndex - 1);
        _resolutionText.text = _resolutions[_resolutionIndex];
    }

    public void OnResolutionRightClick()
    {
        _resolutionIndex = Mathf.Min(_resolutions.Length - 1, _resolutionIndex + 1);
        _resolutionText.text = _resolutions[_resolutionIndex];
    }

    public void OnGraphicsLeftClick()
    {
        _graphicsIndex = Mathf.Max(0, _graphicsIndex - 1);
        _graphicsText.text = _graphics[_graphicsIndex];
    }

    public void OnGraphicsRightClick()
    {
        _graphicsIndex = Mathf.Min(_graphics.Length - 1, _graphicsIndex + 1);
        _graphicsText.text = _graphics[_graphicsIndex];
    }

    public void OnFullScreenClick()
    {
        _isFullScreen = !_isFullScreen;
        _fullScreenText.text = _isFullScreen ? "On" : "Off";
    }

    private void SaveSetting()
    {
        PlayerPrefs.SetInt("Resolution", _resolutionIndex);
        PlayerPrefs.SetInt("Graphics", _graphicsIndex);
        PlayerPrefs.SetInt("FullScreen", _isFullScreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadSetting()
    {
        _resolutionIndex = PlayerPrefs.GetInt("Resolution", 0);
        _graphicsIndex = PlayerPrefs.GetInt("Graphics", 0);
        _isFullScreen = PlayerPrefs.GetInt("FullScreen", 1) == 1;

        _resolutionText.text = _resolutions[_resolutionIndex];
        _graphicsText.text = _graphics[_graphicsIndex];
        _fullScreenText.text = _isFullScreen ? "On" : "Off";
    }

    private void ApplySetting()
    {
        string[] resolution = _resolutions[_resolutionIndex].Split('x');
        Screen.SetResolution(int.Parse(resolution[0]), int.Parse(resolution[1]), _isFullScreen);
        QualitySettings.SetQualityLevel(_graphicsIndex);
    }


    public void OnApplyClick()
    {
        SoundManager.Instance.PlaySFX("Button", transform.position);
        ApplySetting();
        SaveSetting();
    }
}
