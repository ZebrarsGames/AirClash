using UnityEngine;

public class QuestsHandler : MonoBehaviour
{
    [Header("Arrays")]
    [SerializeField] private QuestSO[] commonQuests;
    [Header("Scripts")]
    [SerializeField] private MoneyHandler moneyHandler;
    [SerializeField] private XpHandler xpHandler;
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
                        moneyHandler.AddMoney(currentQuest.Award);
                        break;
                    case AwardType.Xp:
                        xpHandler.AddXp(currentQuest.Award);
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
