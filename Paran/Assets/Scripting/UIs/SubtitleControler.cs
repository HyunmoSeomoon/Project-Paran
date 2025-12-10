using TMPro;
using UnityEngine;

public class SubtitleController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI subtitleText;

    public void ChangeSubtitle(string message)
    {
        subtitleText.text = message;
    }

    public void ClearSubtitle()
    {
        subtitleText.text = "";
    }
}
