using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkinItem : MonoBehaviour
{
    [Header("Skin Data")]
    [SerializeField] private SkinData skinData;
    public string skinName;
    public string guiSkinName;
    public int skinPrice;

    [Header("Status")]
    public bool isBuy;
    public bool isCanBuy;
    public ShopHandler shop;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI skinNameText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private Image skinImage;

    [Header("Selection indicators")]
    public Image checkmark;
    public Image equipArrow;


    void Start()
    {
        skinName = skinData.skinName;
        skinPrice = skinData.price;
        guiSkinName = skinData.skinGuiName;
        skinNameText.text = guiSkinName;
        descText.text = skinData.skinDescripton;
        skinImage.sprite = skinData.sprite;
        switch(skinData.rarity)
        {
            case SkinRarity.Def:
                isCanBuy = true;
                if(ColorUtility.TryParseHtmlString("#FFFFFF", out Color DefColor))
                {
                    skinNameText.color = DefColor;
                }
                break;
            case SkinRarity.Rare:
                isCanBuy = true;
                if(ColorUtility.TryParseHtmlString("#B9C24B", out Color RareColor))
                {
                    skinNameText.color = RareColor;
                }
                break;
            case SkinRarity.SuperRare:
                isCanBuy = true;
                if(ColorUtility.TryParseHtmlString("#90E0EF", out Color SuperRareColor))
                {
                    skinNameText.color = SuperRareColor;
                }
                break;
            case SkinRarity.Epic:
                isCanBuy = true;
                if(ColorUtility.TryParseHtmlString("#A99AD3", out Color EpicColor))
                {
                    skinNameText.color = EpicColor;
                }
                break;
            case SkinRarity.Mythic:
                isCanBuy = true;
                if(ColorUtility.TryParseHtmlString("#F94449", out Color MythicColor))
                {
                    skinNameText.color = MythicColor;
                }
                break;
            case SkinRarity.Legendary:
                isCanBuy = true;
                if(ColorUtility.TryParseHtmlString("#FFE747", out Color LegendaryColor))
                {
                    skinNameText.color = LegendaryColor;
                }
                break;    
            case SkinRarity.Special:
                isCanBuy = false;
                if(ColorUtility.TryParseHtmlString("#0004ff", out Color XpColor))
                {
                    skinNameText.color = XpColor;
                }
                break;   
            default:
                break;
        }
        if(PlayerPrefs.GetInt(skinName, 0) == 1)
        {
            isBuy = true;
            checkmark.gameObject.SetActive(true);
        }
        string skins = PlayerPrefs.GetString("AllBuySkins", "DefSkin");
        string[] parts = skins.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        bool isHasThisSkin = false;
        for(int i = 0; i < parts.Length; i ++) if(parts[i] == skinName) isHasThisSkin = true;
        if(PlayerPrefs.GetInt(skinName, 0) == 1 && !isHasThisSkin)
        {
            skins += "," + skinName;
            PlayerPrefs.SetString("AllBuySkins", skins);
            PlayerPrefs.Save();
        }
        if(isCanBuy)
        {
            priceText.text = $"{skinPrice} <sprite=0>";
        }
    }

    public void OnClickBuy() 
    {
        if(!isBuy && isCanBuy)
        {
            isBuy = shop.BuySkin(skinName, skinPrice);      
            if(isBuy) 
            {
                checkmark.gameObject.SetActive(true);
                shop.EquipSkin(skinName);
            }
        } 
        else if(isBuy)
        {
            shop.EquipSkin(skinName);
        } else if(!isCanBuy)
        {
            shop.PlayCancelSound();
        }
    }  

}
