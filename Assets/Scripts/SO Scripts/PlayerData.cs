using System;
using UnityEngine;

[Serializable]
public class PlayerData : ScriptableObject
{
    public string NickName;
    public string AvatarPath;
    public int Money;
    public int XP;
    public int XpLevel;
    public int Goals;
    public SkinData CurrentSkin;
}
