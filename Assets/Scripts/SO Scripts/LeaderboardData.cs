using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LeaderboardItem
{
    public string nick;
    public int elo;
    public Sprite avatar;
    public string createdAt;
}