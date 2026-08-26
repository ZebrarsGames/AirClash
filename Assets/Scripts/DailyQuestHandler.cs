using UnityEngine;
using System;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class DailyQuestHandler : MonoBehaviour
{
    [Header("Arrays")]
    [SerializeField] private DailyQuestSO[] quests;
    private DailyQuestSO[] todayPool;

    [Header("Floats")]
    [SerializeField] private int maxQuests = 3;
    [SerializeField] private int generateNewQuestsCost = 100;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI moneyText;
    
    [Header("Scripts")]
    [SerializeField] private AchievementsHandler achievementsHandler;
    [SerializeField] private MoneyHandler moneyHandler;

    [Header("Sounds")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buySound;
    [SerializeField] private AudioClip cancelSound;

    [Header("Other")]
    [SerializeField] private UnityEvent generateNewQuestsEvent;
    [SerializeField] private UnityEvent nextDayEvent;
    
    private const string NextMidnightTimeKey = "NextMidnightSave";
    private const string QuestIdsKey = "SavedQuestIds"; 
    private bool isMainMenu;
    private float _nextUpdate;
    private DateTime nextMidnightTime;
    private int lastRenderedSeconds = -1;
    private static readonly string TimerTemplate = "До обновления квестов: {0:00}:{1:00}:{2:00}";

    void Awake()
    {
        todayPool = new DailyQuestSO[maxQuests];
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
        _nextUpdate = Time.time + 0.2f;

        UpdateTimer();
    }

    private void CheckQuestAvailability() 
    {
        if (!PlayerPrefs.HasKey(NextMidnightTimeKey))
        {
            nextDayEvent.Invoke();
            GenerateNewQuests();
            return;
        }

        string nextMidnightStr = PlayerPrefs.GetString(NextMidnightTimeKey);

        if (DateTime.TryParse(nextMidnightStr, out DateTime savedMidnight))
        {
            nextMidnightTime = savedMidnight;
        }
        else
        {
            nextDayEvent.Invoke();
            GenerateNewQuests();
            return;
        }
        
        if (DateTime.Now >= nextMidnightTime)
        {
            nextDayEvent.Invoke();
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
        System.Array.Clear(todayPool, 0, todayPool.Length); 

        for(int i = 0; i < maxQuests; i++)
        {
            int rand = 0;
            int safetyAttempts = 0; 
            do 
            {
                rand = UnityEngine.Random.Range(0, quests.Length);
                safetyAttempts++;
            } while ((IsTodayHasQuest(quests[rand].QuestId) || IsTodayHasSeries(quests[rand].DailyQuestSeries)) && safetyAttempts < 100);

            savedIds[i] = rand;
            todayPool[i] = quests[rand];
        }

        string idsString = string.Join(",", savedIds);
        PlayerPrefs.SetString(QuestIdsKey, idsString);

        nextMidnightTime = DateTime.Today.AddDays(1); 
        PlayerPrefs.SetString(NextMidnightTimeKey, nextMidnightTime.ToString());
        
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
            for(int i = 0; i < todayPool.Length; i++)
            {  
                if(todayPool[i] != null && todayPool[i].QuestId.Equals(questId))
                {
                    DailyQuestSO currentQuest = todayPool[i];

                    if(QuestSaveSystem.GetIsCompleted(currentQuest.QuestId)) return;

                    QuestSaveSystem.PlusProgress(currentQuest.QuestId, amount);
                    int currentProgress = QuestSaveSystem.GetProgress(currentQuest.QuestId);

                    if(currentProgress > currentQuest.Target) 
                    { 
                        QuestSaveSystem.SetProgress(currentQuest.QuestId, currentQuest.Target);
                        currentProgress = currentQuest.Target; 
                    }

                    Debug.Log("Прогресс у " + questId +  " стал больше на " + amount);

                    if(currentProgress >= currentQuest.Target)
                    {
                        achievementsHandler.UpdateProgress("daily", 1);
                        QuestSaveSystem.SetCompleted(currentQuest.QuestId);
                        GiveAward(currentQuest);
                    }

                    break;
                }
            }
        }
    }
    public void GiveAward(DailyQuestSO currentQuest)
    {
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
        Debug.Log("Награда за " + currentQuest.QuestId + " выдана!");
    }
    public bool IsTodayHasQuest(string questId)
    {
        for(int i = 0; i < todayPool.Length; i++)
        {
            if(todayPool[i] != null && todayPool[i].QuestId.Equals(questId))
            {
                return true;
            }
        }
        return false;
    }
    public bool IsTodayHasSeries(DailyQuestSeries series) 
    {
        for(int i = 0; i < todayPool.Length; i++) 
        {
            if(todayPool[i] != null && todayPool[i].DailyQuestSeries == series) 
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

    public void BuyNewQuests()
    {
        if(moneyHandler.GetMoney() >= generateNewQuestsCost)
        {
            audioSource.PlayOneShot(buySound);
            moneyHandler.RemoveMoney(generateNewQuestsCost);
            GenerateNewQuests();
            generateNewQuestsEvent.Invoke();
            moneyText.SetText($"{moneyHandler.GetMoney()} <sprite=0>");
        } else audioSource.PlayOneShot(cancelSound);
    }

    private void UpdateTimer()
    {
        DateTime now = DateTime.UtcNow;

        if(now >= nextMidnightTime) 
        {
            GenerateNewQuests();
            generateNewQuestsEvent?.Invoke();
            nextDayEvent?.Invoke();
            return;
        }

        TimeSpan timeLeft = nextMidnightTime - now;
        int totalHours = (int)timeLeft.TotalHours;
        int minutes = timeLeft.Minutes;
        int seconds = timeLeft.Seconds;

        if(seconds == lastRenderedSeconds)
        {
            return; 
        }
        lastRenderedSeconds = seconds;

        statusText.SetText(TimerTemplate, totalHours, minutes, seconds);
    }
}