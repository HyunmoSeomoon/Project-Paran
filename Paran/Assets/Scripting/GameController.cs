using System;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public enum GamePhase
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

    public GamePhase gamePhase;

    [SerializeField] private UIManager uIManager;
    [SerializeField] private CameraMove cameraMove;

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
    private DateTime startTime = new DateTime(1939,11,11,9,15,00);
    private DateTime currentTime;
    private int a = 0;
    void Start()
    {
        currentTime = startTime;
        uIManager.SetClockTime(currentTime.Hour, currentTime.Minute);
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (uIManager.GetUITypes() == UIManager.UITypes.Menu)
            {
                uIManager.TurnOffUI(UIManager.UITypes.Menu);
                cameraMove.isLocked = false;
                Time.timeScale = 1;
            }
            else
            {
                uIManager.TurnOnUI(UIManager.UITypes.Menu);
                cameraMove.isLocked = true;
                Time.timeScale = 0;
            }
        }
    }
    void FixedUpdate()
    {
        a++;
        if (a == 100)
        {
            currentTime = currentTime.AddMinutes(1);
            uIManager.NormalClockTime();
            a = 0;
        }
    }
}
