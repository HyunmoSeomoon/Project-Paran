using System;
using UnityEngine;

public class GameController : MonoBehaviour
{
    enum GamePhase
    {
        Prologue, //프롤로그 컷씬
        Phase1, //페이즈1. 단서 및 도구 수집
        Phase2, //과거 회상 컷씬
        Phase3, //암살 페이즈
        Phase4, //결말 컷씬
        Phase5, //
        Retry, // PlayerKilled, 재도전 씬
        Ending //
    }

    //For SingleTon pattern
    public static GameController Instance { get; private set; }
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
        }
        else Destroy(gameObject);
    }

    public bool timeFlag = true;
    private DateTime startTime = new DateTime(1939,11,11,10,00,00);
    private DateTime currentTime;
    private int a = 0;
    void Start() {
        currentTime = startTime;
    }
    void FixedUpdate()
    {
        a++;
        if (a == 50)
        {
            currentTime = currentTime.AddSeconds(1);
            Debug.Log(currentTime.ToString("MM/dd/yyyy HH:mm:ss"));
            a = 0;
        }
    }
}
