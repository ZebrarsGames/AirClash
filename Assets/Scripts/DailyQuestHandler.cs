using UnityEngine;
using System;
using UnityEngine.UI;

public class DailyQuestHandler : MonoBehaviour
{
    [SerializeField] private DailyQuestSO[] quests;
    [SerializeField] private DailyQuestItem[] dailyQuestItems;
    [SerializeField] private Text statusText;
    private const string LastQuestTimeKey = "LastTimeSave";
    private const int RewardIntervalHours = 24;
    void Start()
    {
        CheckQuestAvailability();
    }
    private void CheckQuestAvailability() {
        if (!PlayerPrefs.HasKey(LastQuestTimeKey))
        {
            UpdateQuests();
            return;
        }

        // Получаем время последнего получения
        string lastClaimedStr = PlayerPrefs.GetString(LastQuestTimeKey);
        DateTime lastClaimedTime = DateTime.Parse(lastClaimedStr);
        
        // Вычисляем, сколько времени прошло
        TimeSpan timePassed = DateTime.Now - lastClaimedTime;

        if (timePassed.TotalHours >= RewardIntervalHours)
        {
            UpdateQuests();
        }
    }
    private void UpdateQuests()
    {
        for(int i = 0; i < dailyQuestItems.Length; i++)
        {
            int rand = UnityEngine.Random.Range(0, quests.Length);
            dailyQuestItems[i].SetData(quests[rand]);
        }
    }
    private void UpdateTimer()
    {
        string lastClaimedStr = PlayerPrefs.GetString(LastQuestTimeKey, DateTime.MinValue.ToString());
        DateTime nextClaimTime = DateTime.Parse(lastClaimedStr).AddHours(RewardIntervalHours);
        TimeSpan timeLeft = nextClaimTime - DateTime.Now;
        // Форматируем вывод в вид ЧЧ:ММ:СС
        statusText.text = string.Format("До следующей награды: {0:D2}:{1:D2}:{2:D2}", 
        timeLeft.Hours, timeLeft.Minutes, timeLeft.Seconds);
    }
}
