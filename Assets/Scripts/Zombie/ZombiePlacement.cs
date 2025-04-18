using UnityEngine;

public class ZombiePlacement : MonoBehaviour
{
    [SerializeField] private GameObject[] _zombies;

    void Start()
    {
        GameManager.Instance.PlayAfterCoroutine(() =>
        {
            foreach (GameObject zombie in _zombies)
            {
                zombie.SetActive(true);
            }
        }, 
        0.5f);
    }
}
