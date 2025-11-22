using Unity.Cinemachine;
using UnityEngine;

public class MissionPoint : MonoBehaviour
{
    public MissionManager missionManager;
    public string missionName;
    public GameObject cinemachine;
    public float cameraTime;
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
        if (other.CompareTag("Player"))
        {
            missionManager.CompleteMission(missionName);
            MissionViewChange();
        }
    }

    private void MissionViewChange()
    {
        if(cinemachine==null) Destroy(gameObject);
        
        cinemachine.SetActive(true);
        Invoke("BackOriginalCamera",cameraTime);

    }

    private void BackOriginalCamera()
    {
        cinemachine.SetActive(false);
        Destroy(gameObject);
    }
}
