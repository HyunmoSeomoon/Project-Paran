using UnityEngine;

public class QuitButton : MonoBehaviour
{
    public void OnQuitClicked()
    {
        Debug.Log("게임 종료 요청됨");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

