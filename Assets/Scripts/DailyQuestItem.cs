using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DailyQuestItem : MonoBehaviour
{
    [Header("Quest Info")]
    [SerializeField] private string questId;
    [SerializeField] private DailyQuestHandler dailyQuestHandler;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI questNameText;
    [SerializeField] private TextMeshProUGUI questDescriptionText;
    [SerializeField] private TextMeshProUGUI targetText;
    [SerializeField] private GameObject completeArrow;
    [SerializeField] private Image questLogo;

    private static readonly string TargetFormat = "{0}/{1}";

    private void Start()
    {
        StartSetQuestInfo();
    }

    private void SetQuestInfo(int i, DailyQuestSO[] todayPool)
    {
        if(todayPool == null || i < 0 || i >= todayPool.Length || todayPool[i] == null) return;

        DailyQuestSO quest = todayPool[i];
        questId = quest.QuestId;

        if(questNameText != null) questNameText.SetText(quest.QuestName);
        if(questDescriptionText != null) questDescriptionText.SetText(quest.Description);
        if(questLogo != null) questLogo.sprite = quest.QuestLogo;

        int currentProgress = QuestSaveSystem.GetProgress(questId);
        int target = quest.Target;
        int clampedProgress = currentProgress > target ? target : currentProgress;

        if(targetText != null) targetText.SetText(TargetFormat, clampedProgress, target);

        bool isCompleted = QuestSaveSystem.GetIsCompleted(questId);
        if(completeArrow != null) completeArrow.SetActive(isCompleted);
    }

    public void StartSetQuestInfo()
    {
        if(dailyQuestHandler == null) return;

        DailyQuestSO[] todayPool = dailyQuestHandler.GetTodayPool();
        int currentUpdateIndex = PlayerPrefs.GetInt("CurrentUIUpdate", 0);

        SetQuestInfo(currentUpdateIndex, todayPool);

        int nextIndex = (currentUpdateIndex + 1) % 3;
        PlayerPrefs.SetInt("CurrentUIUpdate", nextIndex);
        PlayerPrefs.Save();
    }
}