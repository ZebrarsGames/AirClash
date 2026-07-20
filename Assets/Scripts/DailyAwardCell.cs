using UnityEngine;
using UnityEngine.UI;

public class DailyAwardCell : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image cellLogo;
    [SerializeField] private Text dayText;
    [SerializeField] private GameObject checkMark;

    [Header("Floats")]
    [SerializeField] private int day;
    [SerializeField] private bool isGive;

    [Header("Scripts")]
    [SerializeField] private DailyAwardHandler dailyAwardHandler;

    void Start()
    {
        DailyAwardSO currentSO = dailyAwardHandler.GetDailyAward(day);
        cellLogo.sprite = currentSO.AwardSprite;
        dayText.text = "День " + day.ToString();
        if(dailyAwardHandler.GetDaysPlayed() >= day) isGive = true;
        else isGive = false;
        checkMark.SetActive(isGive);
    }
}
