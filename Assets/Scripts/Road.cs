using Unity.AI.Navigation;
using UnityEngine;

public class Road : MonoBehaviour
{
    public Transform NextStagePoint => _nextStagePoint;
    [SerializeField] private Transform _nextStagePoint;

    [SerializeField] private NavMeshLink _navMeshLinkFront;
    [SerializeField] private NavMeshLink _navMeshLinkBackward;

    public StageType Type { get; private set; }

    private void Start()
    {
        GameManager.Instance.PlayAfterCoroutine(() =>
        {
            UpdateLinker();
        }, 1.0f);
    }

    public void SetRoad(StageType type)
    {
        Type = type;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        { 
            StageManager.Instance.CreateStage();
            GetComponent<Collider>().enabled = false;
        }
    }

    private void UpdateLinker()
    {
        _navMeshLinkFront.enabled = false;
        _navMeshLinkBackward.enabled = false;

        _navMeshLinkFront.enabled = true;
        _navMeshLinkBackward.enabled = true;

        GameManager.Instance.PlayAfterCoroutine(() =>
        {
            UpdateLinker();
        }, 1.0f);
    }
}
