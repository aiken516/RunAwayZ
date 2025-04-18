using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private Transform _door;
    [SerializeField] private Transform _doorFrontPoint;
    [SerializeField] private Transform _doorBackPoint;

    private Coroutine _doorCoroutine;

    private Vector3 _startAngle;
    private bool _isOpen = false;

    private void Awake()
    {
        _startAngle = _door.localEulerAngles;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (_doorCoroutine != null)
                {
                    StopCoroutine(_doorCoroutine);
                }

                if (_isOpen)
                {
                    Debug.Log("Door Close");
                    _isOpen = false;
                    _doorCoroutine = StartCoroutine(DoorCoroutine(2.0f, _startAngle));
                }
                else
                {
                    Debug.Log("Door Open");
                    _isOpen = true;
                    float angle = 90.0f;
                    if (Vector3.Distance(_doorFrontPoint.position, other.gameObject.transform.position) <=
                        Vector3.Distance(_doorBackPoint.position, other.gameObject.transform.position))
                    {
                        angle = -90.0f;
                    }

                    _doorCoroutine = StartCoroutine(DoorCoroutine(2.0f, new Vector3(_door.localEulerAngles.x, angle, _door.localEulerAngles.z)));
                }

            }
        }
    }

    IEnumerator DoorCoroutine(float duration, Vector3 angle)
    {
        float elapsedTime = 0.0f;
        Vector3 startAngle = _door.localEulerAngles;
        if (startAngle.y == 270.0f)
        { 
            startAngle.y = -90.0f;
        }

        while (Mathf.Abs(_door.localEulerAngles.y - angle.y) > 0.05f)
        {
            elapsedTime += Time.deltaTime;
            _door.localEulerAngles = Vector3.Lerp(startAngle, angle, elapsedTime / duration);
            Debug.Log($"{_door.localEulerAngles}, {startAngle}, {angle}, {elapsedTime / duration}");

            yield return null;
        }

        _door.localEulerAngles = angle;
    }
}
