using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FadeController : MonoBehaviour // Panel 불투명도 조절해 페이드인 or 페이드아웃
{
    public bool isFadeIn;
    public bool isFadeOut;
    public string nextScene;
    public GameObject panel; // 불투명도를 조절할 Panel 오브젝트
    private Action onCompleteCallback; // FadeIn 또는 FadeOut 다음에 진행할 함수

    void Start()
    {
        if (!panel)
        {
            Debug.LogError("Panel 오브젝트를 찾을 수 없습니다.");
            throw new MissingComponentException();
        }

        if (isFadeIn) // Fade In Mode -> 바로 코루틴 시작
        {
            panel.SetActive(true); // Panel 활성화
            StartCoroutine(CoFadeIn());
        }
        else if (isFadeOut)
        {
            panel.SetActive(false); // Panel 비활성화
        }
    }

    public void FadeOut()
    {
        panel.SetActive(true); // Panel 활성화
        Debug.Log("FadeCanvasController_ Fade Out 시작");
        StartCoroutine(CoFadeOut());
        Debug.Log("FadeCanvasController_ Fade Out 끝");
    }

    IEnumerator CoFadeIn()
    {
        float elapsedTime = 0f; // 누적 경과 시간
        float fadedTime = 2f; // 총 소요 시간

        while (elapsedTime <= fadedTime)
        {
            panel.GetComponent<CanvasRenderer>().SetAlpha(Mathf.Lerp(1f, 0f, elapsedTime / fadedTime));
            
            elapsedTime += Time.deltaTime;
            Debug.Log("Fade In 중...");
            yield return null;
        }
        Debug.Log("Fade In 끝");
        panel.SetActive(false); // Panel을 비활성화
        onCompleteCallback?.Invoke(); // 이후에 해야 하는 다른 액션이 있는 경우(null이 아님) 진행한다
        yield break;
    }

    IEnumerator CoFadeOut()
    {
        float elapsedTime = 0f; // 누적 경과 시간
        float fadedTime = 2f; // 총 소요 시간

        while (elapsedTime <= fadedTime)
        {
            panel.GetComponent<CanvasRenderer>().SetAlpha(Mathf.Lerp(0f, 1f, elapsedTime / fadedTime));

            elapsedTime += Time.deltaTime;
            Debug.Log("Fade Out 중...");
            yield return null;
        }

        Debug.Log("Fade Out 끝");
        onCompleteCallback?.Invoke(); // 이후에 해야 하는 다른 액션이 있는 경우(null이 아님) 진행한다
        SceneManager.LoadScene(nextScene);
        yield break;
    }

    public void RegisterCallback(Action callback) // 다른 스크립트에서 콜백 액션 등록하기 위해 사용
    {
        onCompleteCallback = callback;
    }
}