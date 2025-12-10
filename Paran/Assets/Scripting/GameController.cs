using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public enum GamePhase
    {
        Prologue, //프롤로그 컷씬
        Phase1, //페이즈1, 튜토리얼
        Phase2, //케이코 만남 및 1층
        Phase3, //2층 암살 페이즈
        Phase4, //결말 컷씬
        Phase5, //
        Retry, // PlayerKilled, 재도전 씬
        Ending //
    }

    public GamePhase gamePhase;

    public GamePhase previousPhase;

    private UIManager uIManager;
    [SerializeField] private CameraMove cameraMove = null;
    private MissionManager missionManager;
    private SceneSettingController sceneSettingController;

    // 무조건 0번은 타이틀, 1번은 게임오버 씬. 이후로는 최초 플레이 순서대로 씬 입력
    [Header("씬 이름 플레이 순서대로 입력")]
    [SerializeField] private string[] sceneNames;

    [Header("튜토리얼 페이즈(Phase 1)")]
    [SerializeField] private TutorialManager tutorialManager;
    public bool isGameOverRoutineRunning = false;

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

    void OnEnable()
    {
        SceneManager.sceneLoaded += checkcurrentPhase;
    }

    public bool retry = false;
    private DateTime startTime = new DateTime(1939, 11, 11, 9, 15, 00);
    private DateTime currentTime;
    //private int a = 0;
    void Start()
    {
        currentTime = startTime;
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

        if (gamePhase == GamePhase.Retry && !isGameOverRoutineRunning)
        {   
            StartCoroutine(GameOver());
        }
    }

    void checkcurrentPhase(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1f; 

        cameraMove = FindAnyObjectByType<CameraMove>(); 
        uIManager = FindAnyObjectByType<UIManager>();  
        sceneSettingController = FindAnyObjectByType<SceneSettingController>().GetComponent<SceneSettingController>();
        if (gamePhase == GamePhase.Phase1 && scene.name == "Floor2") 
        {  
            Debug.Log("start Phase1"); 
            isGameOverRoutineRunning = false;
        }
        if(sceneSettingController!=null)
            sceneSettingController.SetScenefromPhase(gamePhase);
    }
    
    private IEnumerator GameOver()
    {
        isGameOverRoutineRunning = true;
        Debug.Log("잡힘 - 5초 후 SceneChange");

        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene("GameOverScene");
    }
    
    public void ChangeScene(string NextScene)
    {
        if(NextScene==sceneNames[3]){gamePhase = GamePhase.Phase2;}
        SceneManager.LoadScene(NextScene);
    }

    public void ChangeScene(GamePhase phase)
    {
        gamePhase = phase;
        switch (phase)
        {
            case GamePhase.Phase1: ChangeScene("Floor2");
            break;
            case GamePhase.Phase2: ChangeScene("Floor1");
            break;
        }
    }
}
