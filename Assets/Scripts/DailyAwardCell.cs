using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DailyAwardCell : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image cellLogo;
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private GameObject checkMark;

    [Header("Floats")]
    [SerializeField] private int day;
    [SerializeField] private bool isGive;

    [Header("Scripts")]
    [SerializeField] private DailyAwardHandler dailyAwardHandler;

    private void Start()
    {
        if(dailyAwardHandler == null) return;

        DailyAwardSO currentSO = dailyAwardHandler.GetDailyAward(day);
        if(currentSO != null && cellLogo != null)
        {
            cellLogo.sprite = currentSO.AwardSprite;
        }

        if(dayText != null)
        {
            dayText.SetText("День {0}", day);
        }

        isGive = dailyAwardHandler.GetDaysPlayed() >= day;

        if(checkMark != null)
        {
            checkMark.SetActive(isGive);
        }
    }
}