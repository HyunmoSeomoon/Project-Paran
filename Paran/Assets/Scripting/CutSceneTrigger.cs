using UnityEngine;
using UnityEngine.Playables;

public class CutSceneTrigger : MonoBehaviour
{
    public PlayableDirector timelineA;
    public PlayableDirector timelineB;
    //public GameObject playerCamera;

    public bool sceneflag = false;
    public bool playflag = false;

    GameController gameController = null;
    
    void OnEnable()
    {
        gameController = null;
        gameController = FindAnyObjectByType<GameController>();
    }

    void Start()
    {
        timelineA.stopped += OnCutsceneFinished;
        timelineB.stopped += OnCutsceneFinished;
        if (timelineA != null && timelineB != null)
        {
            if (gameController.gamePhase == GameController.GamePhase.Phase3) PlayTimelineB();
            else PlayTimelineA();
        }
    }

    public void StartCutscene()
    {
        // 플래그로 어떤 컷씬 나올지 조정
        Debug.Log("실행실행");
        if (sceneflag)
            PlayTimelineA();
        else
            PlayTimelineB();
    }

    public void PlayTimelineA()
    {
        //if (timelineA == null) return;
        //playerCamera.SetActive(false);
        if(timelineB!=null) timelineB.Stop();
        timelineA.time = 0;
        timelineA.Play();
    }

    public void PlayTimelineB()
    {
        //if (timelineB == null) return;
        //playerCamera.SetActive(false);
        if(timelineA!=null) timelineA.Stop();
        timelineB.time = 0;
        timelineB.Play();
    }

    private void OnCutsceneFinished(PlayableDirector obj)
    {
        playflag = false;
    }
}