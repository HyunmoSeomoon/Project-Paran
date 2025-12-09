using System.Collections;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private UIManager uIManager;

    [Header("튜토리얼 UI")]
    [SerializeField] private GameObject[] UIs;
    [SerializeField] private GameObject[] buttons;

    private int currentStepIndex = 0; // UIsNum 이름을 좀 더 직관적으로 변경
    private int UIsLength;

    void Awake()
    {
        if (UIs != null)
            UIsLength = UIs.Length;

        // 시작 시 모든 튜토리얼 UI 끄기
        CloseAllTutorialUIs();
    }

    // [기존 기능] 처음부터 순서대로 시작
    public void StartTutorial()
    {
        ShowTutorialByIndex(0);
    }

    // [요청하신 기능] 특정 인덱스의 튜토리얼을 강제로 실행
    public void ShowTutorialByIndex(int index)
    {
        // 유효하지 않은 인덱스면 무시
        if (index < 0 || index >= UIsLength)
        {
            Debug.LogWarning($"튜토리얼 인덱스 {index}는 존재하지 않습니다.");
            return;
        }

        // 1. 기존에 켜져 있던 튜토리얼이 있다면 끄기
        CloseAllTutorialUIs();

        // 2. 인덱스 업데이트
        currentStepIndex = index;

        // 3. 매니저 활성화 및 시간 정지
        gameObject.SetActive(true);
        Time.timeScale = 0;

        // 4. 해당 UI 켜기
        UIs[currentStepIndex].SetActive(true);
    }

    // [기능 개선] 다음 튜토리얼로 넘어가는 기능
    public void NextTutorial()
    {
        Time.timeScale = 0;
        
        // 현재 UI 끄기
        if (currentStepIndex < UIsLength)
        {
            UIs[currentStepIndex].SetActive(false);
        }

        currentStepIndex++; // 다음 단계로

        // 다음 단계가 범위 내에 있다면 켜기
        if (currentStepIndex < UIsLength)
        {
            UIs[currentStepIndex].SetActive(true);
        }
        else
        {
            // 더 이상 튜토리얼이 없으면 종료
            EndTutorial();
        }
    }

    // [기능 추가] 튜토리얼 완전 종료
    public void EndTutorial()
    {
        CloseAllTutorialUIs();

        // 메뉴가 열려있지 않다면 시간 재개
        if (!uIManager.IsMenuOpened())
            Time.timeScale = 1;

        // 매니저 오브젝트를 파괴하지 않고 비활성화 (나중에 다시 쓰기 위해)
        gameObject.SetActive(false);
    }

    // 모든 튜토리얼 창을 닫는 헬퍼 함수
    private void CloseAllTutorialUIs()
    {
        foreach (GameObject go in UIs)
        {
            if(go != null) go.SetActive(false);
        }
    }

    // [기존 기능 유지] 즉시 종료 (Skip 버튼 등)
    public void Imm()
    {
        // 현재 켜진 창 끄기
        if (currentStepIndex < UIsLength)
        {
            UIs[currentStepIndex].SetActive(false);
        }
        
        // 버튼들 숨기기
        foreach (GameObject go in buttons)
        {
            go.SetActive(false);
        }

        EndTutorial();
    }

    public void Wait(int index)
    {
        gameObject.SetActive(true);
        StartCoroutine(TutorialCoroutine(index));
    }

    private IEnumerator TutorialCoroutine(int index)
    {
        GameObject immButton = null;
        // 현재 활성화된 버튼 찾기
        foreach (GameObject b in buttons)
        {
            if (b.activeSelf) 
            {
                immButton = b;
                b.SetActive(false);
            }
        }

        yield return new WaitForSecondsRealtime(3); // TimeScale이 0일 때도 작동하도록 Realtime 권장
        buttons[1].SetActive(true);

        ShowTutorialByIndex(index);
    }
}
