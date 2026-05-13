using UnityEngine;

public class QuestsHandler : MonoBehaviour
{
    [Header("Arrays")]
    [SerializeField] private QuestSO[] commonQuests;

    public void UpdateQuestProgress(string questName, int amount)
    {
        for(int i = 0; i < commonQuests.Length; i++)
        {
            if(commonQuests[i].QuestName.Equals(questName))
            {
                QuestSO currentQuest = commonQuests[i];
                QuestSaveSystem.PlusProgress(currentQuest.QuestId, amount);
                if(QuestSaveSystem.GetProgress(currentQuest.QuestId) >= currentQuest.Target)
                {
                    QuestSaveSystem.SetCompleted(currentQuest.QuestId);
                }
            }
        }
    }
}
