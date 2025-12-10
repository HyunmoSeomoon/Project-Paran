using Unity.VisualScripting;
using UnityEngine;

public class SceneSettingController : MonoBehaviour
{
    //모든 배열은 0번이 prologue, 1번이 phase1, 2번이 phase2
    public GameObject[] enemies;
    public CutSceneTrigger cutSceneTrigger;
    public GameObject[] npcs;
    public GameObject[] missionPoints;
    public GameObject[] items;
    public Transform[] startPosition;

    public void SetScenefromPhase(GameController.GamePhase gamePhase)
    {
        switch (gamePhase)
        {
            case GameController.GamePhase.Prologue:
                if(cutSceneTrigger!=null)
                    cutSceneTrigger.sceneflag = true;
            break;
            case GameController.GamePhase.Phase1:
                SetSceneObjects(1);
            break;
            case GameController.GamePhase.Phase2:
                SetSceneObjects(2);
            break;
            case GameController.GamePhase.Phase3:
                SetSceneObjects(3);
                if(cutSceneTrigger!=null)
                    cutSceneTrigger.sceneflag = false;
            break;
            default:
                Debug.Log("Cant detect phase");
            break;
        }
    }

    private void SetSceneObjects(int p)
    {
        for(int i = 0 ; i<4 ; i++)
        {
            if(enemies[i]!=null)
                enemies[i].SetActive(false);
            if(npcs[i]!=null)
                npcs[i].SetActive(false);
            if(missionPoints[i]!=null)
                missionPoints[i].SetActive(false);
            if(items[i]!=null)
                items[i].SetActive(false);
        }
        if(enemies[p]!=null)
            enemies[p].SetActive(true);
        if(npcs[p]!=null)
            npcs[p].SetActive(true);
        if(missionPoints[p]!=null)
            missionPoints[p].SetActive(true);
        if(items[p]!=null)
            items[p].SetActive(true);
        if (startPosition[p] != null)
        {
            Transform player = FindAnyObjectByType<PlayerMove>().gameObject.transform;
            player.position = startPosition[p].position;
            player.rotation = startPosition[p].rotation;
        }
            
    }

}
