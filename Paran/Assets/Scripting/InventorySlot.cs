using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Button button;

    private ItemData _itemData;
    
    // InventoryUI가 이 함수를 호출해서 데이터를 세팅함
    public void Setup(ItemData data, System.Action<ItemData> onClickCallback)
    {
        _itemData = data;

        // 1. 아이콘 설정
        if (data.itemIcon != null)
        {
            iconImage.sprite = data.itemIcon;
            iconImage.enabled = true;
        }
        else
        {
            iconImage.enabled = false;
        }

        // 2. 버튼 클릭 시 실행할 행동 연결 (상세 정보 띄우기 등)
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClickCallback(data));
    }
}
