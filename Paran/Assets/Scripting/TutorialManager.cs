using System.Collections;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private UIManager uIManager;

    [Header("튜토리얼 UI")]
    [SerializeField] private GameObject[] UIs;
    [SerializeField] private GameObject[] buttons;
    private int UIsNum = 0;
    private int UIsLength;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if(UIs!=null)
            UIsLength = UIs.Length;
        foreach(GameObject go in UIs)
        {
            go.SetActive(false);
        }
    }

    public void StartTutorial()
    {
        Time.timeScale = 0;
        UIs[0].SetActive(true);
    }

    public void NextTutorial()
    {
        Time.timeScale = 0;
        if(UIsNum < UIsLength-1)
        {
            UIs[UIsNum].SetActive(false);
            UIsNum++;
            UIs[UIsNum].SetActive(true);
        }
        else if(UIsNum == UIsLength)
        {
            if(!uIManager.IsMenuOpened())
                Time.timeScale = 1;
            Destroy(gameObject);
        }
    }

    public void Imm()
    {
        Debug.Log(uIManager.IsMenuOpened());
        if(!uIManager.IsMenuOpened())
            Time.timeScale = 1;
        UIs[UIsNum].SetActive(false);
        foreach(GameObject go in buttons)
        {
            go.SetActive(false);
        }

    }

    public void Wait(int t)
    {
        StartCoroutine(TutorialCoroutine(t));
    }

    private IEnumerator TutorialCoroutine(int t)
    {
        yield return new WaitForSeconds(t);
        NextTutorial();
        yield break;
    }
}
