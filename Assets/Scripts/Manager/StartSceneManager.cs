using System.Collections;
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
        StartCoroutine(LoadSceneCoroutine("TutorialScene", 3.0f));
        //SceneManager.LoadScene("TutorialScene");
    }

    private IEnumerator LoadSceneCoroutine(string sceneName, float delayTime)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        yield return new WaitForSeconds(delayTime);

        asyncLoad.allowSceneActivation = true;
    }

    public void OnClickExitButton()
    {
        Application.Quit();
    }
}
