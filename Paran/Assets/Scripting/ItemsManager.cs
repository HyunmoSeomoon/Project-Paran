using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ItemsManager : MonoBehaviour
{
    public Sprite itemIcon;
    public string itemDescription;
    public string itemName;
    [SerializeField] private GameObject interactionUI;
    [SerializeField] private Transform Camera;
    [SerializeField] private PlayerMove playerMove;
    private bool canInteract = false;
    public static event Action<ItemsManager> OnGetItem;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (canInteract)
            if (Input.GetKeyDown(KeyCode.E))
            {
                PickUp();
            }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            interactionUI.SetActive(true);
            interactionUI.transform.forward = Camera.forward;
            canInteract = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canInteract = false;
            interactionUI.SetActive(false);
        }
    }

    public void PickUp()
    {
        UIManager uIManager = FindAnyObjectByType<UIManager>().GetComponent<UIManager>();
        uIManager.TurnOnUI(UIManager.UITypes.ItemInfo);
        if (uIManager.GetUIPanel(UIManager.UITypes.ItemInfo) is ItemInfoUI itemPanel)
        {
            itemPanel.UpdateInfo(itemIcon, itemName, itemDescription);
        }
        playerMove.isMoved = false;
        OnGetItem?.Invoke(this);
        gameObject.SetActive(false);
    }

    public void Use()
    {
        
    }
}
