using UnityEngine;
using System;

public class MissionResultSender : MonoBehaviour
{
    public MissionManager missionManager;
    public string missionName;
    [SerializeField] private EnemySearch enemySearch;
    [SerializeField] private ItemData itemData;
    public bool missionCompleted = false;

    void Start()
    {
        EnemySearch.OnEnemyDied += KillMissionComplete;
        ItemController.OnGetItem += GetMissionComplete;
    }

    private void KillMissionComplete(EnemySearch eventEnemySearch)
    {
        if (eventEnemySearch == enemySearch)
        {
            missionCompleted = true;
            missionManager.CompleteMission(missionName);
        }
    }

    private void GetMissionComplete(ItemData eventItemsManager)
    {
        if (eventItemsManager == itemData)
        {
            missionCompleted = true;
            missionManager.CompleteMission(missionName);
        }
    }
}
