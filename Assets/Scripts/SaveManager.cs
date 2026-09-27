using UnityEngine;
using System.IO;
using System;

public class SaveManager : MonoBehaviour
{
    [SerializeField] private XpHandler xpHandler;
    [SerializeField] private MoneyHandler moneyHandler;

    public void SaveData()
    {
        PlayerData playerData = GlobalSaveManager.Data.playerData;

        playerData.Money = moneyHandler.GetMoney();
        playerData.XP = xpHandler.GetXP();
        playerData.XpLevel = xpHandler.GetLevel();
        playerData.XpToNextLevel = xpHandler.GetXpToNextLevel();
        playerData.Goals = PlayerPrefs.GetInt("TotalGoals", 0);
        playerData.NickName = PlayerPrefs.GetString("Nick", playerData.NickName);
        playerData.CurrentSkinName = PlayerPrefs.GetString("CurrentSkin", "DefSkin");
        playerData.Playtime = PlaytimeTracker.Instance.GetSecondsPlaytime();
        playerData.TotalMoney = moneyHandler.GetTotalMoney();
        playerData.TotalXP = xpHandler.GetTotalXP();

        string skins = PlayerPrefs.GetString("AllBuySkins", "DefSkin");
        string[] parts = skins.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        playerData.AllBuySkins = parts;

        GlobalSaveManager.MarkAsDirty();
        GlobalSaveManager.SaveToDisk();
        
        PlayerPrefs.Save();
        Debug.Log("[SaveManager] Данные игрока обновлены в глобальном сохранении!");
    }

    public PlayerData GetData()
    {
        return GlobalSaveManager.Data.playerData;
    }

    public void DeleteData()
    {
        string globalSavePath = Path.Combine(Application.persistentDataPath, "global_save.json");
        
        if(File.Exists(globalSavePath))
        {
            File.Delete(globalSavePath);
        }
        
        string avatarPath = Path.Combine(Application.persistentDataPath, "avatar.png");
        if(File.Exists(avatarPath))
        {
            File.Delete(avatarPath);
        }

        GlobalSaveManager.OverwriteFromCloud("{}"); 
        
        Debug.Log("[SaveManager] Файл глобального сохранения и аватар успешно удалены!");
    }

    public void SaveDefaultData()
    {
        PlayerData playerData = GlobalSaveManager.Data.playerData;
        
        playerData.Money = PlayerPrefs.GetInt("Money", 0);
        playerData.XP = PlayerPrefs.GetInt("CurrentXp", 0);
        playerData.XpLevel = PlayerPrefs.GetInt("XpLevel", 1);
        playerData.XpToNextLevel = PlayerPrefs.GetInt("XpToNextLevel", 100);
        playerData.Goals = 0;
        playerData.NickName = "Ник";
        playerData.CurrentSkinName = PlayerPrefs.GetString("CurrentSkin", "DefSkin");
        playerData.Playtime = 0;
        playerData.TotalMoney = PlayerPrefs.GetInt("Money", 0);
        playerData.TotalXP = PlayerPrefs.GetInt("CurrentXp", 0);

        string skins = "DefSkin";
        string[] parts = skins.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        playerData.AllBuySkins = parts;

        GlobalSaveManager.MarkAsDirty();
        GlobalSaveManager.SaveToDisk();
        
        PlayerPrefs.Save();
        Debug.Log("[SaveManager] Дефолтные данные сохранены в глобальный файл!");
    }

    public PlayerData GetDefaultData()
    {
        PlayerData playerData = new PlayerData();
        playerData.Money = PlayerPrefs.GetInt("Money", 0);
        playerData.XP = PlayerPrefs.GetInt("CurrentXp", 0);
        playerData.XpLevel = PlayerPrefs.GetInt("XpLevel", 1);
        playerData.XpToNextLevel = PlayerPrefs.GetInt("XpToNextLevel", 100);
        playerData.Goals = 0;
        playerData.NickName = "Ник";
        playerData.CurrentSkinName = PlayerPrefs.GetString("CurrentSkin", "DefSkin");
        playerData.Playtime = 0;
        playerData.TotalMoney = PlayerPrefs.GetInt("Money", 0);
        playerData.TotalXP = PlayerPrefs.GetInt("CurrentXp", 0);

        string skins = "DefSkin";
        string[] parts = skins.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        playerData.AllBuySkins = parts;

        return playerData;
    }
}