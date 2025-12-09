using UnityEngine;

public class TimeLineSceneChange : MonoBehaviour
{
    GameController gameController = null;
    
    void OnEnable()
    {
        gameController = null;
        gameController = FindAnyObjectByType<GameController>();
    }

    public void ChangeSceneTimeLine(string sceneName)
    {
        gameController.ChangeScene(sceneName);
    }
}
