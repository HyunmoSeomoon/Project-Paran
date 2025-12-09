using UnityEngine;

public class GameOverRetry : MonoBehaviour
{
    GameController gameController = null;
    
    void OnEnable()
    {
        gameController = null;
        gameController = FindAnyObjectByType<GameController>();

    }

    public void Retry()
    {
        if (gameController != null)
        {
            gameController.ChangeScene(gameController.previousPhase);
        }
    }
}
