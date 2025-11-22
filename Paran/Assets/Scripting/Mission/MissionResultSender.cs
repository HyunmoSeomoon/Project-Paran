using UnityEngine;
using System;

public class MissionResultSender : MonoBehaviour
{
    public MissionManager missionManager;
    public string missionName;
    [SerializeField] private EnemySearch enemySearch;
    [SerializeField] private ItemsManager itemsManager;

    void Start()
    {
        EnemySearch.OnEnemyDied += KillMissionComplete;
        ItemsManager.OnGetItem += GetMissionComplete;
    }

    private void KillMissionComplete(EnemySearch eventEnemySearch)
    {
        if (eventEnemySearch == enemySearch)
        {
            missionManager.CompleteMission(missionName);
        }
    }

    private void GetMissionComplete(ItemsManager eventItemsManager)
    {
        if (eventItemsManager == itemsManager)
        {
            missionManager.CompleteMission(missionName);
        }
    }
}
