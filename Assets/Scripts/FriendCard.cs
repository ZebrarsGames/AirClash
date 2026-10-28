using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FriendCard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nickText;
    [SerializeField] private TextMeshProUGUI eloText;
    [SerializeField] private TextMeshProUGUI onlineStatusText;
    public Button sendBattleInviteBtn;
    public TextMeshProUGUI sendBattleInviteBtnText;

    public void Setup(string username, int elo, bool isOnline)
    {
        if(nickText != null) nickText.SetText(username);
        if(eloText != null) eloText.SetText("{0} ЭЛО", elo);
        if(onlineStatusText != null) onlineStatusText.SetText(isOnline ? "Онлайн" : "Оффлайн");
    }
}
