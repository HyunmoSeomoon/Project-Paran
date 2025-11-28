using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemInfoUI : IUIPanel
{
    [SerializeField] private GameObject itemInfoUI;
    [SerializeField] private UIManager uIManager;
    [Header("UI 요소")]
    [SerializeField] private Image itemIconImage;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    public override void Show()
    {
        if (itemInfoUI != null){
            itemInfoUI.SetActive(true);
            Time.timeScale = 0;
        }
    }

    public override void Hide()
    {
        if(!uIManager.IsMenuOpened())
            Time.timeScale = 1;
        itemInfoUI.SetActive(false);
    }

    public void UpdateInfo(Sprite icon, string name, string description)
    {
        if (itemIconImage != null) itemIconImage.sprite = icon;
        if (itemNameText != null) itemNameText.text = name;
        if (itemDescriptionText != null) itemDescriptionText.text = description;
    }
}
