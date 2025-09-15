using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

public class ItemInfoUI : IUIPanel
{
    [SerializeField] private GameObject itemInfoUI;
    private Image image;
    public override void Show()
    {
        if (itemInfoUI != null)
            itemInfoUI.SetActive(true);
    }

    public override void Hide()
    {
        itemInfoUI.SetActive(false);
    }
}
