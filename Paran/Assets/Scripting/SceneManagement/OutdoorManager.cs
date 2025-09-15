using UnityEngine;
using UnityEngine.SceneManagement;

public class OutdoorManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void GotoMain()
    {
        SceneManager.LoadScene("TitleScene");
    }
    public void ResetScene()
    {
        SceneManager.LoadScene("OutdoorsScene");
    }
}
