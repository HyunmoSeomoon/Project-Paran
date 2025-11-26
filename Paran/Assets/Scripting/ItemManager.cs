using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
// [수정] 컨트롤러(오브젝트) 대신 데이터(파일)를 저장합니다.
    public List<ItemData> inventoryList = new List<ItemData>(); 
    
    public static ItemManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    void OnEnable()
    {
        // 이벤트 구독
        ItemController.OnGetItem += AddItem;
    }

    void OnDisable()
    {
        // [중요] 이벤트 구독 해제 (습관화하는 것이 좋습니다)
        ItemController.OnGetItem -= AddItem;
    }

    // [수정] 매개변수로 ItemData를 받습니다.
    private void AddItem(ItemData newItemData)
    {
        if (newItemData != null)
        {
            inventoryList.Add(newItemData);
            Debug.Log($"인벤토리에 추가됨: {newItemData.itemName}");
        }
    }
}
