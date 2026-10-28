using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LeaderboardCard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nickText;
    [SerializeField] private TextMeshProUGUI eloText;
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private Image avatarImage;

    public void Setup(string username, int elo, int rank)
    {
        if(nickText != null) nickText.SetText(username);
        if(eloText != null) eloText.SetText("{0} ЭЛО", elo);
        if(rankText != null) rankText.SetText("{0}.", rank);
        if(avatarImage != null)
        {
            // avatarImage.gameObject.SetActive(false); 
        }
    }
}