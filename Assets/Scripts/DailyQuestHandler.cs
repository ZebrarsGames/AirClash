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
    void FixedUpdate()
    {
        UpdateTimer();
    }
    private void CheckQuestAvailability() {
        if (!PlayerPrefs.HasKey(LastQuestTimeKey))
        {
            UpdateQuests();
            return;
        }

        string lastClaimedStr = PlayerPrefs.GetString(LastQuestTimeKey);
        DateTime lastClaimedTime = DateTime.Parse(lastClaimedStr);
        
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
        DateTime now = DateTime.Now;
        
        DateTime nextMidnight = now.Date.AddDays(1);
        
        TimeSpan timeLeft = nextMidnight - now;

        statusText.text = string.Format("До обновления квестов: {0:D2}:{1:D2}:{2:D2}", 
            (int)timeLeft.TotalHours, 
            timeLeft.Minutes, 
            timeLeft.Seconds);
    }
}
