using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic; // List 사용
using TMPro;
using System.Collections;
//using GLTF.Schema;

public class MissionAlertUI : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private GameObject alertPanel; // 팝업 패널 (배경 이미지 등)
    [SerializeField] private TextMeshProUGUI alertTitleText; // "새로운 미션" / "미션 완료"
    [SerializeField] private TextMeshProUGUI alertMissionNameText; // 미션 이름

    [Header("설정")]
    [SerializeField] private float displayDuration = 3.0f; // 표시 시간 (3초)

    private Coroutine currentAlertCoroutine;

    // (IUIPanel이 아닐 수 있으므로 Start/OnDestroy에서 구독)
    void Awake()
    {
        // '알림'용 이벤트만 구독합니다.
        MissionManager.OnMissionStarted += ShowMissionStartedAlert;
        MissionManager.OnMissionCompleted += ShowMissionCompletedAlert;
        
        alertPanel.SetActive(false); // 처음엔 숨김
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
        alertTitleText.text = "새로운 미션";
        alertMissionNameText.text = newMission.missionDescription;
        
        // 팝업 코루틴 시작
        StartAlertPopup();
    }

    // 미션 완료 알림이 도착했을 때
    private void ShowMissionCompletedAlert(Mission completedMission)
    {
        // 텍스트 설정
        alertTitleText.text = "미션 완료";
        alertMissionNameText.text = completedMission.missionDescription;
        
        // 팝업 코루틴 시작
        StartAlertPopup();
    }

    private void StartAlertPopup()
    {
        // 이미 팝업이 떠있으면 중지하고 다시 시작 (새 알림이 덮어쓰기)
        if (currentAlertCoroutine != null)
        {
            StopCoroutine(currentAlertCoroutine);
        }
        currentAlertCoroutine = StartCoroutine(PopupRoutine());
    }

    // [요청] 텍스트가 이미 설정된 패널을 띄웠다가 숨기는 코루틴
    private IEnumerator PopupRoutine()
    {
        CanvasRenderer canvasRenderer1 = alertPanel.GetComponent<CanvasRenderer>();
        CanvasRenderer canvasRenderer2 = alertTitleText.GetComponent<CanvasRenderer>();
        CanvasRenderer canvasRenderer3 = alertMissionNameText.GetComponent<CanvasRenderer>();

        // 1. UI를 띄움
        canvasRenderer1.SetAlpha(1f);
        canvasRenderer2.SetAlpha(1f);
        canvasRenderer3.SetAlpha(1f);
        alertPanel.SetActive(true);

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
        alertPanel.SetActive(false);
        currentAlertCoroutine = null;
    }
}
