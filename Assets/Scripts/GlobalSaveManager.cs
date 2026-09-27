using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#region Save Data Structures

[Serializable]
public class GlobalSaveData
{
    public PlayerData playerData = new PlayerData();
    public AchievementsSaveWrapper achievements = new AchievementsSaveWrapper();
    public QuestSaveWrapper quests = new QuestSaveWrapper();
}

[Serializable]
public class AchievementSaveData
{
    public string id;
    public int progress;
    public bool isUnlocked;
}

[Serializable]
public class AchievementsSaveWrapper
{
    public List<AchievementSaveData> list = new List<AchievementSaveData>();
}

[Serializable]
public class QuestData
{
    public int progress;
    public bool isCompleted;
}

[Serializable]
public class QuestSaveWrapper
{
    public List<string> keys = new List<string>();
    public List<QuestData> values = new List<QuestData>();

    public void FromDictionary(Dictionary<string, QuestData> dictionary)
    {
        keys.Clear();
        values.Clear();
        foreach (var kvp in dictionary)
        {
            keys.Add(kvp.Key);
            values.Add(kvp.Value);
        }
    }

    public Dictionary<string, QuestData> ToDictionary()
    {
        var dictionary = new Dictionary<string, QuestData>();
        for(int i = 0; i < keys.Count; i++)
        {
            dictionary[keys[i]] = values[i];
        }
        return dictionary;
    }
}
#endregion

public static class GlobalSaveManager
{
    private static readonly string SavePath = Path.Combine(Application.persistentDataPath, "global_save.json");
    
    private static GlobalSaveData _data;
    private static bool _isDirty = false;

    public static GlobalSaveData Data
    {
        get
        {
            if(_data == null) LoadFromDisk();
            return _data;
        }
    }

    private static void LoadFromDisk()
    {
        if(File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            _data = JsonUtility.FromJson<GlobalSaveData>(json) ?? new GlobalSaveData();
        }
        else
        {
            _data = new GlobalSaveData();
        }
    }

    public static void MarkAsDirty()
    {
        _isDirty = true;
    }

    public static void SaveToDisk()
    {
        if(!_isDirty || _data == null) return;

        string json = JsonUtility.ToJson(_data);
        File.WriteAllText(SavePath, json);
        _isDirty = false;
    }

    public static string GetCloudJson()
    {
        return JsonUtility.ToJson(Data);
    }

    public static void OverwriteFromCloud(string cloudJson)
    {
        if(string.IsNullOrEmpty(cloudJson)) return;
        
        _data = JsonUtility.FromJson<GlobalSaveData>(cloudJson) ?? new GlobalSaveData();
        MarkAsDirty();
        SaveToDisk();
    }
}