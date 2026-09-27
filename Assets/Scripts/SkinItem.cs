using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkinItem : MonoBehaviour
{
    [Header("Skin Data")]
    [SerializeField] private SkinData skinData;

    [Header("Status")]
    public string skinName;
    public string guiSkinName;
    public int skinPrice;
    public bool isBuy;
    public bool isCanBuy = true;
    public ShopHandler shop;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI skinNameText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private Image skinImage;

    [Header("Selection Indicators")]
    public Image checkmark;
    public Image equipArrow;

    private void Start()
    {
        InitializeSkin();
    }

    public void InitializeSkin()
    {
        if(skinData == null)
        {
            Debug.LogError($"[SkinItem] SkinData не назначен на {gameObject.name}!");
            return;
        }

        skinName = skinData.skinName;
        guiSkinName = skinData.skinGuiName;
        skinPrice = skinData.price;

        if(skinNameText != null)
        {
            skinNameText.text = guiSkinName;
            skinNameText.color = GetRarityColor(skinData.rarity);
        }

        if(descText != null) descText.text = skinData.skinDescription;
        if(skinImage != null) skinImage.sprite = skinData.sprite;

        isCanBuy = skinData.rarity != SkinRarity.Special;

        isBuy = shop != null && shop.IsSkinBought(skinName);

        if(checkmark != null)
        {
            checkmark.gameObject.SetActive(isBuy);
        }
    }

    public void OnClickBuy() 
    {
        if(shop == null)
        {
            Debug.LogError($"[SkinItem] ShopHandler не привязан к {gameObject.name}!");
            return;
        }

        if(!isBuy && isCanBuy)
        {
            isBuy = shop.BuySkin(skinName, skinPrice);      
            if(isBuy) 
            {
                if(checkmark != null) checkmark.gameObject.SetActive(true);
                shop.EquipSkin(skinName);
            }
        } 
        else if(isBuy)
        {
            shop.EquipSkin(skinName);
        } 
        else
        {
            shop.PlayCancelSound();
        }
    }  

    private Color GetRarityColor(SkinRarity rarity)
    {
        return rarity switch
        {
            SkinRarity.Def => Color.white,
            SkinRarity.Rare => ParseColor("#B9C24B"),
            SkinRarity.SuperRare => ParseColor("#90E0EF"),
            SkinRarity.Epic => ParseColor("#A99AD3"),
            SkinRarity.Mythic => ParseColor("#F94449"),
            SkinRarity.Legendary => ParseColor("#FFE747"),
            SkinRarity.Special => ParseColor("#0004FF"),
            _ => Color.white
        };
    }

    private Color ParseColor(string hex)
    {
        return ColorUtility.TryParseHtmlString(hex, out Color color) ? color : Color.white;
    }
}