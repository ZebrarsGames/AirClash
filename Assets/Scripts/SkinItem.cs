using System;
using UnityEngine;
using UnityEngine.UI;

public class SkinItem : MonoBehaviour
{
    public string skinName;
    public int skinPrice;
    public ShopHandler shop;
    public bool isBuy;
    public Image checkmark;
    public Image equipArrow;
    [SerializeField] private SkinData skinData;
    [SerializeField] private Text skinNameText;
    [SerializeField] private Text priceText;
    [SerializeField] private Image skinImage;

    void Start()
    {
        skinName = skinData.skinName;
        skinPrice = skinData.price;
        priceText.text = skinPrice.ToString();
        skinNameText.text = skinData.skinGuiName.ToString();
        skinImage.sprite = skinData.sprite;
        switch(skinData.rarity)
        {
            case 1:
                if(ColorUtility.TryParseHtmlString("#FFFFFF", out Color DefColor))
                {
                    skinNameText.color = DefColor;
                }
                break;
            case 2:
                if(ColorUtility.TryParseHtmlString("#B9C24B", out Color RareColor))
                {
                    skinNameText.color = RareColor;
                }
                break;
            case 3:
                if(ColorUtility.TryParseHtmlString("#90E0EF", out Color SuperRareColor))
                {
                    skinNameText.color = SuperRareColor;
                }
                break;
            case 4:
                if(ColorUtility.TryParseHtmlString("#A99AD3", out Color EpicColor))
                {
                    skinNameText.color = EpicColor;
                }
                break;
            case 5:
                if(ColorUtility.TryParseHtmlString("#F94449", out Color MythicColor))
                {
                    skinNameText.color = MythicColor;
                }
                break;
            case 6:
                if(ColorUtility.TryParseHtmlString("#FFE747", out Color LegendaryColor))
                {
                    skinNameText.color = LegendaryColor;
                }
                break;    
            default:
                break;
        }
        if(PlayerPrefs.GetInt(skinName) == 1)
        {
            isBuy = true;
            checkmark.gameObject.SetActive(true);
        }
    }

    public void OnClickBuy() 
    {
        if(!isBuy)
        {
            isBuy = shop.BuySkin(skinName, skinPrice);      
            if(isBuy) 
            {
                checkmark.gameObject.SetActive(true);
                shop.EquipSkin(skinName);
            }
        } 
        else
        {
            shop.EquipSkin(skinName);
        }
    }  

}
