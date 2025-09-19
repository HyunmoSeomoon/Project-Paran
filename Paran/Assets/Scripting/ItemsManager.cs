using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ItemsManager : InteractableObject
{
    [SerializeField] private Image itemImage;

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
        UIManager uIManager = attachPoint.gameObject.GetComponent<UIManager>();
        uIManager.TurnOnUI(UIManager.UITypes.ItemInfo);
    }

    public override void Drop()
    {
        
    }
}
