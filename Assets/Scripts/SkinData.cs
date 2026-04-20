using UnityEngine;

[CreateAssetMenu(menuName = "Skin")]
public class SkinData : ScriptableObject
{
    public string skinName;
    public string skinGuiName;
    public Sprite sprite;
    public GameObject particles;
    public AudioClip sound;
    public int rarity;
    public int price;
}