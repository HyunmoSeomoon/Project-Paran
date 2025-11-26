using System;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject itemUI;

    //[Header("시계 ui")]
    //[SerializeField] private GameObject clockUI;
    [SerializeField] private GameObject minuteUI;
    [SerializeField] private GameObject hourUI;

    public EventHandler turnOnSpecificUI;

    public enum UITypes
    {
        Menu,
        Dialogue,
        ItemInfo,
        Mission,
        Idle
    }

    private UITypes currentUIType = UITypes.Idle;

    // 각 UI 타입에 맞는 패널들을 등록할 딕셔너리
    private Dictionary<UITypes, IUIPanel> uiPanels = new Dictionary<UITypes, IUIPanel>();

    // Unity 에디터에서 UI들을 등록하기 위한 리스트
    [SerializeField] private List<UIPanelPair> uiPanelPairs;

    [System.Serializable]
    public class UIPanelPair
    {
        public UITypes uiType;
        public IUIPanel panelScript; // InventoryUI, SettingsUI 등 IUIPanel을 구현한 스크립트를 넣을 곳
    }

    void Awake()
    {
        foreach (var pair in uiPanelPairs)
        {
            if (pair.panelScript is IUIPanel panel)
            {
                uiPanels[pair.uiType] = panel;
            }
            else Debug.Log("error");
        }
        HideAll();
        Debug.Log(uiPanels.Count);
    }

    void Start()
    {

    }

    public void TurnOnUI(UITypes uITypes)
    {
        if (uiPanels.TryGetValue(uITypes, out IUIPanel uiPanel))
        {
            HideAll();
            uiPanel.Show();
            currentUIType = uITypes;
        }
        else Debug.Log("uiTypes error");
    }

    public void TurnOffUI(UITypes uITypes)
    {
        if (uiPanels.TryGetValue(uITypes, out IUIPanel uiPanel))
        {
            uiPanel.Hide();
            currentUIType = UITypes.Idle;
        }
    }

    public IUIPanel GetUIPanel(UITypes uITypes)
    {
        if (uiPanels.TryGetValue(uITypes, out IUIPanel uiPanel))
        {
            return uiPanel;
        }
        return null;
    }

    private void HideAll()
    {
        foreach (var panel in uiPanels)
        {
            panel.Value.Hide();
        }
    }

    public UITypes GetUITypes()
    {
        return currentUIType;
    }

    /*
    public void SetClockTime(int hour, int minute)
    {
        if (minuteUI == null || hourUI == null)
            return;

        minuteUI.transform.rotation = Quaternion.Euler(0, 0, minute * -6);
        hourUI.transform.rotation = Quaternion.Euler(0, 0, ( (hour * -30) - (minute*0.5f) ));
    }

    Vector3 normalMinute = new Vector3(0, 0, -6);
    Vector3 normalHour = new Vector3(0, 0, -0.5f);
    public void NormalClockTime()
    {
        minuteUI.transform.Rotate(normalMinute);
        hourUI.transform.Rotate(normalHour);
    }
    */
}

public abstract class IUIPanel : MonoBehaviour
{
    public abstract void Show();
    public abstract void Hide();
}
