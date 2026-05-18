using UnityEngine;
using System;
using UnityEngine.UI;

public class DailyQuestHandler : MonoBehaviour
{
    [SerializeField] private DailyQuestSO[] quests;
    [SerializeField] private DailyQuestItem[] dailyQuestItems;
    [SerializeField] private Text statusText;
    
    private const string NextMidnightTimeKey = "NextMidnightSave";
    private const string QuestIdsKey = "SavedQuestIds"; 

    void Start()
    {
        CheckQuestAvailability();
    }

    void FixedUpdate() 
    {
        UpdateTimer();
    }

    private void CheckQuestAvailability() 
    {
        if (!PlayerPrefs.HasKey(NextMidnightTimeKey))
        {
            GenerateNewQuests();
            return;
        }

        string nextMidnightStr = PlayerPrefs.GetString(NextMidnightTimeKey);
        DateTime nextMidnight = DateTime.Parse(nextMidnightStr);
        
        if (DateTime.Now >= nextMidnight)
        {
            GenerateNewQuests();
        }
        else
        {
            LoadSavedQuests();
        }
    }

    private void GenerateNewQuests()
    {
        int[] savedIds = new int[dailyQuestItems.Length];

        for(int i = 0; i < dailyQuestItems.Length; i++)
        {
            int rand = UnityEngine.Random.Range(0, quests.Length);
            dailyQuestItems[i].SetData(quests[rand]);
            savedIds[i] = rand;
        }

        string idsString = string.Join(",", savedIds);
        PlayerPrefs.SetString(QuestIdsKey, idsString);

        DateTime nextMidnight = DateTime.Now.Date.AddDays(1);
        PlayerPrefs.SetString(NextMidnightTimeKey, nextMidnight.ToString());
        
        PlayerPrefs.Save();
    }

    private void LoadSavedQuests()
    {
        if (!PlayerPrefs.HasKey(QuestIdsKey)) return;

        string idsString = PlayerPrefs.GetString(QuestIdsKey);
        string[] splitIds = idsString.Split(',');

        for (int i = 0; i < dailyQuestItems.Length; i++)
        {
            if (i < splitIds.Length && int.TryParse(splitIds[i], out int questIndex))
            {
                if (questIndex >= 0 && questIndex < quests.Length)
                {
                    dailyQuestItems[i].SetData(quests[questIndex]);
                }
            }
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