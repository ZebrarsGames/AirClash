using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SaveManager : MonoBehaviour
{
    [SerializeField] private XpHandler xpHandler;
    [SerializeField] private MoneyHandler moneyHandler;
    public void SaveData()
    {
        Debug.Log("Data Saved!");
        PlayerData playerData = new PlayerData();
        playerData.Money = moneyHandler.GetMoney();
        playerData.XP = xpHandler.GetXP();
        playerData.XpLevel = xpHandler.GetLevel();
        playerData.XpToNextLevel = xpHandler.GetXpToNextLevel();
        playerData.Goals = PlayerPrefs.GetInt("TotalGoals", 0);
        playerData.NickName = PlayerPrefs.GetString("Nick", "Ник");
        playerData.AvatarPath = Path.Combine(Application.persistentDataPath, "avatar.png");
        playerData.CurrentSkinName = PlayerPrefs.GetString("CurrentSkin", "DefSkin");

        string json = JsonUtility.ToJson(playerData);
        File.WriteAllText(Application.persistentDataPath + "/save.json", json);
        PlayerPrefs.DeleteKey("Nick");
        PlayerPrefs.Save();
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
            Debug.Log("Файл сохранения успешно удален по пути: " + Application.persistentDataPath + "/save.json");
        }
        else
        {
            Debug.LogWarning("Файл сохранения не найден");
        }
    }
}
