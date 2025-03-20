using System.Collections.Generic;
using UnityEngine;

public enum StageType
{
    Straight = 0,
    Right = 1,
    Left = 2
}

public class StageManager : TSingleton<StageManager>
{
    [SerializeField] private GameObject[] _stageObjectPrefabs;
    [SerializeField] private GameObject[] _stageBendRightObjectPrefabs;
    [SerializeField] private GameObject[] _stageBendLeftObjectPrefabs;

    [SerializeField] private GameObject _roadPrefab;
    [SerializeField] private GameObject _roadBendRightPrefab;
    [SerializeField] private GameObject _roadBendLeftPrefab;

    [SerializeField] private Transform _stageObjectParent;
    [SerializeField] private Transform _roadParent;

    [SerializeField] private List<GameObject> _stageObjectPool;
    [SerializeField] private List<GameObject> _stageBendRightObjectPool;
    [SerializeField] private List<GameObject> _stageBendLeftObjectPool;

    [SerializeField] private List<GameObject> _roadObjectPool;
    [SerializeField] private List<GameObject> _roadBendRightObjectPool;
    [SerializeField] private List<GameObject> _roadBendLeftObjectPool;


    private StageType _prevStage = StageType.Straight;//두 번 중 한 번은 꺾도록

    private StageType _prevBendStage = StageType.Straight;//두 번 꺾어서 원이 되지 않도록 관리

    [SerializeField] private Transform _startStageGeneratePoint;
    private Transform _nextStageGeneratePoint;

    private void Start()
    {
        _nextStageGeneratePoint = _startStageGeneratePoint;
        CreateStage();
    }

    public bool CreateStage()
    {
        int randomIndex = 0;

        if (_prevStage == StageType.Straight)
        {
            randomIndex = Random.Range(1, 3);
            if ((int)_prevBendStage == randomIndex)
            {
                randomIndex = (randomIndex == 1) ? 2 : 1;
            }
        }

        StageType stageType = (StageType)randomIndex;

        _prevStage = stageType;
        if (randomIndex != 0)
        {
            _prevBendStage = stageType;
        }

        GameObject stageObject = GetStageFromPool(stageType);
        stageObject.SetActive(true);
        stageObject.transform.SetPositionAndRotation(_nextStageGeneratePoint.position, _nextStageGeneratePoint.rotation);
        
        GameObject roadObject = GetRoadFromPool(stageType);
        roadObject.SetActive(true);
        roadObject.transform.SetPositionAndRotation(_nextStageGeneratePoint.position, _nextStageGeneratePoint.rotation);
        Road road = roadObject.GetComponent<Road>();

        _nextStageGeneratePoint = road.NextStagePoint;
        Debug.Log(_nextStageGeneratePoint.position);

        return true;
    }

    private GameObject GetStageFromPool(StageType type)
    {
        GameObject returnObject;

        GameObject[] prefabs;
        List<GameObject> objectPool;

        if (type == StageType.Right)
        {
            objectPool = _stageBendRightObjectPool;
            prefabs = _stageBendRightObjectPrefabs;
        }
        else if (type == StageType.Left)
        {
            objectPool = _stageBendLeftObjectPool;
            prefabs = _stageBendLeftObjectPrefabs;
        }
        else
        {
            objectPool = _stageObjectPool;
            prefabs = _stageObjectPrefabs;
        }

        if (objectPool.Count <= 0)
        {
            int randomIndex = Random.Range(0, prefabs.Length);

            objectPool.Add(Instantiate(prefabs[randomIndex], _stageObjectParent));
        }

        returnObject = objectPool[0];
        objectPool.Remove(returnObject);

        return returnObject;
    }

    private GameObject GetRoadFromPool(StageType type)
    {
        GameObject returnObject;

        GameObject prefab;
        List<GameObject> objectPool;

        if (type == StageType.Right)
        {
            objectPool = _roadBendRightObjectPool;
            prefab = _roadBendRightPrefab;
        }
        else if (type == StageType.Left)
        {
            objectPool = _roadBendLeftObjectPool;
            prefab = _roadBendLeftPrefab;
        }
        else
        {
            objectPool = _roadObjectPool;
            prefab = _roadPrefab;
        }

        if (objectPool.Count <= 0)
        {
            objectPool.Add(Instantiate(prefab, _roadParent));
        }

        returnObject = objectPool[0];
        objectPool.Remove(returnObject);

        return returnObject;
    }

    public void ReturnStageToPool(GameObject stageObject, StageType type)
    {
        stageObject.SetActive(false);
        if (type == StageType.Right)
        {
            _stageBendRightObjectPool.Add(stageObject);
        }
        else if (type == StageType.Left)
        {
            _stageBendLeftObjectPool.Add(stageObject);
        }
        else
        {
            _stageObjectPool.Add(stageObject);
        }
    }

    public void ReturnRoadToPool(GameObject roadObject, StageType type)
    {
        roadObject.SetActive(false);
        if (type == StageType.Right)
        {
            _roadBendRightObjectPool.Add(roadObject);
        }
        else if (type == StageType.Left)
        {
            _roadBendLeftObjectPool.Add(roadObject);
        }
        else
        {
            _roadObjectPool.Add(roadObject);
        }
    }
}
