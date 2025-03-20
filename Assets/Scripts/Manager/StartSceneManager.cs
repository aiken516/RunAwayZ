using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartSceneManager : MonoBehaviour
{
    [SerializeField] private Image _fade;

    private void Start()
    {
        SoundManager.Instance.PlayBGM("MainBGM");
    }

    public void OnClickStartButton()
    {
        _fade.gameObject.SetActive(true);
        Invoke(nameof(ChangeScene), 3.0f);
    }

    private void ChangeScene()
    {
        SceneManager.LoadScene("TutorialScene");
    }

    public void OnClickExitButton()
    {
        Application.Quit();
    }
}
