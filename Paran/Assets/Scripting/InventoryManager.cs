using UnityEngine;
using System;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private Transform slotContainer;   // Scroll View의 Content
    [SerializeField] private GameObject slotPrefab;     // 슬롯 프리팹

    [Header("상세 정보 UI (선택)")]
    [SerializeField] private ItemInfoUI itemInfoUI;     // 아이템 클릭 시 정보를 띄울 UI

    // [핵심] 이 오브젝트가 켜질 때마다(SetActive true) 자동으로 실행됨
    void OnEnable()
    {
        UpdateInventory();
    }

    // 인벤토리 목록 갱신
    private void UpdateInventory()
    {
        // 1. 기존 슬롯 삭제 (초기화)
        foreach (Transform child in slotContainer)
        {
            Destroy(child.gameObject);
        }

        // 2. 데이터 가져오기 (ItemManager가 없다면 예외처리)
        if (ItemManager.Instance == null) return;
        List<ItemData> myItems = ItemManager.Instance.inventoryList;
        Debug.Log("test1");

        // 3. 슬롯 생성
        foreach (ItemData item in myItems)
        {
            Debug.Log("test2");
            GameObject newSlot = Instantiate(slotPrefab, slotContainer);
            InventorySlot slotScript = newSlot.GetComponent<InventorySlot>();

            if (slotScript != null)
            {
                // 슬롯 설정 및 클릭 이벤트 연결
                slotScript.Setup(item, OnItemClicked);
            }
        }
    }

    // 아이템 슬롯 클릭 시 실행
    private void OnItemClicked(ItemData clickedItem)
    {
        // 상세 정보창에 정보 전달
        if (itemInfoUI != null)
        {
            // 만약 ItemInfoUI가 꺼져있다면 켜줌 (필요 시)
            itemInfoUI.gameObject.SetActive(true); 
            
            // 정보 갱신 (ItemInfoUI의 UpdateInfo 함수가 public이어야 함)
            itemInfoUI.UpdateInfo(clickedItem.itemIcon, clickedItem.itemName, clickedItem.itemDescription);
        }
    }
}
