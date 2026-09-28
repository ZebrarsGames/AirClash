using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;

public class ShopHandler : MonoBehaviour
{
    private const string BOUGHT_SKINS_KEY = "AllBuySkins";
    private const string DEFAULT_SKIN = "DefSkin";
    private const string CURRENT_SKIN_KEY = "CurrentSkin";

    [Header("Economy and Progress")]
    [SerializeField] private MoneyHandler moneyHandler;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private AchievementsHandler achievementsHandler;

    [Header("Shop and Skins")]
    [SerializeField] private SkinItem[] allSkins;
    [SerializeField] private GameObject surePanel;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource audioSourceBgMusic;
    [SerializeField] private AudioClip shopMusic;
    [SerializeField] private AudioClip buySound;
    [SerializeField] private AudioClip cancelSound;
    [SerializeField] private AudioClip warningSound;

    [Header("Other")]
    [SerializeField] private SaveManager saveManager;

    private readonly HashSet<string> _boughtSkins = new HashSet<string>();

    private void Awake()
    {
        LoadShopData();
    }

    void Start()
    {
        if(moneyText != null && moneyHandler != null)
        {
            moneyText.SetText($"{moneyHandler.GetMoney()} <sprite=0>");
        }

        if(audioSourceBgMusic != null && shopMusic != null)
        {
            audioSourceBgMusic.clip = shopMusic;
            audioSourceBgMusic.loop = true;
            audioSourceBgMusic.time = PlayerPrefs.GetFloat("ShopMusicTime", 0);
            audioSourceBgMusic.Play();
        }

        string currentSkin = PlayerPrefs.GetString(CURRENT_SKIN_KEY, DEFAULT_SKIN);
        EquipSkin(currentSkin);
    }

    private void LoadShopData()
    {
        _boughtSkins.Clear();

        string savedSkins = PlayerPrefs.GetString(BOUGHT_SKINS_KEY, DEFAULT_SKIN);
        string[] parts = savedSkins.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

        foreach(string skin in parts)
        {
            _boughtSkins.Add(skin.Trim());
        }

        _boughtSkins.Add(DEFAULT_SKIN);
    }

    public bool IsSkinBought(string skinName)
    {
        if(string.IsNullOrEmpty(skinName)) return false;
        return _boughtSkins.Contains(skinName) || PlayerPrefs.GetInt(skinName, 0) == 1;
    }

    public bool BuySkin(string skinName, int skinCost)
    {
        if(moneyHandler.GetMoney() >= skinCost && !IsSkinBought(skinName))
        {
            if(achievementsHandler != null)
            {
                achievementsHandler.UpdateProgress("large_wardrobe", 1);
            }

            if(audioSource != null && buySound != null)
            {
                audioSource.PlayOneShot(buySound);
            }

            moneyHandler.RemoveMoney(skinCost);

            PlayerPrefs.SetString(CURRENT_SKIN_KEY, skinName);
            PlayerPrefs.SetInt(skinName, 1);

            _boughtSkins.Add(skinName);
            SaveBoughtSkins();

            if(moneyText != null)
            {
                moneyText.SetText($"{moneyHandler.GetMoney()} <sprite=0>");
            }

            return true;
        }
        else
        {
            PlayCancelSound();
            return false;
        }
    }

    public void EquipSkin(string skinName)
    {
        PlayerPrefs.SetString(CURRENT_SKIN_KEY, skinName);
        PlayerPrefs.Save();

        if(allSkins != null)
        {
            foreach(var skin in allSkins)
            {
                if(skin != null && skin.equipArrow != null)
                {
                    skin.equipArrow.gameObject.SetActive(skin.skinName == skinName);
                }
            }
        }
    }

    public void CloseShop()
    {
        if(audioSourceBgMusic != null)
        {
            PlayerPrefs.SetFloat("ShopMusicTime", audioSourceBgMusic.time);
        }
        PlayerPrefs.Save();
        SceneManager.LoadScene("MainMenu");
    }

    public void PlayCancelSound()
    {
        if(audioSource != null && cancelSound != null)
        {
            audioSource.PlayOneShot(cancelSound);
        }
    }

    public void RemoveAllMoney()
    {
        if(moneyHandler != null)
        {
            moneyHandler.RemoveMoney(moneyHandler.GetMoney());
            if(moneyText != null)
            {
                moneyText.SetText($"{moneyHandler.GetMoney()} <sprite=0>");
            }
        }
    }

    public void PlusMoney(int money)
    {
        if(moneyHandler != null)
        {
            moneyHandler.AddMoney(money);
            if(moneyText != null)
            {
                moneyText.SetText($"{moneyHandler.GetMoney()} <sprite=0>");
            }
        }
    }

    public void ShowSurePanel()
    {
        VibrationHandler.Vibrate(500, 255);
        if(audioSource != null && warningSound != null)
        {
            audioSource.PlayOneShot(warningSound);
        }

        if(surePanel != null)
        {
            var rect = surePanel.GetComponent<RectTransform>();
            rect.localScale = Vector3.zero;
            surePanel.SetActive(true);
            rect.DOScale(new Vector3(1.0f, 1.0f, 1.0f), 0.2f).SetEase(Ease.OutBack);
        }
    }

    public void HideSurePanel()
    {
        var rect = surePanel.GetComponent<RectTransform>();
        rect.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).OnComplete(() => surePanel.SetActive(false));
    }

    public void DeletePlayerPrefs()
    {
        int fps = PlayerPrefs.GetInt("FPS");
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume");
        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetInt("FPS", fps);
        Application.targetFrameRate = fps;
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);

        if(saveManager != null)
        {
            saveManager.DeleteData();
        }

        PlayerPrefs.Save();
    }

    private void SaveBoughtSkins()
    {
        string serializedSkins = string.Join(",", _boughtSkins);
        PlayerPrefs.SetString(BOUGHT_SKINS_KEY, serializedSkins);
        PlayerPrefs.Save();
    }
}