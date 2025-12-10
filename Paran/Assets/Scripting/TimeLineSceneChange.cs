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

    public void ChangeGamePhase(int p)
    {
        if(-1<p && p < 4)
        {
            switch (p)
            {
                case 1 : gameController.gamePhase = GameController.GamePhase.Phase1;
                break;
                case 2 : gameController.gamePhase = GameController.GamePhase.Phase2;
                break;
                case 3 :  gameController.gamePhase = GameController.GamePhase.Phase3;
                break;
            }
        }
    }
}
