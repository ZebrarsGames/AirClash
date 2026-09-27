using System.Collections.Generic;

public static class QuestSaveSystem
{
    private static Dictionary<string, QuestData> _cachedSaves;

    private static Dictionary<string, QuestData> Saves
    {
        get
        {
            if(_cachedSaves == null)
            {
                _cachedSaves = GlobalSaveManager.Data.quests.ToDictionary();
            }
            return _cachedSaves;
        }
    }

    private static QuestData GetOrCreateQuest(string questId)
    {
        if (!Saves.TryGetValue(questId, out var questData))
        {
            questData = new QuestData();
            Saves[questId] = questData;
        }
        return questData;
    }

    public static void SetProgress(string questId, int progress)
    {
        QuestData quest = GetOrCreateQuest(questId);
        quest.progress = progress;
        SaveToGlobal();
    }

    public static void PlusProgress(string questId, int progress)
    {
        QuestData quest = GetOrCreateQuest(questId);
        quest.progress += progress;
        SaveToGlobal();
    }

    public static int GetProgress(string questId)
    {
        if(Saves.TryGetValue(questId, out var questData))
        {
            return questData.progress;
        }
        return 0;
    }

    public static bool GetIsCompleted(string questId)
    {
        if(Saves.TryGetValue(questId, out var questData))
        {
            return questData.isCompleted;
        }
        return false;
    }

    public static void SetCompleted(string questId)
    {
        QuestData quest = GetOrCreateQuest(questId);
        quest.isCompleted = true;
        SaveToGlobal();
    }

    public static void RemoveQuest(string questId)
    {
        if(Saves.ContainsKey(questId))
        {
            Saves.Remove(questId);
            SaveToGlobal();
        }
    }

    private static void SaveToGlobal()
    {
        GlobalSaveManager.Data.quests.FromDictionary(Saves);
        GlobalSaveManager.MarkAsDirty(); 
    }
}