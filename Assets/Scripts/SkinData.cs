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

[CreateAssetMenu(fileName = "NewSkinData", menuName = "Skin")]
public class SkinData : ScriptableObject
{
    [Header("Main Settings")]
    public string skinName;
    public string skinGuiName;
    
    [TextArea(3, 5)]
    public string skinDescription;
    
    public Sprite sprite;
    public SkinRarity rarity;
    public int price;

    [Header("Visual & Audio Effects")]
    public GameObject particles;
    public GameObject trail;
    public AudioClip sound;
}