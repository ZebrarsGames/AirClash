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

        string json = JsonUtility.ToJson(playerData);
        File.WriteAllText(Application.persistentDataPath + "/save.json", json);
    }
}
