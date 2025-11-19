using System;
using System.Collections;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class DialogueController : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private DialogueNode[] dialogues;
    [SerializeField] private GameObject interactionUI;
    [SerializeField] private UIManager uIManager;
    [SerializeField] private Transform Camera;
    [SerializeField] private CinemachineCamera cinemachineCamera;
    public bool finished = false;
    private CameraMove cameraMove;
    private bool canDialogue = false;

    public float dialogueDistance = 1f;

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
        if (canDialogue)
            if (Input.GetKeyDown(KeyCode.E))
            {
                cinemachineCamera.gameObject.SetActive(true);
                StartCoroutine(Dialogue());
            }
    }

    void OnTriggerStay(Collider other)
    {
        if (finished) return;

        if (other.CompareTag("Player"))
        {
            interactionUI.SetActive(true);
            interactionUI.transform.forward = Camera.forward;
            canDialogue = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canDialogue = false;
            interactionUI.SetActive(false);
        }
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

        while (Vector3.Distance(player.transform.position, targetPosition) > 0.01f)
        {
            // 현재 위치에서 targetPosition으로 moveSpeed의 속도로 이동
            player.transform.position = Vector3.MoveTowards(player.transform.position, targetPosition, 2 * Time.deltaTime);
            yield return null;
        }
        player.transform.position = targetPosition;

        uIManager.TurnOnUI(UIManager.UITypes.Dialogue); // 대화 ui 키기
        if (uIManager.GetUIPanel(UIManager.UITypes.Dialogue) is DialogueUI dialogueUI)
        {
            dialogueUI.StartDialogue(dialogues, cinemachineCamera); // 대화 ui 진행 시작하기
        }
        yield break;
    }

}

[System.Serializable]
public class Choice
{
    [TextArea(2, 3)]
    public string choiceText;     // 버튼에 표시될 텍스트
    public int nextDialogueIndex; // 이 선택지를 골랐을 때 점프할 'allDialogues' 배열의 인덱스
    public UnityEvent OnDialogueNodeEnd;
}

[System.Serializable]
public class DialogueNode
{
    [TextArea(3, 5)]
    public string dialogueText;  // 현재 띄울 대사
    public Choice[] choices;     // 이 대사에 붙어있는 선택지들 (0개 ~ 4개)
    public bool dialogueEnd = false;
    public UnityEvent OnDialogueNodeEnd;
}
