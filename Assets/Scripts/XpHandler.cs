using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class XpHandler : MonoBehaviour
{
    private int currentXP = 0;
    private int oldXp = 0;
    private int xpToNextLevel = 100;
    private int level = 1;
    public UnityEvent onLevelUp; 
    public int GetXP() => currentXP;
    public int GetOldXP() => oldXp;
    public int GetXpToNextLevel() => xpToNextLevel;
    public int GetLevel() => level;

    public class XpAward
    {
        public string TypeOfAward;
        public int Award;
        public string SkinAward;
        public int RequiredLevel;
    }

    private Dictionary<string, XpAward> xpAwards = new Dictionary<string, XpAward>();

    void Start()
    {
        currentXP = PlayerPrefs.GetInt("CurrentXp", 0);
        level = PlayerPrefs.GetInt("XpLevel", 1);
        xpToNextLevel = PlayerPrefs.GetInt("XpToNextLevel", 100);
        Log();

        xpAwards.Add("AwardFor1Level", new XpAward { TypeOfAward = "Money", Award = 10, RequiredLevel = 1 });
        xpAwards.Add("AwardFor2Level", new XpAward { TypeOfAward = "Money", Award = 15, RequiredLevel = 2 });
        xpAwards.Add("AwardFor3Level", new XpAward { TypeOfAward = "Money", Award = 20, RequiredLevel = 3 });
        // xpAwards.Add("AwardFor3LevelSkin", new XpAward { TypeOfAward = "Skin", SkinAward = "SkinFor3Level", RequiredLevel = 3 });
        xpAwards.Add("AwardFor4Level", new XpAward { TypeOfAward = "Money", Award = 40, RequiredLevel = 4 });
        xpAwards.Add("AwardFor5Level", new XpAward { TypeOfAward = "Money", Award = 50, RequiredLevel = 5 });
        // xpAwards.Add("AwardFor5LevelSkin", new XpAward { TypeOfAward = "Skin", SkinAward = "SkinFor5Level", RequiredLevel = 5 });
        xpAwards.Add("AwardFor6Level", new XpAward { TypeOfAward = "Money", Award = 70, RequiredLevel = 6 });
        xpAwards.Add("AwardFor7Level", new XpAward { TypeOfAward = "Money", Award = 100, RequiredLevel = 7 });
        xpAwards.Add("AwardFor8Level", new XpAward { TypeOfAward = "Money", Award = 150, RequiredLevel = 8 });
        xpAwards.Add("AwardFor9Level", new XpAward { TypeOfAward = "Money", Award = 170, RequiredLevel = 9 });
        xpAwards.Add("AwardFor10Level", new XpAward { TypeOfAward = "Money", Award = 200, RequiredLevel = 10 });
        // xpAwards.Add("AwardFor10LevelSkin", new XpAward { TypeOfAward = "Money", SkinAward = "SkinFor10Level", RequiredLevel = 10 });
        for(int i = 11; i <= 30; i++)
        {
            xpAwards.Add("AwardFor" + i + "Level", new XpAward { TypeOfAward = "Money", Award = i*15, RequiredLevel = i });
        }
    }

    public void AddXp(int amount)
    {
        oldXp = currentXP;
        currentXP += amount;
        if(currentXP >= xpToNextLevel)
        {
            currentXP -= xpToNextLevel;
            LevelUp();
        }
        Save();
        Log();
    }

    private void LevelUp()
    {
        onLevelUp.Invoke();
        level++;
        xpToNextLevel += 50;
        foreach(var pair in xpAwards)
        {
            XpAward xpAward = pair.Value;
            if(xpAward.RequiredLevel == level)
            {
                switch(xpAward.TypeOfAward)
                {
                    case "Money":
                        PlayerPrefs.SetInt("HowMoneyAdds", PlayerPrefs.GetInt("HowMoneyAdds") + xpAward.Award);
                        PlayerPrefs.Save();
                        break;
                }
            }
        }
        Save();
        Log();
    }

    public float GetXPProgress()
    {
        return (float)currentXP / xpToNextLevel;
    }

    public float GetOldXPProgress()
    {
        return (float)oldXp / xpToNextLevel;
    }

    public float GetXPProgress(int _currentXP, int _xpToNextLevel)
    {
        return (float)_currentXP / _xpToNextLevel;
    }

    public float GetXPProgress(int _currentXP)
    {
        return (float)_currentXP / xpToNextLevel;
    }

    private void Save()
    {
        PlayerPrefs.SetInt("CurrentXp", currentXP);
        PlayerPrefs.SetInt("XpLevel", level);
        PlayerPrefs.SetInt("XpToNextLevel", xpToNextLevel);
        PlayerPrefs.Save();
    }

    private void Log()
    {
        Debug.Log("Current Xp: " + currentXP);
        Debug.Log("Current level: " + level);
    }
    public void ResetOldXp() 
    {
        oldXp = currentXP; 
    }

}
