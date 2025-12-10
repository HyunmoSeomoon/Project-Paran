using System.Collections.Generic;
using UnityEngine;
using TMPro;
//using UnityEditor.Embree;

public class MissionMenu : MonoBehaviour
{
    [Header("미션 메뉴 ui")]
    public GameObject[] missionPanels;
    void Awake()
    {
        MissionManager.OnActiveMissionsUpdated += UpdateMissionMenu;
    }

    void Start()
    {
        foreach(GameObject go in missionPanels)
        {
            go.SetActive(false);
        }
    }

    void OnDisable()
    {
        MissionManager.OnActiveMissionsUpdated -= UpdateMissionMenu;
    }

    private void UpdateMissionMenu(List<Mission> missions)
    {
        foreach(GameObject go in missionPanels)
        {
            go.SetActive(false);
        }

        for(int i=0 ; i<missions.Count ; i++)
        {
            GameObject missionDescription = missionPanels[i].transform.GetChild(1).gameObject;
            missionDescription.GetComponent<TextMeshProUGUI>().text = missions[i].missionDescription;
            missionPanels[i].SetActive(true);
        }
    }

    
}
