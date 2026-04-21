using UnityEngine;

[CreateAssetMenu(menuName = "Roulette Item")]
public class RouletteItemData : ScriptableObject
{
    public string typeOfAward;
    public int award;
    public string skinAward;
    public Sprite itemSprite;
}
