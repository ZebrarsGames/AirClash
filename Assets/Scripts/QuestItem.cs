using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestItem : MonoBehaviour
{
    [Header("Quest Info")]
    [SerializeField] private string questId;
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI questNameText;
    [SerializeField] private TextMeshProUGUI questDescriptionText;
    [SerializeField] private TextMeshProUGUI targetText;
    [SerializeField] private GameObject completeArrow;
    [SerializeField] private Image questLogo;
    [Header("Scripts")]
    [SerializeField] private QuestsHandler questsHandler;

    void Start() 
    {
        questNameText.text = questsHandler.GetQuestName(questId);
        questDescriptionText.text = questsHandler.GetQuestDescription(questId);
        if(questsHandler.GetQuestProgress(questId) > questsHandler.GetQuestTarget(questId)) targetText.text = questsHandler.GetQuestTarget(questId) + "/" + questsHandler.GetQuestTarget(questId);
        else targetText.text = questsHandler.GetQuestProgress(questId) + "/" + questsHandler.GetQuestTarget(questId);
        questLogo.sprite = questsHandler.GetQuestIcon(questId);
        completeArrow.SetActive(QuestSaveSystem.GetIsCompleted(questId));
    }
}
