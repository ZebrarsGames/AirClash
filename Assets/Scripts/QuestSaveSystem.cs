using UnityEngine;

public static class QuestSaveSystem
{
    private const string CompletedSuffix = "_completed";
    public static void SaveProgress(string questId, int progress)
    {
        PlayerPrefs.SetInt(questId, progress);
        PlayerPrefs.Save();
    }
    public static int GetProgress(string questId)
    {
        int progress = PlayerPrefs.GetInt(questId, 0);
        return progress;
    }
    public static bool GetIsCompleted(string questId)
    {
        bool isCompleted;
        if(PlayerPrefs.GetInt(questId + CompletedSuffix, 0) == 0)
        {
            isCompleted = false;
        } else
        {
            isCompleted = true;
        }
        return isCompleted;
    }
    public static void SetCompleted(string questId)
    {
        PlayerPrefs.SetInt(questId + CompletedSuffix, 1);
        PlayerPrefs.Save();
    }
}
