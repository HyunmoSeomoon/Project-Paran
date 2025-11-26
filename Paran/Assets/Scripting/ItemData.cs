using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Item Data")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite itemIcon;
    [TextArea(3, 5)]
    public string itemDescription;
    public int itemID; // 나중에 퀘스트나 인벤토리 시스템에서 구분용
}
