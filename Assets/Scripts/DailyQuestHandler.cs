using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DailyQuestHandler : MonoBehaviour
{
    [SerializeField] private DailyQuestSO[] quests;
    [SerializeField] private int maxQuests;
    private DailyQuestSO[] todayPool = new DailyQuestSO[3];
    [SerializeField] private Text statusText;
    [SerializeField] private AchievementsHandler achievementsHandler;
    
    private const string NextMidnightTimeKey = "NextMidnightSave";
    private const string QuestIdsKey = "SavedQuestIds"; 
    private bool isMainMenu;
    private float _nextUpdate;

    void Awake()
    {
        CheckQuestAvailability();
    }

    void Start()
    {
        if(SceneManager.GetActiveScene().name.Equals("MainMenu")) isMainMenu = true;
        else isMainMenu = false;
    }

    void Update() 
    {
        if (Time.time < _nextUpdate || !isMainMenu) return;
        _nextUpdate = Time.time + 0.1f;

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
        for(int i = 0; i < quests.Length; i++)
        {
            QuestSaveSystem.RemoveQuest(quests[i].QuestId);
        }
        int[] savedIds = new int[maxQuests];

        for(int i = 0; i < maxQuests; i++)
        {
            int rand = UnityEngine.Random.Range(0, quests.Length);
            if(todayPool[i] != null && IsTodayHasQuest(quests[rand].QuestId)) GenerateNewQuests();
            savedIds[i] = rand;
            todayPool[i] = quests[rand];
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

        for (int i = 0; i < maxQuests; i++)
        {
            if (i < splitIds.Length && int.TryParse(splitIds[i], out int questIndex))
            {
                if (questIndex >= 0 && questIndex < quests.Length)
                {           
                    if (i < todayPool.Length)
                    {
                        todayPool[i] = quests[questIndex];
                    }
                }
            }
        }
    }

    public void UpdateQuestProgress(string questId, int amount)
    {
        if(IsTodayHasQuest(questId))
        {
            for(int i = 0; i < quests.Length; i++)
            {  
                if(quests[i].QuestId.Equals(questId))
                {
                    DailyQuestSO currentQuest = quests[i];
                    if(QuestSaveSystem.GetIsCompleted(currentQuest.QuestId)) return;
                    QuestSaveSystem.PlusProgress(currentQuest.QuestId, amount);
                    if(amount > currentQuest.Target) 
                    { 
                        QuestSaveSystem.SetProgress(currentQuest.QuestId, currentQuest.Target);
                    }
                    Debug.Log("Прогресс у " + questId +  " стал больше на " + amount);
                    if(QuestSaveSystem.GetProgress(currentQuest.QuestId) >= currentQuest.Target)
                    {
                        achievementsHandler.UpdateProgress("daily", 1);
                        QuestSaveSystem.SetCompleted(currentQuest.QuestId);
                        GiveAward(currentQuest.QuestId);
                    }
                }
            }
        }
    }
    public void GiveAward(string questId)
    {
        for(int i = 0; i < quests.Length; i++)
        {
            if(quests[i].QuestId.Equals(questId))
            {
                DailyQuestSO currentQuest = quests[i];
                switch(currentQuest.AwardType)
                {
                    case AwardType.Money:
                        PlayerPrefs.SetInt("HowMoneyAdds", PlayerPrefs.GetInt("HowMoneyAdds") + currentQuest.Award);
                        PlayerPrefs.Save();
                        break;
                    case AwardType.Xp:
                        PlayerPrefs.SetInt("HowXpAdds", PlayerPrefs.GetInt("HowXpAdds") + currentQuest.Award);
                        PlayerPrefs.Save();
                        break;     
                }
                Debug.Log("Награда за " + questId + " выдана!");
            }
        }
    }
    public bool IsTodayHasQuest(string questId)
    {
        for(int i = 0; i < todayPool.Length; i++)
        {
            if(todayPool[i].QuestId.Equals(questId))
            {
                return true;
            }
        }
        return false;
    }
    public DailyQuestSO[] GetTodayPool()
    {
        return todayPool;
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