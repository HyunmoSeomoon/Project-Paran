using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUI : IUIPanel
{
    [SerializeField] private GameObject menuUI;
    [SerializeField] private GameObject[] menuPanelUI;
    void Start()
    {
        CloseAllPanel();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void Show()
    {
        if (menuUI != null)
            menuUI.SetActive(true);
        Time.timeScale = 0;
    }

    public override void Hide()
    {
        Time.timeScale = 1;
        CloseAllPanel();
        if (menuUI != null)
            menuUI.SetActive(false);
    }

    // 0 : setting
    // 1 : map
    // 2 : item
    // 3 : credit
    // 4 : restart
    public void changePanel(int i)
    {
        CloseAllPanel();
        if (i < 0 || i > 3) return;
        if (i == 4) return;
        menuPanelUI[i].SetActive(true);
    }
    
    public void CloseAllPanel()
    {
        foreach(GameObject p in menuPanelUI)
        {
            p.SetActive(false);
        }
    }
}
