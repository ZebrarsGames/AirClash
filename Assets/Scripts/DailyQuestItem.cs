using UnityEngine;
using UnityEngine.UI;

public class DailyQuestItem : MonoBehaviour
{
    [Header("Quest Info")]
    [SerializeField] private string questId;
    [SerializeField] private DailyQuestHandler dailyQuestHandler;
    [Header("UI")]
    [SerializeField] private Text questNameText;
    [SerializeField] private Text questDescriptionText;
    [SerializeField] private Text targetText;
    [SerializeField] private GameObject completeArrow;
    [SerializeField] private Image questLogo;
    void Start()
    {
        DailyQuestSO[] todayPool = dailyQuestHandler.GetTodayPool();
        switch(PlayerPrefs.GetInt("CurrentUIUpdate", 0))
        {
            case 0:
                SetQuestInfo(0, todayPool);
                PlayerPrefs.SetInt("CurrentUIUpdate", 1);
                PlayerPrefs.Save();
                break;
            case 1:
                SetQuestInfo(1, todayPool);
                PlayerPrefs.SetInt("CurrentUIUpdate", 2);
                PlayerPrefs.Save();
                break;
            case 2:
                SetQuestInfo(2, todayPool);
                PlayerPrefs.SetInt("CurrentUIUpdate", 0);
                PlayerPrefs.Save();
                break;
        }
    }
    private void SetQuestInfo(int i, DailyQuestSO[] todayPool)
    {
        questNameText.text = todayPool[i].QuestName;
        questDescriptionText.text = todayPool[i].Description;
        questId = todayPool[i].QuestId;
        if(QuestSaveSystem.GetProgress(questId) > todayPool[i].Target) targetText.text = todayPool[i].Target + "/" + todayPool[i].Target;
        else targetText.text = QuestSaveSystem.GetProgress(questId) + "/" + todayPool[i].Target;
        questLogo.sprite = todayPool[i].QuestLogo;
        if(QuestSaveSystem.GetIsCompleted(questId)) completeArrow.SetActive(true);
        else completeArrow.SetActive(false);
    }
}
