using System;
using System.Collections;
using UnityEngine;

public class GameManager : TSingleton<GameManager>
{
    public void PlayAfterCoroutine(Action action, float time) => StartCoroutine(PlayCoroutine(action, time));
    private IEnumerator PlayCoroutine(Action action, float time)
    {
        yield return new WaitForSeconds(time);

        action();
    }
}
