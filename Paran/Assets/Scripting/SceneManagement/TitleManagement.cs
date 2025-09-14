using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleInput : MonoBehaviour
{
    public FadeController fader;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            fader.FadeOut();
    }
}