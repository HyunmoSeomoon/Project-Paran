using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ItemController : MonoBehaviour
{
    [Header("Item Data")]
    public ItemData itemData; // [변경] 개별 변수 대신 데이터 파일 연결

    [Header("Interaction Settings")]
    [SerializeField] private GameObject interactionUI;
    [SerializeField] private Transform mainCamera; // Camera 변수명은 대문자 지양 (C# 클래스와 혼동)
    [SerializeField] private PlayerMove playerMove;

    private bool canInteract = false;
    [SerializeField] private UIManager uiManager; // [최적화] 미리 참조 저장

    public static event Action<ItemData> OnGetItem; 

    void Start()
    {
        if (interactionUI != null)
            interactionUI.SetActive(false);
            
        // 카메라가 연결 안 되어있으면 자동으로 메인 카메라 찾기
        if (mainCamera == null && Camera.main != null)
            mainCamera = Camera.main.transform;
    }

    void Update()
    {
        if (canInteract)
        {
            // [최적화] UI가 항상 카메라를 보게 함 (빌보드) - Update에서 처리
            if (interactionUI != null && mainCamera != null)
            {
                interactionUI.transform.forward = mainCamera.forward;
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                PickUp();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (interactionUI != null) interactionUI.SetActive(true);
            canInteract = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canInteract = false;
            if (interactionUI != null) interactionUI.SetActive(false);
        }
    }

    public void PickUp()
    {
        // 1. 아이템 획득 이벤트 발송 (퀘스트 매니저 등이 수신)
        // 오브젝트가 꺼지기 전에 데이터를 넘깁니다.
        OnGetItem?.Invoke(itemData);

        // 2. 획득 정보 UI 띄우기
        if (uiManager != null)
        {
            uiManager.TurnOnUI(UIManager.UITypes.ItemInfo);
            
            if (uiManager.GetUIPanel(UIManager.UITypes.ItemInfo) is ItemInfoUI itemPanel)
            {
                // ItemData에 있는 정보를 넘겨줌
                itemPanel.UpdateInfo(itemData.itemIcon, itemData.itemName, itemData.itemDescription);
            }
        }

        // 3. 플레이어 이동 정지 (UI 보는 동안)
        if (playerMove != null)
        {
            playerMove.isMoved = false; 
            // 필요하다면 MoveEnable(false) 등 상태 변경 함수 호출
        }

        // 4. 오브젝트 비활성화 (아이템 먹음)
        // 상호작용 UI도 같이 꺼지도록 처리
        if (interactionUI != null) interactionUI.SetActive(false);
        gameObject.SetActive(false); 
    }
}
