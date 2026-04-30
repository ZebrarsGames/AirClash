using UnityEngine;

public class XpHandler : MonoBehaviour
{
    private int currentXP = 0;
    private int xpToNextLevel = 100;
    private int level = 1;
    public int GetXP() => currentXP;
    public int GetXpToNextLevel() => xpToNextLevel;
    public int GetLevel() => level;

    void Start()
    {
        currentXP = PlayerPrefs.GetInt("CurrentXp", 0);
        level = PlayerPrefs.GetInt("XpLevel", 1);
        xpToNextLevel = PlayerPrefs.GetInt("XpToNextLevel", 100);
        Log();
    }

    public void AddXp(int amount)
    {
        currentXP += amount;
        if(currentXP >= xpToNextLevel)
        {
            currentXP -= xpToNextLevel;
            LevelUp();
        }
        Save();
        Log();
    }

    private void LevelUp()
    {
        level++;
        xpToNextLevel += 50;
        Save();
        Log();
    }

    public float GetXPProgress()
    {
        return (float)currentXP / xpToNextLevel;
    }

    private void Save()
    {
        PlayerPrefs.SetInt("CurrentXp", currentXP);
        PlayerPrefs.SetInt("XpLevel", level);
        PlayerPrefs.SetInt("XpToNextLevel", xpToNextLevel);
        PlayerPrefs.Save();
    }

    private void Log()
    {
        Debug.Log("Current Xp: " + currentXP);
        Debug.Log("Current level: " + level);
    }
}
