using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class XpHandler : MonoBehaviour
{
    private int currentXP = 0;
    private int totalXP = 0;
    private int oldXp = 0;
    private int xpToNextLevel = 100;
    private int level = 1;
    
    public UnityEvent onLevelUp; 
    [SerializeField] private AchievementsHandler achievementsHandler;
    [SerializeField] private SaveManager saveManager;

    public class XpAward
    {
        public string TypeOfAward;
        public int Award;
        public string SkinAward;
        public string GuiSkinName;
        public int RequiredLevel;
    }

    private Dictionary<string, XpAward> xpAwards = new Dictionary<string, XpAward>();
    private static readonly string[] AchievementsKeys = { "first_steps", "regular_player", "thunderstorm_game", "game_legend" };

    private void Awake()
    {
        var data = saveManager.GetData();
        currentXP = data.XP;
        level = data.XpLevel == 0 ? 1 : data.XpLevel;
        xpToNextLevel = data.XpToNextLevel == 0 ? 100 : data.XpToNextLevel;
        totalXP = data.TotalXP;
        Log();

        xpAwards.Add("AwardFor1Level", new XpAward { TypeOfAward = "Money", Award = 10, RequiredLevel = 1 });
        xpAwards.Add("AwardFor2Level", new XpAward { TypeOfAward = "Money", Award = 15, RequiredLevel = 2 });
        xpAwards.Add("AwardFor3Level", new XpAward { TypeOfAward = "Money", Award = 20, RequiredLevel = 3 });
        xpAwards.Add("AwardFor3LevelSkin", new XpAward { TypeOfAward = "Skin", SkinAward = "WoodSkin", GuiSkinName = "Дерево", RequiredLevel = 3 });
        xpAwards.Add("AwardFor4Level", new XpAward { TypeOfAward = "Money", Award = 40, RequiredLevel = 4 });
        xpAwards.Add("AwardFor5Level", new XpAward { TypeOfAward = "Money", Award = 50, RequiredLevel = 5 });
        xpAwards.Add("AwardFor5LevelSkin", new XpAward { TypeOfAward = "Skin", SkinAward = "IceSkin", GuiSkinName = "Лёд", RequiredLevel = 5 });
        xpAwards.Add("AwardFor6Level", new XpAward { TypeOfAward = "Money", Award = 70, RequiredLevel = 6 });
        xpAwards.Add("AwardFor7Level", new XpAward { TypeOfAward = "Money", Award = 100, RequiredLevel = 7 });
        xpAwards.Add("AwardFor8Level", new XpAward { TypeOfAward = "Money", Award = 150, RequiredLevel = 8 });
        xpAwards.Add("AwardFor9Level", new XpAward { TypeOfAward = "Money", Award = 170, RequiredLevel = 9 });
        xpAwards.Add("AwardFor10Level", new XpAward { TypeOfAward = "Money", Award = 200, RequiredLevel = 10 });
        xpAwards.Add("AwardFor10LevelSkin", new XpAward { TypeOfAward = "Skin", SkinAward = "FireSkin", GuiSkinName = "Огонь", RequiredLevel = 10 });
        
        for(int i = 11; i <= 30; i++)
        {
            xpAwards.Add($"AwardFor{i}Level", new XpAward { TypeOfAward = "Money", Award = i * 15, RequiredLevel = i });
        }
    }

    public void AddXp(int amount)
    {
        oldXp = currentXP;
        currentXP += amount;
        totalXP += amount;
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
        UpdateAchievements();

        foreach(var xpAward in xpAwards.Values)
        {
            if(xpAward.RequiredLevel == level)
            {
                switch(xpAward.TypeOfAward)
                {
                    case "Money":
                        PlayerPrefs.SetInt("HowMoneyAdds", PlayerPrefs.GetInt("HowMoneyAdds") + xpAward.Award);
                        break;
                    case "Skin":
                        achievementsHandler.UpdateProgress("large_wardrobe", 1);
                        PlayerPrefs.SetInt(xpAward.SkinAward, 1);
                        break;   
                }
            }
        }
        PlayerPrefs.Save();
        Save();
        Log();
    }

    public int GetXP() => currentXP;
    public int GetOldXP() => oldXp;
    public int GetXpToNextLevel() => xpToNextLevel;
    public int GetLevel() => level;
    public int GetTotalXP() => totalXP;
    public void SetXp(int amount) { currentXP = amount; Save(); Log(); }
    public void SetLevel(int amount) { level = amount; Save(); Log(); }
    public void SetTotalXp(int amount) { totalXP = amount; Save(); Log(); }
    public void SetXpToNextLevel(int amount) { xpToNextLevel = amount; Save(); Log(); }
    public float GetXPProgress() => (float)currentXP / xpToNextLevel;
    public float GetOldXPProgress() => (float)oldXp / xpToNextLevel;
    public float GetXPProgress(int _currentXP, int _xpToNextLevel) => (float)_currentXP / _xpToNextLevel;
    public float GetXPProgress(int _currentXP) => (float)_currentXP / xpToNextLevel;
    public void ResetOldXp() { oldXp = currentXP; }
    
    public XpAward GetMoneyAwardForNextLevel() => xpAwards[$"AwardFor{level + 1}Level"];
    public XpAward GetSkinAwardForNextLevel()
    {
        xpAwards.TryGetValue($"AwardFor{level + 1}LevelSkin", out var award);
        return award;
    }

    private void Save() => saveManager.SaveData();
    private void Log() { /* Debug.Log($"Current Xp: {currentXP} | Current level: {level}"); */ }

    private void UpdateAchievements()
    {
        if(achievementsHandler != null)
        {
            foreach(var key in AchievementsKeys)
                achievementsHandler.UpdateProgress(key, 1);
        }
    }
}