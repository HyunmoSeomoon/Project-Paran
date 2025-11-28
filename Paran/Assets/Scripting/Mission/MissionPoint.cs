using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;

public class MissionPoint : MonoBehaviour
{
    public MissionManager missionManager;
    public string missionName;
    public UnityEvent OnArrivePoint;
    public GameObject cinemachine;
    public float cameraTime;
    private bool first = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(cinemachine!=null) cinemachine.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && first)
        {
            missionManager.CompleteMission(missionName);
            OnArrivePoint?.Invoke();
            MissionViewChange();
            first = false;
        }
    }

    private void MissionViewChange()
    {
        if(cinemachine==null) Destroy(gameObject);
        else{
            cinemachine.SetActive(true);
            Invoke("BackOriginalCamera",cameraTime);
        }
    }

    private void BackOriginalCamera()
    {
        cinemachine.SetActive(false);
        Destroy(gameObject);
    }
}
