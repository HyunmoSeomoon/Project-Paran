using UnityEngine;
using UnityEngine.Playables;

public class CutSceneTrigger : MonoBehaviour
{
    public PlayableDirector timelineA;
    public PlayableDirector timelineB;

    public bool sceneflag = false;

    void Start()
    {
        timelineA.stopped += OnCutsceneFinished;
        timelineB.stopped += OnCutsceneFinished;

        StartCutscene();
    }

    public void StartCutscene()
    {
        // 플래그로 어떤 컷씬 나올지 조정
        if (sceneflag)
            PlayTimelineA();
        else
            PlayTimelineB();
    }

    public void PlayTimelineA()
    {
        timelineB.Stop();
        timelineA.time = 0;
        timelineA.Play();
    }

    public void PlayTimelineB()
    {
        timelineA.Stop();
        timelineB.time = 0;
        timelineB.Play();
    }

    private void OnCutsceneFinished(PlayableDirector obj)
    {
        // 끝이긴 한디
    }
}