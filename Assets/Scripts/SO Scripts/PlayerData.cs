using System;
using UnityEngine;

[Serializable]
public class PlayerData : ScriptableObject
{
    public int Money;
    public int XP;
    public int XpLevel;
    public int Goals;
    public string[] UnlockedSkinsIds; //coming soon
    public string[] UnlockedAchievementsIds; //coming soon
}
