using UnityEngine;
using System.IO;
using System.Collections.Generic;

public static class SaveManager
{
    public static void SaveData()
    {
        Debug.Log("Data Saved!");
        PlayerData playerData = ScriptableObject.CreateInstance<PlayerData>();
        playerData.Money = PlayerPrefs.GetInt("Money", 0);
        playerData.XP = PlayerPrefs.GetInt("CurrentXp", 0);
        playerData.XpLevel = PlayerPrefs.GetInt("XpLevel", 1);
        playerData.Goals = PlayerPrefs.GetInt("TotalGoals", 0);
        playerData.NickName = "test";
        playerData.AvatarPath = "test";

        string json = JsonUtility.ToJson(playerData);
        File.WriteAllText(Application.persistentDataPath + "/save.json", json);
    }

    public static PlayerData GetData()
    {
        if (!File.Exists(Application.persistentDataPath + "/save.json"))
        {
            Debug.Log("No save file");
            return new PlayerData();
        }
        string json = File.ReadAllText(Application.persistentDataPath + "/save.json");
        PlayerData playerData = JsonUtility.FromJson<PlayerData>(json);
        return playerData;
    }

    public static void DeleteData()
    {
        if (File.Exists(Application.persistentDataPath + "/save.json"))
        {
            File.Delete(Application.persistentDataPath + "/save.json");
            Debug.Log("Файл сохранения успешно удален по пути: " + Application.persistentDataPath + "/save.json");
        }
        else
        {
            Debug.LogWarning("Файл сохранения не найден");
        }
    }
}
