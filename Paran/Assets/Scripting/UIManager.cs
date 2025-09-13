using System;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject itemUI;
    [SerializeField] private GameObject clock;
    public EventHandler turnOnSpecificUI;
    public enum UITypes
    {
        Menu,
        Inventory,
        ItemInfo,
        Map
    }

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

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.I))
        {
            TurnOnUI(UITypes.Inventory);
        }
    }

    public void TurnOnUI(UITypes uITypes)
    {
        if (uiPanels.TryGetValue(uITypes, out IUIPanel uiPanel))
        {
            HideAll();
            uiPanel.Show();
        }
        else Debug.Log("uiTypes error");
    }

    private void HideAll()
    {
        foreach (var panel in uiPanels)
        {
            panel.Value.Hide();
        }
    }
}

public abstract class IUIPanel : MonoBehaviour
{
    public abstract void Show();
    public abstract void Hide();
}
