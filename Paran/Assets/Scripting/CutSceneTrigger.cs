using UnityEngine;
using UnityEngine.Playables;

public class CutScneTrigger : MonoBehaviour
{
    public PlayableDirector timeline;
    public GameObject cinemachineVCam;  
    public Camera mainCam;
    //private bool isPlayerInside = false;
    private bool cutscenePlaying = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCutscene();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartCutscene()
    {
        cutscenePlaying = true;

        cinemachineVCam.SetActive(true);
        
        timeline.Play();

        timeline.stopped += OnCutsceneFinished;
    }

    private void OnCutsceneFinished(PlayableDirector obj)
    {
        cinemachineVCam.SetActive(false);
        cutscenePlaying = false;
        
    }
}
