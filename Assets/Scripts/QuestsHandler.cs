using UnityEngine;
using System.Collections.Generic;

public class QuestsHandler : MonoBehaviour
{
    [Header("Arrays")]
    [SerializeField] private QuestSO[] commonQuests;
    [Header("Scripts")]
    [SerializeField] private AchievementsHandler achievementsHandler;

    private Dictionary<string, QuestSO> questDict;

    private void Awake()
    {
        questDict = new Dictionary<string, QuestSO>(commonQuests.Length);
        foreach(var quest in commonQuests)
        {
            questDict[quest.QuestId] = quest;
        }
    }

    public void UpdateQuestProgress(string questId, int amount)
    {
        if(!questDict.TryGetValue(questId, out var quest)) return;
        if(QuestSaveSystem.GetIsCompleted(questId)) return;

        int newProgress = QuestSaveSystem.GetProgress(questId) + amount;
        ProcessProgress(quest, newProgress);
    }

    public void SetQuestProgress(string questId, int amount)
    {
        if(!questDict.TryGetValue(questId, out var quest)) return;
        if(QuestSaveSystem.GetIsCompleted(questId)) return;

        ProcessProgress(quest, amount);
    }

    private void ProcessProgress(QuestSO quest, int progressAmount)
    {
        if(progressAmount > quest.Target) progressAmount = quest.Target;
        
        QuestSaveSystem.SetProgress(quest.QuestId, progressAmount);
        
        if(progressAmount >= quest.Target)
        {
            QuestSaveSystem.SetCompleted(quest.QuestId);
            GiveAchievement(quest.QuestType);
            GiveAward(quest.QuestId);
        }
    }

    private void GiveAchievement(QuestType type)
    {
        switch(type)
        {
            case QuestType.Money:
                achievementsHandler.UpdateProgress("coin_master", 1);
                break;
            case QuestType.Xp:
                achievementsHandler.UpdateProgress("master_xp", 1);
                break;   
            case QuestType.Goals:
                achievementsHandler.UpdateProgress("master_of_goals", 1);
                break;
        }
    }

    public void GiveAward(string questId)
    {
        if(!questDict.TryGetValue(questId, out var quest)) return;

        switch(quest.AwardType)
        {
            case AwardType.Money:
                PlayerPrefs.SetInt("HowMoneyAdds", PlayerPrefs.GetInt("HowMoneyAdds") + quest.Award);
                break;
            case AwardType.Xp:
                PlayerPrefs.SetInt("HowXpAdds", PlayerPrefs.GetInt("HowXpAdds") + quest.Award);
                break;   
            case AwardType.Skin:
                achievementsHandler.UpdateProgress("large_wardrobe", 1);
                PlayerPrefs.SetInt(quest.SkinAward, 1);
                break;     
        }
        PlayerPrefs.Save();
    }

    public string GetQuestName(string questId) => questDict.TryGetValue(questId, out var q) ? q.QuestName : null;
    public string GetQuestDescription(string questId) => questDict.TryGetValue(questId, out var q) ? q.Description : null;
    public int GetQuestTarget(string questId) => questDict.TryGetValue(questId, out var q) ? q.Target : 0;
    public Sprite GetQuestIcon(string questId) => questDict.TryGetValue(questId, out var q) ? q.QuestLogo : null;
    public int GetQuestProgress(string questId) => QuestSaveSystem.GetProgress(questId);
}