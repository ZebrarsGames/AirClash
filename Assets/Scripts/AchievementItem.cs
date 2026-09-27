using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementItem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AchievementsHandler achievementsHandler;
    [SerializeField] private AchievementSO achievementData;

    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject unlockedCheckMark;

    private static readonly string TargetFormat = "{0}/{1}";

    private void Start()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        if(achievementsHandler == null || achievementData == null) return;

        string id = achievementData.Id;
        bool isUnlocked = achievementsHandler.GetUnlocked(id);
        int progress = achievementsHandler.GetProgress(id);
        int target = achievementData.Target;

        if(titleText != null) titleText.SetText(achievementData.Title);
        if(iconImage != null && achievementData.Icon != null) iconImage.sprite = achievementData.Icon;

        int clampedProgress = progress > target ? target : progress;
        if(progressText != null) progressText.SetText(TargetFormat, clampedProgress, target);

        if(unlockedCheckMark != null) unlockedCheckMark.SetActive(isUnlocked);
    }

    public void SetAchievementData(AchievementSO data, AchievementsHandler handler)
    {
        achievementData = data;
        achievementsHandler = handler;
        UpdateUI();
    }
}