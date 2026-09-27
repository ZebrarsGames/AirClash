using UnityEngine;
using System;
using System.Globalization;
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
    private const string MainMenuSceneName = "MainMenu";
    private static readonly string TimerTemplate = "До обновления квестов: {0:00}:{1:00}:{2:00}";

    private bool isMainMenu;
    private float nextUpdate;
    private DateTime nextMidnightTime;
    private int lastRenderedSeconds = -1;

    private void Awake()
    {
        todayPool = new DailyQuestSO[maxQuests];
        CheckQuestAvailability();
    }

    private void Start()
    {
        isMainMenu = SceneManager.GetActiveScene().name == MainMenuSceneName;
    }

    private void Update()
    {
        if(!isMainMenu || Time.time < nextUpdate) return;
        nextUpdate = Time.time + 0.2f;

        UpdateTimer();
    }

    private void CheckQuestAvailability()
    {
        if(!PlayerPrefs.HasKey(NextMidnightTimeKey))
        {
            nextDayEvent?.Invoke();
            GenerateNewQuests();
            return;
        }

        string nextMidnightStr = PlayerPrefs.GetString(NextMidnightTimeKey);

        if(DateTime.TryParse(nextMidnightStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime savedMidnight))
        {
            nextMidnightTime = savedMidnight;
        }
        else
        {
            nextDayEvent?.Invoke();
            GenerateNewQuests();
            return;
        }

        if(DateTime.Now >= nextMidnightTime)
        {
            nextDayEvent?.Invoke();
            GenerateNewQuests();
        }
        else
        {
            LoadSavedQuests();
        }
    }

    private void GenerateNewQuests()
    {
        if(quests == null || quests.Length == 0) return;

        for(int i = 0; i < quests.Length; i++)
        {
            if(quests[i] != null)
            {
                QuestSaveSystem.RemoveQuest(quests[i].QuestId);
            }
        }

        int[] savedIds = new int[maxQuests];
        Array.Clear(todayPool, 0, todayPool.Length);

        for(int i = 0; i < maxQuests; i++)
        {
            int rand = 0;
            int safetyAttempts = 0;
            do
            {
                rand = UnityEngine.Random.Range(0, quests.Length);
                safetyAttempts++;
            }
            while((IsTodayHasQuest(quests[rand].QuestId) || IsTodayHasSeries(quests[rand].DailyQuestSeries)) && safetyAttempts < 100);

            savedIds[i] = rand;
            todayPool[i] = quests[rand];
        }

        string idsString = string.Join(",", savedIds);
        PlayerPrefs.SetString(QuestIdsKey, idsString);

        nextMidnightTime = DateTime.Today.AddDays(1);
        PlayerPrefs.SetString(NextMidnightTimeKey, nextMidnightTime.ToString("o", CultureInfo.InvariantCulture));
        PlayerPrefs.Save();
    }

    private void LoadSavedQuests()
    {
        if(!PlayerPrefs.HasKey(QuestIdsKey)) return;

        string idsString = PlayerPrefs.GetString(QuestIdsKey);
        string[] splitIds = idsString.Split(',');

        for(int i = 0; i < maxQuests; i++)
        {
            if(i < splitIds.Length && int.TryParse(splitIds[i], out int questIndex))
            {
                if(questIndex >= 0 && questIndex < quests.Length && i < todayPool.Length)
                {
                    todayPool[i] = quests[questIndex];
                }
            }
        }
    }

    public void UpdateQuestProgress(string questId, int amount)
    {
        if(string.IsNullOrEmpty(questId)) return;

        for(int i = 0; i < todayPool.Length; i++)
        {
            DailyQuestSO currentQuest = todayPool[i];
            if(currentQuest != null && currentQuest.QuestId == questId)
            {
                if(QuestSaveSystem.GetIsCompleted(currentQuest.QuestId)) return;

                QuestSaveSystem.PlusProgress(currentQuest.QuestId, amount);
                int currentProgress = QuestSaveSystem.GetProgress(currentQuest.QuestId);

                if(currentProgress > currentQuest.Target)
                {
                    QuestSaveSystem.SetProgress(currentQuest.QuestId, currentQuest.Target);
                    currentProgress = currentQuest.Target;
                }

                if(currentProgress >= currentQuest.Target)
                {
                    achievementsHandler?.UpdateProgress("daily", 1);
                    QuestSaveSystem.SetCompleted(currentQuest.QuestId);
                    GiveAward(currentQuest);
                }

                break;
            }
        }
    }

    public void GiveAward(DailyQuestSO currentQuest)
    {
        if(currentQuest == null) return;

        switch(currentQuest.AwardType)
        {
            case AwardType.Money:
                PlayerPrefs.SetInt("HowMoneyAdds", PlayerPrefs.GetInt("HowMoneyAdds") + currentQuest.Award);
                break;
            case AwardType.Xp:
                PlayerPrefs.SetInt("HowXpAdds", PlayerPrefs.GetInt("HowXpAdds") + currentQuest.Award);
                break;
        }
    }

    public bool IsTodayHasQuest(string questId)
    {
        if(string.IsNullOrEmpty(questId)) return false;

        for(int i = 0; i < todayPool.Length; i++)
        {
            if(todayPool[i] != null && todayPool[i].QuestId == questId)
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
        if(moneyHandler != null && moneyHandler.GetMoney() >= generateNewQuestsCost)
        {
            if(audioSource != null && buySound != null) audioSource.PlayOneShot(buySound);
            moneyHandler.RemoveMoney(generateNewQuestsCost);
            GenerateNewQuests();
            generateNewQuestsEvent?.Invoke();
            if(moneyText != null) moneyText.SetText($"{moneyHandler.GetMoney()} <sprite=0>");
        }
        else if(audioSource != null && cancelSound != null)
        {
            audioSource.PlayOneShot(cancelSound);
        }
    }

    private void UpdateTimer()
    {
        DateTime now = DateTime.Now;

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

        if(seconds == lastRenderedSeconds) return;
        lastRenderedSeconds = seconds;

        if(statusText != null) statusText.SetText(TimerTemplate, totalHours, minutes, seconds);
    }
}