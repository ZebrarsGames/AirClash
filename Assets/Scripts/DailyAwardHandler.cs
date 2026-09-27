using System;
using System.Globalization;
using UnityEngine;

public class DailyAwardHandler : MonoBehaviour
{
    [Header("Arrays")]
    [SerializeField] private DailyAwardSO[] dailyAwards;

    [Header("Scripts")]
    [SerializeField] private MoneyHandler moneyHandler;
    [SerializeField] private XpHandler xpHandler;
    [SerializeField] private QuestsHandler questsHandler;
    [SerializeField] private DailyQuestHandler dailyQuestHandler;
    [SerializeField] private AchievementsHandler achievementsHandler;

    [Header("Floats")]
    [SerializeField] private int maxDays = 7;

    [Header("UI")]
    [SerializeField] private GameObject awardsPanel;

    private DateTime firstTimePlay;
    private const string FirstTimePlayKey = "FirstTimePlayed";
    private bool isInitialized;

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        if(isInitialized) return;

        if(PlayerPrefs.HasKey(FirstTimePlayKey))
        {
            string dateStr = PlayerPrefs.GetString(FirstTimePlayKey);
            if(!DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out firstTimePlay))
            {
                ResetFirstTime();
            }
        }
        else
        {
            ResetFirstTime();
        }

        isInitialized = true;
    }

    private void ResetFirstTime()
    {
        firstTimePlay = DateTime.Today;
        PlayerPrefs.SetString(FirstTimePlayKey, firstTimePlay.ToString("o", CultureInfo.InvariantCulture));
        PlayerPrefs.Save();
    }

    public void OnDayChanged()
    {
        int daysPlayed = GetDaysPlayed();
        if(daysPlayed > maxDays || dailyAwards == null) return;

        for(int i = 0; i < dailyAwards.Length; i++)
        {
            if(dailyAwards[i] != null && dailyAwards[i].Day == daysPlayed)
            {
                if(awardsPanel != null) awardsPanel.SetActive(true);
                GiveAward(dailyAwards[i]);
                break;
            }
        }
    }

    public void GiveAward(DailyAwardSO award)
    {
        if(award == null) return;

        switch(award.AwardType)
        {
            case AwardType.Money:
                UpdateMoneyQuests(award.Award);
                if(moneyHandler != null) moneyHandler.AddMoney(award.Award);
                break;
            case AwardType.Xp:
                UpdateXpQuests(award.Award);
                if(xpHandler != null) xpHandler.AddXp(award.Award);
                break;
            case AwardType.Skin:
                if(achievementsHandler != null) achievementsHandler.UpdateProgress("large_wardrobe", 1);
                PlayerPrefs.SetInt(award.SkinAward, 1);
                PlayerPrefs.Save();
                break;
        }
    }

    public DailyAwardSO GetDailyAward(int day)
    {
        if(dailyAwards == null) return null;

        for(int i = 0; i < dailyAwards.Length; i++)
        {
            if(dailyAwards[i] != null && dailyAwards[i].Day == day)
            {
                return dailyAwards[i];
            }
        }
        return null;
    }

    public int GetDaysPlayed()
    {
        return (DateTime.Today - firstTimePlay).Days + 1;
    }

    private void UpdateXpQuests(int amount)
    {
        if(questsHandler != null)
        {
            questsHandler.UpdateQuestProgress("xp100", amount);
            questsHandler.UpdateQuestProgress("xp200", amount);
            questsHandler.UpdateQuestProgress("xp400", amount);
            questsHandler.UpdateQuestProgress("xp500", amount);
            questsHandler.UpdateQuestProgress("xp700", amount);
            questsHandler.UpdateQuestProgress("xp1000", amount);
        }

        if(dailyQuestHandler != null)
        {
            dailyQuestHandler.UpdateQuestProgress("xp50", amount);
        }
    }

    private void UpdateMoneyQuests(int amount)
    {
        if(questsHandler != null)
        {
            questsHandler.UpdateQuestProgress("money10", amount);
            questsHandler.UpdateQuestProgress("money50", amount);
            questsHandler.UpdateQuestProgress("money100", amount);
            questsHandler.UpdateQuestProgress("money200", amount);
            questsHandler.UpdateQuestProgress("money300", amount);
            questsHandler.UpdateQuestProgress("money500", amount);
        }

        if(dailyQuestHandler != null)
        {
            dailyQuestHandler.UpdateQuestProgress("daily_money50", amount);
            dailyQuestHandler.UpdateQuestProgress("money70", amount);
            dailyQuestHandler.UpdateQuestProgress("daily_money100", amount);
        }
    }
}