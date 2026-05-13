using UnityEngine;
using UnityEngine.UI;

public class QuestItem : MonoBehaviour
{
    [Header("QuestInfo")]
    [SerializeField] private string questId;
    [Header("UI")]
    [SerializeField] private Text questNameText;
    [SerializeField] private Text questDescriptionText;
    [SerializeField] private Text targetText;
    [SerializeField] private GameObject completeArrow;
    [SerializeField] private Image questLogo;
    [Header("Scripts")]
    [SerializeField] private QuestsHandler questsHandler;

    void Start() 
    {
        questNameText.text = questsHandler.GetQuestName(questId);
        questDescriptionText.text = questsHandler.GetQuestDescription(questId);
    }
}
