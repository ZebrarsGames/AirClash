using UnityEngine;

public enum SkinRarity
{
    Def,
    Rare,
    SuperRare,
    Epic,
    Mythic,
    Legendary,
    Special
}

[CreateAssetMenu(menuName = "Skin")]
public class SkinData : ScriptableObject
{
    public string skinName;
    public string skinGuiName;
    [TextArea]
    public string skinDescripton;
    public Sprite sprite;
    public GameObject particles;
    public GameObject trail;
    public AudioClip sound;
    public SkinRarity rarity;
    public int price;
}