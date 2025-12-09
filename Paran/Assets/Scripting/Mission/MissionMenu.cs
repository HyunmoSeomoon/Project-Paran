using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MissionMenu : MonoBehaviour
{
    [Header("미션 메뉴 ui")]
    public GameObject[] missionPanels;
    void Awake()
    {
        MissionManager.OnActiveMissionsUpdated += UpdateMissionMenu;
    }

    void OnDisable()
    {
        MissionManager.OnActiveMissionsUpdated -= UpdateMissionMenu;
    }

    private void UpdateMissionMenu(List<Mission> missions)
    {
        for(int i=0 ; i<missions.Count ; i++)
        {

        }
    }

    
}
