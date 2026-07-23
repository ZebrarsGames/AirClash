using System;
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

    void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        if(isInitialized) return;

        if(PlayerPrefs.HasKey(FirstTimePlayKey))
        {
            if(!DateTime.TryParse(PlayerPrefs.GetString(FirstTimePlayKey), out firstTimePlay))
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
        PlayerPrefs.SetString(FirstTimePlayKey, firstTimePlay.ToString());
        PlayerPrefs.Save();
    }

    public void OnDayChanged()
    {
        int daysPlayed = (DateTime.Today - firstTimePlay).Days + 1;
        if(daysPlayed > maxDays) return;

        for (int i = 0; i < dailyAwards.Length; i++)
        {
            if (dailyAwards[i].Day == daysPlayed)
            {
                awardsPanel.SetActive(true);
                GiveAward(dailyAwards[i]);
                break;
            }
        }
    } 

    public void GiveAward(DailyAwardSO award)
    {
        Debug.Log("Выдаём награду за " + award.Day + " день");
        switch(award.AwardType)
        {
            case AwardType.Money:
                UpdateMoneyQuests(award.Award);
                moneyHandler.AddMoney(award.Award);
                break;
            case AwardType.Xp:
                UpdateXpQuests(award.Award);
                xpHandler.AddXp(award.Award);
                break;
            case AwardType.Skin:
                achievementsHandler.UpdateProgress("large_wardrobe", 1);
                PlayerPrefs.SetInt(award.SkinAward, 1);
                PlayerPrefs.Save();
                break;
        }
    }

    public DailyAwardSO GetDailyAward(int day)
    {
        for (int i = 0; i < dailyAwards.Length; i++)
        {
            if (dailyAwards[i].Day == day)
            {
                return dailyAwards[i];
            }
        }
        return new DailyAwardSO();
    }

    public int GetDaysPlayed()
    {
        return (DateTime.Today - firstTimePlay).Days + 1;
    }

    private void UpdateXpQuests(int amount)
    {
        questsHandler.UpdateQuestProgress("xp100", amount);
        questsHandler.UpdateQuestProgress("xp200", amount);
        questsHandler.UpdateQuestProgress("xp400", amount);
        questsHandler.UpdateQuestProgress("xp500", amount);
        questsHandler.UpdateQuestProgress("xp700", amount);
        questsHandler.UpdateQuestProgress("xp1000", amount);
        dailyQuestHandler.UpdateQuestProgress("xp50", amount);
    }

    private void UpdateMoneyQuests(int amount)
    {
        questsHandler.UpdateQuestProgress("money10", amount);
        questsHandler.UpdateQuestProgress("money50", amount);
        questsHandler.UpdateQuestProgress("money100", amount);
        questsHandler.UpdateQuestProgress("money200", amount);
        questsHandler.UpdateQuestProgress("money300", amount);
        questsHandler.UpdateQuestProgress("money500", amount);
        dailyQuestHandler.UpdateQuestProgress("daily_money50", amount);
        dailyQuestHandler.UpdateQuestProgress("money70", amount);
        dailyQuestHandler.UpdateQuestProgress("daily_money100", amount);
    }
}
