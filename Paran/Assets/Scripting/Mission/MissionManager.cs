using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
//using GLTF.Schema;

public class MissionManager : MonoBehaviour
{
    public MissionList[] missionLists; // 인스펙터에서 모든 미션 리스트를 설정
    public static MissionManager Instance { get; private set; }

    // 1. 단일 객체 -> 리스트로 변경
    private List<MissionList> activeMissionLists = new List<MissionList>();

    // 2. 이벤트가 Mission 하나가 아닌, 'Mission 리스트'를 전달
    // 1. (신규) 새 미션 '알림/팝업'용
    public static event Action<Mission> OnMissionStarted;
    // 2. (신규) 미션 '완료 알림/팝업'용
    public static event Action<Mission> OnMissionCompleted;
    // 3. (기존) '임무 기록(Log)' 메뉴 UI 갱신용
    public static event Action<List<Mission>> OnActiveMissionsUpdated;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }
    
    public void StartMissionList(string listName)
    {
        if (activeMissionLists.Any(list => list.missionListName == listName)) return;

        MissionList listToStart = missionLists.FirstOrDefault(list => list.missionListName == listName);

        if (listToStart != null)
        {
            listToStart.Init();
            activeMissionLists.Add(listToStart);
            
            Mission firstMission = listToStart.GetCurrentMission();

            // [발송 1] 새 미션 팝업(Alert) 이벤트 발송
            OnMissionStarted?.Invoke(firstMission);
            SetMissionPoint(true,firstMission);
            
            // [발송 2] 임무 기록(Log) 갱신 이벤트 발송
            TriggerFullListUpdate();
        }
    }

    public void CompleteMission(string missionName)
    {
        MissionList targetList = null;
        Mission completedMission = null;

        foreach (var list in activeMissionLists)
        {
            Mission mission = list.GetCurrentMission();
            if (mission != null && !mission.isEnded && mission.missionName == missionName)
            {
                targetList = list;
                completedMission = mission;
                break;
            }
        }

        if (targetList != null)
        {
            // 1. 현재 미션 완료 처리 (isEnded = true)
            targetList.CompleteCurrentMission();
            SetMissionPoint(false,targetList.GetCurrentMission());
            
            // [발송 3] 미션 완료 팝업(Alert) 이벤트 발송
            OnMissionCompleted?.Invoke(completedMission);
            
            // 2. 다음 미션으로 인덱스 이동
            Mission nextMission = targetList.GoToNextMission();
            
            // 3. (중요) 다음 미션이 있다면, '새 미션' 팝업 이벤트 또 발송
            if (nextMission != null)
            {
                OnMissionStarted?.Invoke(nextMission);
                SetMissionPoint(true,nextMission);
            }
            
            // 4. [발송 4] 임무 기록(Log) 갱신 (완료 표시를 위해)
            TriggerFullListUpdate();
        }
    }


    private void TriggerFullListUpdate()
    {
        // "현재 UI에 표시해야 할 모든 활성 미션" 목록을 만듭니다.
        List<Mission> allCurrentActiveMissions = new List<Mission>();
        
        foreach (var list in activeMissionLists)
        {
            // 각 리스트에서 "현재 진행 중인" 미션만 가져옵니다.
            Mission currentMission = list.GetCurrentMission();
            
            // (참고: GetCurrentMission()은 리스트가 완료되면 null을 반환합니다)
            if (currentMission != null)
            {
                allCurrentActiveMissions.Add(currentMission);
            }
        }

        // 이 "활성 미션" 목록을 UI로 보냅니다.
        OnActiveMissionsUpdated?.Invoke(allCurrentActiveMissions);
    }

    private void SetMissionPoint(bool b, Mission mission)
    {
        mission.missionPoint.SetActive(b);
    }
}

[System.Serializable]
public class Mission
{
    public string missionName;
    public string missionDescription;
    public bool isEnded;
    public UnityEvent OnMissionStart;
    public UnityEvent OnMissionEnd;
    public GameObject missionPoint;
}

[System.Serializable]
public class MissionList
{
    public Mission[] missionList;
    public string missionListName;
    private int currentMissionIndex;
    public bool isStarted = false;
    public bool isCompleted = false; // 리스트 완료 여부

    public void Init()
    {
        Debug.Log("Start mission :" + missionListName);
        currentMissionIndex = 0;
        isStarted = true;
        isCompleted = false;
    }

    public Mission GetCurrentMission()
    {
        if (isCompleted || !isStarted) return null;
        return missionList[currentMissionIndex];
    }

    // 다음 미션으로 이동하고, 새 미션을 반환합니다.
    public Mission GoToNextMission()
    {
        // 1. 현재 미션이 완료되었는지 확인 (이건 외부에서 CompleteCurrentMission으로 설정)
        if (!missionList[currentMissionIndex].isEnded)
        {
            Debug.LogWarning("현재 미션이 끝나지 않아 다음으로 넘어갈 수 없습니다.");
            return missionList[currentMissionIndex]; // 현재 미션 반환
        }

        // 2. 다음 미션이 배열에 있는지 확인
        if (currentMissionIndex + 1 < missionList.Length)
        {
            currentMissionIndex++;
            return missionList[currentMissionIndex]; // 새 미션 반환
        }
        else
        {
            // 3. 리스트의 모든 미션을 완료함
            isCompleted = true;
            return null; // 완료했으므로 null 반환
        }
    }

    public void CompleteCurrentMission()
    {
        if (isCompleted) return;
        missionList[currentMissionIndex].isEnded = true;
    }
}
