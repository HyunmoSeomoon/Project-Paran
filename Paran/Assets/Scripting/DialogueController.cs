using System;
using System.Collections;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class DialogueController : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private string[] dialogues;
    [SerializeField] private GameObject interactionUI;
    [SerializeField] private UIManager uIManager;
    [SerializeField] private Transform Camera;
    [SerializeField] private CinemachineCamera cinemachineCamera;
    public bool finished = false;
    private CameraMove cameraMove;

    public float dialogueDistance =1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        interactionUI.SetActive(false);
        cinemachineCamera.gameObject.SetActive(false);
        cameraMove = Camera.gameObject.GetComponent<CameraMove>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerStay(Collider other)
    {
        if (finished) return;

        if (other.CompareTag("Player"))
        {
            interactionUI.SetActive(true);
            interactionUI.transform.forward = Camera.forward;
            if (Input.GetKeyDown(KeyCode.E))
            {
                cinemachineCamera.gameObject.SetActive(true);
                StartCoroutine(Dialogue());
            }
                
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            interactionUI.SetActive(false);
    }

    IEnumerator Dialogue()
    {
        Debug.Log("대화 시작");

        finished = true; //코루틴 연속 방지
        player.GetComponent<PlayerMove>().currentState = PlayerMove.PlayerState.Dialogue; // 애니메이션 종료
        player.GetComponent<PlayerMove>().MoveEnable(false); // 이동 중지(input 못받음)

        player.transform.forward = -transform.forward;
        Vector3 targetPosition = transform.position + transform.forward * dialogueDistance;
        targetPosition.y = player.transform.position.y;

        //여기에 카메라 이동

        while (Vector3.Distance(player.transform.position, targetPosition) > 0.01f)
        {
            // 현재 위치에서 targetPosition으로 moveSpeed의 속도로 이동
            player.transform.position = Vector3.MoveTowards(player.transform.position, targetPosition, 1 * Time.deltaTime);
            yield return null;
        }
        player.transform.position = targetPosition;
        
        uIManager.TurnOnUI(UIManager.UITypes.Dialogue); // 대화 ui 키기
        if (uIManager.GetUIPanel(UIManager.UITypes.Dialogue) is DialogueUI dialogueUI)
        {
            dialogueUI.StartDialogue(dialogues,cinemachineCamera); // 대화 ui 진행 시작하기
        }
        yield break;
    }
    
    
}
