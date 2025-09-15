using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ItemsManager : InteractableObject
{
    public Sprite itemIcon;
    public string itemDescription;
    public string itemName;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void PickUp(Transform attachPoint)
    {
        UIManager uIManager = FindAnyObjectByType<UIManager>().GetComponent<UIManager>();
        uIManager.TurnOnUI(UIManager.UITypes.ItemInfo);
        if (uIManager.GetUIPanel(UIManager.UITypes.ItemInfo) is ItemInfoUI itemPanel)
        {
            itemPanel.UpdateInfo(itemIcon, itemName, itemDescription);
        }
        attachPoint.parent.gameObject.GetComponent<PlayerMove>().isMoved = false;
        gameObject.SetActive(false);
    }

    public override void Drop()
    {
        
    }
}
