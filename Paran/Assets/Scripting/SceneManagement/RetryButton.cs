using UnityEngine;
using UnityEngine.SceneManagement;

public class RetryButton : MonoBehaviour
{
    public void OnRetryClicked()
    {
        string sceneToLoad = GameController.Instance.lastSceneName;

        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.Log($"Retry {sceneToLoad}");
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("씬 저장 실패");
        }
    }
}
