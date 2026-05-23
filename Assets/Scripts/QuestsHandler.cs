using UnityEngine;

public class QuestsHandler : MonoBehaviour
{
    [Header("Arrays")]
    [SerializeField] private QuestSO[] commonQuests;
    [Header("Scripts")]
    [SerializeField] private AchievementsHandler achievementsHandler;

    public void UpdateQuestProgress(string questId, int amount)
    {
        for(int i = 0; i < commonQuests.Length; i++)
        {
            if(commonQuests[i].QuestId.Equals(questId))
            {
                QuestSO currentQuest = commonQuests[i];
                if(QuestSaveSystem.GetIsCompleted(currentQuest.QuestId)) return;
                QuestSaveSystem.PlusProgress(currentQuest.QuestId, amount);
                if(amount > currentQuest.Target) 
                { 
                    QuestSaveSystem.SetProgress(currentQuest.QuestId, currentQuest.Target);
                }
                Debug.Log("Прогресс у " + questId +  " стал больше на " + amount);
                if(QuestSaveSystem.GetProgress(currentQuest.QuestId) >= currentQuest.Target)
                {
                    switch(currentQuest.QuestType)
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
                    QuestSaveSystem.SetCompleted(currentQuest.QuestId);
                    GiveAward(currentQuest.QuestId);
                }
            }
        }
    }
    public void GiveAward(string questId)
    {
        for(int i = 0; i < commonQuests.Length; i++)
        {
            if(commonQuests[i].QuestId.Equals(questId))
            {
                QuestSO currentQuest = commonQuests[i];
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
                    case AwardType.Skin:
                        achievementsHandler.UpdateProgress("large_wardrobe", 1);
                        PlayerPrefs.SetInt(currentQuest.SkinAward, 1);
                        PlayerPrefs.Save();
                        break;     
                }
                Debug.Log("Награда за " + questId + " выдана!");
            }
        }
    }
    public string GetQuestName(string questId)
    {
        for(int i = 0; i < commonQuests.Length; i++)
        {
            if(commonQuests[i].QuestId.Equals(questId))
            {
                return commonQuests[i].QuestName;
            }
        }
        return null;
    }
    public string GetQuestDescription(string questId)
    {
        for(int i = 0; i < commonQuests.Length; i++)
        {
            if(commonQuests[i].QuestId.Equals(questId))
            {
                return commonQuests[i].Description;
            }
        }
        return null;
    }
    public int GetQuestTarget(string questId)
    {
        for(int i = 0; i < commonQuests.Length; i++)
        {
            if(commonQuests[i].QuestId.Equals(questId))
            {
                return commonQuests[i].Target;
            }
        }
        return 0;
    }
    public int GetQuestProgress(string questId)
    {
        return QuestSaveSystem.GetProgress(questId);
    }
    public Sprite GetQuestIcon(string questId)
    {
        for(int i = 0; i < commonQuests.Length; i++)
        {
            if(commonQuests[i].QuestId.Equals(questId))
            {
                return commonQuests[i].QuestLogo;
            }
        }
        return null;
    }
}
