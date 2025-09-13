using UnityEngine;

public class InventoryUI : IUIPanel
{
    [SerializeField] private GameObject inventoryUI;
    public override void Show()
    {
        if(inventoryUI!=null)
            inventoryUI.SetActive(true);
    }

    public override void Hide()
    {
         if(inventoryUI!=null)
            inventoryUI.SetActive(false);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
