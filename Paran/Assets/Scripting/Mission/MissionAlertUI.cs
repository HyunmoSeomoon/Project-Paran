using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic; // List 사용
using TMPro;
using System.Collections;
//using GLTF.Schema;

public class MissionAlertUI : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private GameObject completeAlertPanel; // 팝업 패널 (배경 이미지 등)
    [SerializeField] private TextMeshProUGUI completeAlertTitleText; // "새로운 미션" / "미션 완료"
    [SerializeField] private TextMeshProUGUI completeAlertMissionNameText; // 미션 이름
    [SerializeField] private GameObject startAlertPanel; // 팝업 패널 (배경 이미지 등)
    [SerializeField] private TextMeshProUGUI startAlertTitleText; // "새로운 미션" / "미션 완료"
    [SerializeField] private TextMeshProUGUI startAlertMissionNameText; // 미션 이름

    [Header("설정")]
    [SerializeField] private float displayDuration = 3.0f; // 표시 시간 (3초)

    private Coroutine currentAlertCoroutine;

    // (IUIPanel이 아닐 수 있으므로 Start/OnDestroy에서 구독)
    void Awake()
    {
        // '알림'용 이벤트만 구독합니다.
        MissionManager.OnMissionStarted += ShowMissionStartedAlert;
        MissionManager.OnMissionCompleted += ShowMissionCompletedAlert;
        
        completeAlertPanel.SetActive(false); // 처음엔 숨김
        startAlertPanel.SetActive(false);
    }

    void OnDestroy()
    {
        MissionManager.OnMissionStarted -= ShowMissionStartedAlert;
        MissionManager.OnMissionCompleted -= ShowMissionCompletedAlert;
    }

    // 새 미션 알림이 도착했을 때
    private void ShowMissionStartedAlert(Mission newMission)
    {
        // 텍스트 설정
        startAlertTitleText.text = "새로운 임무";
        startAlertMissionNameText.text = newMission.missionDescription;
        
        // 팝업 코루틴 시작
        StartAlertPopup(false);
    }

    // 미션 완료 알림이 도착했을 때
    private void ShowMissionCompletedAlert(Mission completedMission)
    {
        // 텍스트 설정
        completeAlertTitleText.text = "임무 완료";
        completeAlertMissionNameText.text = completedMission.missionDescription;
        
        // 팝업 코루틴 시작
        StartAlertPopup(true);
    }

    private void StartAlertPopup(bool isComplete)
    {
        currentAlertCoroutine = StartCoroutine(PopupRoutine(isComplete));
    }

    // [요청] 텍스트가 이미 설정된 패널을 띄웠다가 숨기는 코루틴
    private IEnumerator PopupRoutine(bool isComplete)
    {
        CanvasRenderer canvasRenderer1;
        CanvasRenderer canvasRenderer2;
        CanvasRenderer canvasRenderer3;
        if (isComplete)
        {
            canvasRenderer1 = completeAlertPanel.GetComponent<CanvasRenderer>();
            canvasRenderer2 = completeAlertTitleText.GetComponent<CanvasRenderer>();
            canvasRenderer3 = completeAlertMissionNameText.GetComponent<CanvasRenderer>();
        }
        else
        {
            canvasRenderer1 = startAlertPanel.GetComponent<CanvasRenderer>();
            canvasRenderer2 = startAlertTitleText.GetComponent<CanvasRenderer>();
            canvasRenderer3 = startAlertMissionNameText.GetComponent<CanvasRenderer>();
        }

        // 1. UI를 띄움
        canvasRenderer1.SetAlpha(1f);
        canvasRenderer2.SetAlpha(1f);
        canvasRenderer3.SetAlpha(1f);
        if(isComplete) completeAlertPanel.SetActive(true);
        else startAlertPanel.SetActive(true);

        // 2. 일정 시간 대기
        yield return new WaitForSeconds(displayDuration);

        // 3. UI를 숨김
        float elapsedTime = 0f;
        while (canvasRenderer1.GetAlpha() > 0.01f)
        {
            canvasRenderer1.SetAlpha(Mathf.Lerp(1f, 0f, elapsedTime / 1f));
            canvasRenderer2.SetAlpha(Mathf.Lerp(1f, 0f, elapsedTime / 1f));
            canvasRenderer3.SetAlpha(Mathf.Lerp(1f, 0f, elapsedTime / 1f));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        if(isComplete) completeAlertPanel.SetActive(false);
        else startAlertPanel.SetActive(false);
        currentAlertCoroutine = null;
    }
}
