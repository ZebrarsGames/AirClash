using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using TMPro;

public class ProfileHandler : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] RawImage avatarImage;
    [SerializeField] TextMeshProUGUI nickText;
    [SerializeField] TextMeshProUGUI moneyText;
    [SerializeField] TextMeshProUGUI goalText;
    [SerializeField] TextMeshProUGUI playtimeText;
    [SerializeField] Image currentSkinImage;
    [SerializeField] Texture defaultProfileIcon;

    [Header("Scripts")]
    [SerializeField] SaveManager saveManager;
    private string avatarPath;
    private float _nextUpdate;
    private int _lastRenderedSeconds = -1; 
    private static readonly string PlaytimeTemplate = "Наиграно: {0:00}:{1:00}:{2:00}";

    void Start()
    {
        SetProfileDataOnStart();
    }

    void Update()
    {
        if(Time.time < _nextUpdate) return;
        _nextUpdate = Time.time + 0.1f;

        if(PlaytimeTracker.Instance != null)
        {
            int totalSeconds = PlaytimeTracker.Instance.GetSecondsPlaytime(); 

            if(totalSeconds == _lastRenderedSeconds) return;
            _lastRenderedSeconds = totalSeconds;

            int hours = totalSeconds / 3600;
            int minutes = (totalSeconds % 3600) / 60;
            int seconds = totalSeconds % 60;

            playtimeText.SetText(PlaytimeTemplate, hours, minutes, seconds);
        }
    }

    public void SetProfileData(RawImage avatar)
    {
        avatarImage.texture = avatar.texture;
        saveManager.SaveData();
    }

    public void SetProfileDataOnStart()
    {
        PlayerData currentData = saveManager.GetData();
        avatarPath = Path.Combine(Application.persistentDataPath, "avatar.png");
        moneyText.text = "Общие деньги: " + saveManager.GetData().TotalMoney;
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
