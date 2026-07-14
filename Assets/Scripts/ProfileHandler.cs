using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;

public class ProfileHandler : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] RawImage avatarImage;
    [SerializeField] Text nickText;
    [SerializeField] Text moneyText;
    [SerializeField] Text goalText;
    [SerializeField] Text playtimeText;
    [SerializeField] Image currentSkinImage;
    [SerializeField] Texture defaultProfileIcon;

    [Header("Scripts")]
    [SerializeField] SaveManager saveManager;
    [SerializeField] MoneyHandler moneyHandler;
    private string avatarPath;
    private float _nextUpdate;

    void Start()
    {
        SetProfileDataOnStart();
    }

    void Update()
    {
        if (Time.time < _nextUpdate) return;
        _nextUpdate = Time.time + 0.1f;
        
        if (PlaytimeTracker.Instance != null)
        {
            playtimeText.text = "Наиграно: " + PlaytimeTracker.Instance.GetFormattedPlaytime();
        }
    }

    public void SetProfileData(string nick, RawImage avatar)
    {
        avatarImage.texture = avatar.texture;
        nickText.text = nick;
        PlayerPrefs.SetString("Nick", nick);
        PlayerPrefs.Save();
        saveManager.SaveData();
    }

    public void SetProfileDataOnStart()
    {
        PlayerData currentData = saveManager.GetData();
        avatarPath = Path.Combine(Application.persistentDataPath, "avatar.png");
        moneyText.text = "Деньги: " + moneyHandler.GetMoney();
        goalText.text = "Голы: " + currentData.Goals;
        nickText.text = currentData.NickName;
        playtimeText.text = "Наиграно: " + PlaytimeTracker.Instance.GetFormattedPlaytime();
        SkinData currentSkinSO = Resources.LoadAll<SkinData>("").FirstOrDefault(item => item.name == currentData.CurrentSkinName);
        if(currentSkinSO == null)
        {
            currentSkinSO = Resources.LoadAll<SkinData>("").FirstOrDefault(item => item.name == "DefSkin");
        } 
        currentSkinImage.sprite = currentSkinSO.sprite; 
        if (File.Exists(avatarPath))
        {
            byte[] bytes = File.ReadAllBytes(avatarPath);
            
            Texture2D savedTexture = new Texture2D(2, 2);
            savedTexture.LoadImage(bytes);

            avatarImage.texture = savedTexture;
            Debug.Log("Сохраненный аватар успешно загружен при старте.");
        } else
        {
            avatarImage.texture = defaultProfileIcon;
        }
    }
}
