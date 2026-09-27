using System.Collections.Generic;
using UnityEngine;

public class AchievementsHandler : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private AchievementSO[] allAchievements;

    [Header("Handlers")]
    [SerializeField] private AchievementPopup achievementPopup;

    private readonly Dictionary<string, AchievementState> achievementsMap = new Dictionary<string, AchievementState>();

    public class AchievementState
    {
        public AchievementSO Data;
        public AchievementSaveData SaveData;
    }

    private void Awake()
    {
        InitializeAndLoadAchievements();
    }

    private void InitializeAndLoadAchievements()
    {
        achievementsMap.Clear();
        if(allAchievements == null) return;

        var saveList = GlobalSaveManager.Data.achievements.list;

        for(int i = 0; i < allAchievements.Length; i++)
        {
            AchievementSO so = allAchievements[i];
            if(so == null || string.IsNullOrEmpty(so.Id)) continue;

            if(!achievementsMap.ContainsKey(so.Id))
            {
                AchievementSaveData saveData = saveList.Find(x => x.id == so.Id);

                if(saveData == null)
                {
                    saveData = new AchievementSaveData { id = so.Id, progress = 0, isUnlocked = false };
                    
                    if(PlayerPrefs.GetInt(so.Id + "_unlocked", 0) == 1)
                    {
                        saveData.isUnlocked = true;
                        saveData.progress = so.Target;
                        GlobalSaveManager.MarkAsDirty();
                    }
                    else if(PlayerPrefs.HasKey(so.Id))
                    {
                        saveData.progress = PlayerPrefs.GetInt(so.Id, 0);
                        GlobalSaveManager.MarkAsDirty();
                    }

                    saveList.Add(saveData);
                }

                achievementsMap.Add(so.Id, new AchievementState { Data = so, SaveData = saveData });
            }
        }
    }

    public void UpdateProgress(string id, int amount)
    {
        if(string.IsNullOrEmpty(id) || !achievementsMap.TryGetValue(id, out AchievementState state)) return;
        if(state.SaveData.isUnlocked) return;

        state.SaveData.progress += amount;
        GlobalSaveManager.MarkAsDirty();

        if(state.SaveData.progress >= state.Data.Target)
        {
            UnlockAchievement(state);
        }
    }

    public void SetProgress(string id, int amount)
    {
        if(string.IsNullOrEmpty(id) || !achievementsMap.TryGetValue(id, out AchievementState state)) return;
        if(state.SaveData.isUnlocked) return;

        state.SaveData.progress = amount;
        GlobalSaveManager.MarkAsDirty();

        if(state.SaveData.progress >= state.Data.Target)
        {
            UnlockAchievement(state);
        }
    }

    private void UnlockAchievement(AchievementState state)
    {
        state.SaveData.isUnlocked = true;
        state.SaveData.progress = state.Data.Target;
        GlobalSaveManager.MarkAsDirty();

        PlayerPrefs.SetInt("HowXpAdds", PlayerPrefs.GetInt("HowXpAdds", 0) + state.Data.Award);
        PlayerPrefs.Save();

        GlobalSaveManager.SaveToDisk(); 

        if(achievementPopup != null)
        {
            achievementPopup.ShowAchievement(state.Data.Title);
        }
    }

    public bool GetUnlocked(string id) => achievementsMap.TryGetValue(id, out var state) && state.SaveData.isUnlocked;
    public int GetTarget(string id) => achievementsMap.TryGetValue(id, out var state) ? state.Data.Target : 0;
    public int GetProgress(string id) => achievementsMap.TryGetValue(id, out var state) ? state.SaveData.progress : 0;
    public int GetCountOfAchievements() => allAchievements?.Length ?? 0;
    public string GetStringId(int index) => (allAchievements != null && index >= 0 && index < allAchievements.Length) ? allAchievements[index]?.Id : string.Empty;

}