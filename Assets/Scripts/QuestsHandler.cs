using UnityEngine;

public class QuestsHandler : MonoBehaviour
{
    [Header("Arrays")]
    [SerializeField] private QuestSO[] commonQuests;
    [Header("Scripts")]
    [SerializeField] private MoneyHandler moneyHandler;
    [SerializeField] private XpHandler xpHandler;

    public void UpdateQuestProgress(string questId, int amount)
    {
        for(int i = 0; i < commonQuests.Length; i++)
        {
            if(commonQuests[i].QuestName.Equals(questId))
            {
                QuestSO currentQuest = commonQuests[i];
                QuestSaveSystem.PlusProgress(currentQuest.QuestId, amount);
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
            if(commonQuests[i].QuestName.Equals(questId))
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
                        Debug.Log("Данная награда в разработке!");
                        break;     
                }
            }
        }
    }
}
