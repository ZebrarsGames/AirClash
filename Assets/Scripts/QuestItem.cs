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
        if(questsHandler == null) return;

        questNameText.text = questsHandler.GetQuestName(questId);
        questDescriptionText.text = questsHandler.GetQuestDescription(questId);
        
        int progress = questsHandler.GetQuestProgress(questId);
        int target = questsHandler.GetQuestTarget(questId);
        int currentProgress = Mathf.Min(progress, target);

        targetText.SetText("{0}/{1}", currentProgress, target);
        
        questLogo.sprite = questsHandler.GetQuestIcon(questId);
        completeArrow.SetActive(QuestSaveSystem.GetIsCompleted(questId));
    }
}