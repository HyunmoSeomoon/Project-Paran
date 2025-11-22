using UnityEngine;

public class MissionPoint : MonoBehaviour
{
    public MissionManager missionManager;
    public string missionName;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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
            Destroy(gameObject);
        }
    }
}
