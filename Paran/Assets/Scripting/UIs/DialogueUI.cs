using TMPro;
using Unity.Cinemachine;
using UnityEngine;

public class DialogueUI : IUIPanel
{
    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private TextMeshProUGUI textMeshPro;
    [SerializeField] PlayerMove playerMove;
    public int size = 0;
    private int count = 0;
    private string[] newDialogues = null;
    private CinemachineCamera currentCinemachine;
    public override void Show()
    {
        if(dialogueUI!=null)
            dialogueUI.SetActive(true);
    }

    public override void Hide()
    {
        if (dialogueUI != null)
            dialogueUI.SetActive(false);
    }

    public void StartDialogue(string[] dialogues, CinemachineCamera cinemachineCamera)
    {
        size = dialogues.Length;
        if (size != 0)
        {
            newDialogues = dialogues;
            textMeshPro.text = newDialogues[0];
            currentCinemachine = cinemachineCamera;
        }
        else
            Debug.Log("대화 내용이 없습니다.");
    }
    
    public void UpdateDialogue()
    {
        count++;
        if (count < size)
        {
            textMeshPro.text = newDialogues[count];
        }
        else
        {
            playerMove.MoveEnable(true);
            playerMove.currentState = PlayerMove.PlayerState.Stand;
            currentCinemachine.gameObject.SetActive(false);
            Hide();
        }
    }
}