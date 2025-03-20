using UnityEngine;

public class TSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<T>();
                if (_instance == null)
                {
                    GameObject manager = new GameObject(typeof(T).Name);
                    _instance = manager.AddComponent<T>();
                }
            }
            return _instance;
        }
    }

    [SerializeField] private bool _isDonDestroyOnLoad = false;

    protected void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            if (_isDonDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
}