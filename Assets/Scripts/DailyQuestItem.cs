using UnityEngine;
using UnityEngine.UI;

public class DailyQuestItem : MonoBehaviour
{
    [Header("Quest Info")]
    [SerializeField] private string questId;
    [Header("UI")]
    [SerializeField] private Text questNameText;
    [SerializeField] private Text questDescriptionText;
    [SerializeField] private Text targetText;
    [SerializeField] private GameObject completeArrow;
    [SerializeField] private Image questLogo;
    public void SetData(DailyQuestSO quest)
    {
        questNameText.text = quest.QuestName;
        questDescriptionText.text = quest.Description;
        questLogo.sprite = quest.QuestLogo;
        questId = quest.QuestId;
        targetText.text = QuestSaveSystem.GetProgress(questId) + "/" + quest.Target;
    }
}
