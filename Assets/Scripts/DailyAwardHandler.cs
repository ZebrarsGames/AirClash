using System;
using UnityEngine;

public class DailyAwardHandler : MonoBehaviour
{
    [Header("Arrays")]
    [SerializeField] private DailyAwardSO[] dailyAwards;

    [Header("Scripts")]
    [SerializeField] private MoneyHandler moneyHandler;
    [SerializeField] private XpHandler xpHandler;

    [Header("Floats")]
    [SerializeField] private int maxDays = 7;

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
                moneyHandler.AddMoney(award.Award);
                break;
            case AwardType.Xp:
                xpHandler.AddXp(award.Award);
                break;
            case AwardType.Skin:
                PlayerPrefs.SetInt(award.SkinAward, 1);
                PlayerPrefs.Save();
                break;
        }
    }
}
