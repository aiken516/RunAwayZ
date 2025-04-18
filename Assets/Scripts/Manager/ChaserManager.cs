using System.Collections.Generic;
using UnityEngine;

public class ChaserManager : TSingleton<ChaserManager>
{
    public Transform RespawnPoint => _respawnPoint;
    [SerializeField] private Transform _respawnPoint;

    [SerializeField] private List<ChaserZombieManager> _chaserZombieList;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
