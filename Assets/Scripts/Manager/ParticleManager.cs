using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ParticleManager;

public class ParticleManager : TSingleton<ParticleManager>
{
    public enum ParticleType
    {
        Explosion = 0,
        EnemyHit = 1,
        GunFire = 2
    }

    [SerializeField] private Transform _poolParant;

    [SerializeField] private GameObject _explosionParticle;
    [SerializeField] private GameObject _hitParticle;
    [SerializeField] private GameObject _gunFireParticle;

    [SerializeField] private int _poolSize = 30;

    private Dictionary<ParticleType, GameObject> _particleDict = new();
    private Dictionary<ParticleType, Queue<GameObject>> _particlePoolDict = new();

    void Awake()
    {
        base.Awake();

        Debug.Log("Particle Init");


        _particleDict.Add(ParticleType.Explosion, _explosionParticle);
        _particleDict.Add(ParticleType.EnemyHit, _hitParticle);
        _particleDict.Add(ParticleType.GunFire, _gunFireParticle);

        foreach (ParticleType particleType in _particleDict.Keys)
        {
            Queue<GameObject> pool = new();
            for (int i = 0; i < _poolSize; i++)
            {
                GameObject go = Instantiate(_particleDict[particleType], _poolParant);
                go.SetActive(false);
                pool.Enqueue(go);
            }
            _particlePoolDict.Add(particleType, pool);
        }
    }

    public void PlayParticle(ParticleType type, Vector3 position)
    {
        if (_particleDict.ContainsKey(type))
        {
            GameObject particleObject = _particlePoolDict[type].Dequeue();
            if (particleObject != null)
            {
                particleObject.transform.position = position;
                ParticleSystem particleSystem = particleObject.GetComponent<ParticleSystem>();

                if (particleSystem.isPlaying)
                { 
                    particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
                particleObject.SetActive(true);
                particleSystem.Play();
                StartCoroutine(ParticleEnd(type, particleObject, particleSystem));
            }
        }
    }

    public void PlayParticle(ParticleType type, Transform parent)
    {
        if (_particleDict.ContainsKey(type))
        {
            GameObject particleObject = _particlePoolDict[type].Dequeue();
            if (particleObject != null)
            {
                particleObject.transform.SetParent(parent);
                particleObject.transform.localPosition = Vector3.zero;
                particleObject.transform.localEulerAngles = Vector3.zero;

                ParticleSystem particleSystem = particleObject.GetComponent<ParticleSystem>();

                if (particleSystem.isPlaying)
                {
                    particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
                particleObject.SetActive(true);
                particleSystem.Play();
                StartCoroutine(ParticleEnd(type, particleObject, particleSystem));
            }
        }
    }

    IEnumerator ParticleEnd(ParticleType type, GameObject particleObject, ParticleSystem particleSystem)
    {
        yield return new WaitForSeconds(particleSystem.main.duration);
        particleSystem.Stop();

        particleObject.SetActive(false);
        _particlePoolDict[type].Enqueue(particleObject);
    }
}
