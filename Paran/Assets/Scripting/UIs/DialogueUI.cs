using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : IUIPanel
{
    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private TextMeshProUGUI textMeshPro;
    [SerializeField] private Button[] choiceButtons;
    [SerializeField] private Button nextButton;
    [SerializeField] PlayerMove playerMove;
    public int size = 0;
    private int count = 0;
    private DialogueNode[] newDialogues = null;
    private CinemachineCamera currentCinemachine;
    public override void Show()
    {
        if (dialogueUI != null)
            dialogueUI.SetActive(true);
    }

    public override void Hide()
    {
        if (dialogueUI != null)
            dialogueUI.SetActive(false);
    }

    public void StartDialogue(DialogueNode[] dialogues, CinemachineCamera cinemachineCamera)
    {
        for (int i = 0; i < 4; i++)
        {
            choiceButtons[i].gameObject.SetActive(false);
        }
        size = dialogues.Length;
        if (size != 0)
        {
            newDialogues = dialogues;
            if (newDialogues[0].dialogueText != "")
            {
                textMeshPro.text = newDialogues[0].dialogueText;
                textMeshPro.gameObject.SetActive(true);
            }
            else
            {
                textMeshPro.gameObject.SetActive(false);
                for (int i = 0; i < newDialogues[0].choices.Length; i++)
                {
                    if (newDialogues[0].choices[i].choiceText != null)
                    {
                        choiceButtons[i].transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = newDialogues[0].choices[i].choiceText;
                        choiceButtons[i].gameObject.SetActive(true);
                    }
                }
            }
            currentCinemachine = cinemachineCamera;
        }
        else
            Debug.Log("대화 내용이 없습니다.");
    }

    public void UpdateDialogue()
    {
        if (count+1 < size && !newDialogues[count].dialogueEnd)
        {
            count++;
            newDialogues[count].OnDialogueNodeEnd?.Invoke(); 

            if (newDialogues[count].dialogueText != "")
                textMeshPro.text = newDialogues[count].dialogueText;
            else
            {
                for (int i = 0; i < newDialogues[count].choices.Length; i++)
                {
                    if (newDialogues[count].choices[i].choiceText != null)
                    {
                        choiceButtons[i].transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = newDialogues[count].choices[i].choiceText;
                        choiceButtons[i].gameObject.SetActive(true);
                    }
                }
                nextButton.gameObject.SetActive(false);
            }
        }
        else
        {
            playerMove.MoveEnable(true);
            count=0;
            playerMove.currentState = PlayerMove.PlayerState.Stand;
            currentCinemachine.gameObject.SetActive(false);
            Hide();
        }
    }

    public void ChoiceUpdate(int index)
    {
        newDialogues[count].choices[index].OnDialogueNodeEnd?.Invoke(); 
        for (int i = 0; i < newDialogues[count].choices.Length; i++)
        {
            choiceButtons[i].gameObject.SetActive(false);
        }
        count = newDialogues[count].choices[index].nextDialogueIndex - 1;
        UpdateDialogue();
        nextButton.gameObject.SetActive(true);
    }
}