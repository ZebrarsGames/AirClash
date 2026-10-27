using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LeaderboardCard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nickText;
    [SerializeField] private TextMeshProUGUI eloText;
    [SerializeField] private Image avatarImage;

    public void Setup(string username, int elo)
    {
        if(nickText != null) nickText.SetText(username);
        if(eloText != null) eloText.SetText("{0} ЭЛО", elo);
        
        if(avatarImage != null)
        {
            // avatarImage.gameObject.SetActive(false); 
        }
    }
}