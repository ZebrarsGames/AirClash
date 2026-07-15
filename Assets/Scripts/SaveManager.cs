using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SaveManager : MonoBehaviour
{
    [SerializeField] private XpHandler xpHandler;
    [SerializeField] private MoneyHandler moneyHandler;

    public void SaveData()
    {
        PlayerData playerData = new PlayerData();
        playerData.Money = moneyHandler.GetMoney();
        playerData.XP = xpHandler.GetXP();
        playerData.XpLevel = xpHandler.GetLevel();
        playerData.XpToNextLevel = xpHandler.GetXpToNextLevel();
        playerData.Goals = PlayerPrefs.GetInt("TotalGoals", 0);
        playerData.NickName = PlayerPrefs.GetString("Nick", GetData().NickName);
        playerData.AvatarPath = Path.Combine(Application.persistentDataPath, "avatar.png");
        playerData.CurrentSkinName = PlayerPrefs.GetString("CurrentSkin", "DefSkin");
        playerData.Playtime = PlaytimeTracker.Instance.GetSecondsPlaytime();
        playerData.TotalMoney = moneyHandler.GetTotalMoney();
        playerData.TotalXP = xpHandler.GetTotalXP();

        string json = JsonUtility.ToJson(playerData);
        File.WriteAllText(Application.persistentDataPath + "/save.json", json);
        PlayerPrefs.DeleteKey("Nick");
        PlayerPrefs.Save();
        Debug.Log("Data Saved!");
    }

    public PlayerData GetData()
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

    public void DeleteData()
    {
        if (File.Exists(Application.persistentDataPath + "/save.json"))
        {
            File.Delete(Application.persistentDataPath + "/save.json");
            File.Delete(Path.Combine(Application.persistentDataPath, "avatar.png"));
            Debug.Log("Файл сохранения успешно удален по пути: " + Application.persistentDataPath + "/save.json");
        }
        else
        {
            Debug.LogWarning("Файл сохранения не найден");
        }
    }

    public void SaveDefaultData()
    {
        PlayerData playerData = new PlayerData();
        playerData.Money = PlayerPrefs.GetInt("Money", 0);
        playerData.XP = PlayerPrefs.GetInt("CurrentXp", 0);
        playerData.XpLevel = PlayerPrefs.GetInt("XpLevel", 1);;
        playerData.XpToNextLevel = PlayerPrefs.GetInt("XpToNextLevel", 100);;
        playerData.Goals = 0;
        playerData.NickName = "Ник";
        playerData.AvatarPath = Path.Combine(Application.persistentDataPath, "avatar.png");
        playerData.CurrentSkinName = PlayerPrefs.GetString("CurrentSkin", "DefSkin");
        playerData.Playtime = 0;
        playerData.TotalMoney = PlayerPrefs.GetInt("Money", 0);;
        playerData.TotalXP = PlayerPrefs.GetInt("CurrentXp", 0);;

        string json = JsonUtility.ToJson(playerData);
        File.WriteAllText(Application.persistentDataPath + "/save.json", json);
        Debug.Log("Default data Saved!");
    }
}
