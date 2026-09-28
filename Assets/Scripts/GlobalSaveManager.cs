using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

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

public static class GlobalSaveManager
{
    private static readonly string SavePath = Path.Combine(Application.persistentDataPath, "global_save.json");
    private static readonly string AvatarPath = Path.Combine(Application.persistentDataPath, "avatar.png");
    
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

        _data.playerData.avatarBase64 = string.Empty;

        string json = JsonUtility.ToJson(_data);
        File.WriteAllText(SavePath, json);
        _isDirty = false;
    }

    public static string GetCloudJson()
    {
        if(File.Exists(AvatarPath))
        {
            try
            {
                byte[] fileData = File.ReadAllBytes(AvatarPath);
                Texture2D tex = new Texture2D(2, 2);
                if(tex.LoadImage(fileData)) 
                {
                    byte[] compressedData = tex.EncodeToJPG(10); 
                    Data.playerData.avatarBase64 = Convert.ToBase64String(compressedData);
                }
                onetimeTextureCleanup(tex);
            }
            catch(Exception e)
            {
                Debug.LogError($"[GlobalSaveManager] Ошибка сжатия аватарки: {e.Message}");
                Data.playerData.avatarBase64 = string.Empty;
            }
        }
        else
        {
            Data.playerData.avatarBase64 = string.Empty;
        }

        string cloudJson = JsonUtility.ToJson(Data);
        Data.playerData.avatarBase64 = string.Empty; 

        return cloudJson;
    }

    public static void OverwriteFromCloud(string cloudJson)
    {
        if(string.IsNullOrEmpty(cloudJson)) return;
        
        _data = JsonUtility.FromJson<GlobalSaveData>(cloudJson) ?? new GlobalSaveData();

        if(!string.IsNullOrEmpty(_data.playerData.avatarBase64))
        {
            try
            {
                byte[] avatarBytes = Convert.FromBase64String(_data.playerData.avatarBase64);
                File.WriteAllBytes(AvatarPath, avatarBytes);
                Debug.Log($"[GlobalSaveManager] Аватарка успешно сохранена по пути: {AvatarPath}. Размер: {avatarBytes.Length} байт.");
            }
            catch(Exception e)
            {
                Debug.LogError($"[GlobalSaveManager] Ошибка восстановления аватарки: {e.Message}");
            }
        }

        MarkAsDirty();
        SaveToDisk();
    }

    private static void onetimeTextureCleanup(Texture2D tex)
    {
        if(tex != null)
        {
            if (Application.isPlaying) UnityEngine.Object.Destroy(tex);
            else UnityEngine.Object.DestroyImmediate(tex);
        }
    }
}