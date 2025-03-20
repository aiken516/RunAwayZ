using UnityEngine;

public class Road : MonoBehaviour
{
    public Transform NextStagePoint => _nextStagePoint;
    [SerializeField] private Transform _nextStagePoint;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        { 
            StageManager.Instance.CreateStage();
            GetComponent<Collider>().enabled = false;
        }
    }
}
