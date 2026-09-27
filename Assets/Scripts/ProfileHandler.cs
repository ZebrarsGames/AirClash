using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;
using System;
using System.Collections;
using System.Text;
using UnityEngine.Networking;
using System.Threading.Tasks;

public class ProfileHandler : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private RawImage avatarImage;
    [SerializeField] private TextMeshProUGUI nickText;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI goalText;
    [SerializeField] private TextMeshProUGUI playtimeText;
    [SerializeField] private TextMeshProUGUI eloText;
    [SerializeField] private Image eloLevelImg;
    [SerializeField] private Texture defaultProfileIcon;

    [Header("Elo Levels Icons")]
    [SerializeField] private Sprite[] eloLevelsIcons;
    [SerializeField] private Sprite noInternetIcon;

    [Header("Scripts")]
    [SerializeField] private SaveManager saveManager;

    private string avatarPath;
    private float _nextUpdate;
    private int _lastRenderedSeconds = -1; 
    private static readonly string PlaytimeTemplate = "Наиграно: {0:00}:{1:00}:{2:00}";

    private RectTransform _eloLevelRect;
    private AspectRatioFitter _eloLevelFitter;
    private RectTransform _eloTextRect;

    private static readonly int[] LevelThresholds = {
        100,  // Уровень 1
        400,  // Уровень 2
        600,  // Уровень 3
        800,  // Уровень 4
        1050, // Уровень 5
        1300, // Уровень 6
        1600, // Уровень 7
        1900, // Уровень 8
        2250, // Уровень 9
        2600, // Уровень 10
        3000  // Уровень Мастер
    };

    void Awake()
    {
        avatarPath = Path.Combine(Application.persistentDataPath, "avatar.png");
        
        if(eloLevelImg != null)
        {
            _eloLevelRect = eloLevelImg.GetComponent<RectTransform>();
            _eloLevelFitter = eloLevelImg.GetComponent<AspectRatioFitter>();
        }
        if(eloText != null)
        {
            _eloTextRect = eloText.GetComponent<RectTransform>();
        }
    }

    async void Start()
    {   
        bool isAccountExists = false;

        if(Application.internetReachability == NetworkReachability.NotReachable)
        {
            eloText.text = "Ваш эло: нет подключения к интернету!";
            eloLevelImg.sprite = noInternetIcon;
        } 
        else
        {
            string nick = PlayerPrefs.GetString("Nick", "Ник");
            isAccountExists = await GetIsExists(nick);
            
            if(!isAccountExists)
            {
                eloText.text = "Ваш эло: аккаунт не создан!";
                eloLevelImg.sprite = noInternetIcon;
            }
        }

        SetProfileDataOnStart(isAccountExists);
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

    public void SetProfileDataOnStart(bool isAccountExists)
    {
        PlayerData currentData = saveManager.GetData();
        
        moneyText.SetText("Общие деньги: {0}", currentData.TotalMoney);
        goalText.SetText("Голы: {0}", currentData.Goals);
        nickText.text = currentData.NickName;
        playtimeText.text = "Наиграно: " + PlaytimeTracker.Instance.GetFormattedPlaytime();
        
        if(isAccountExists)
        {
            SetElo();
        }

        LoadAvatar();
    }

    private void SetElo()
    {
        eloLevelImg.enabled = false;
        string savedNick = PlayerPrefs.GetString("Nick", "Ник");

        StartCoroutine(GetPlayerEloRequest(savedNick, (elo) => {
            if(elo < 100) 
            {
                eloText.text = "Эло: Ошибка сети";
                return;
            }

            eloText.SetText("Эло: {0}", elo);
            int targetIndex = 0;

            for(int i = LevelThresholds.Length - 1; i >= 0; i--)
            {
                if(elo >= LevelThresholds[i])
                {
                    targetIndex = i;
                    break;
                }
            }

            if(targetIndex < eloLevelsIcons.Length)
            {
                eloLevelImg.sprite = eloLevelsIcons[targetIndex];
                eloLevelImg.enabled = true;

                bool isMaxLevel = (targetIndex == LevelThresholds.Length - 1);
                if(isMaxLevel)
                {
                    if(_eloLevelFitter != null) _eloLevelFitter.aspectRatio = 2;
                    if(_eloLevelRect != null) _eloLevelRect.localPosition = new Vector2(-50, 0);
                    if(_eloTextRect != null) _eloTextRect.localPosition = new Vector3(-270, 0);
                } 
                else
                {
                    if(_eloLevelFitter != null) _eloLevelFitter.aspectRatio = 1;
                    if(_eloLevelRect != null) _eloLevelRect.localPosition = Vector2.zero;
                    if(_eloTextRect != null) _eloTextRect.localPosition = new Vector3(-180, 0);
                }
            }
        }));
    }

    private void LoadAvatar()
    {
        if(File.Exists(avatarPath))
        {
            byte[] bytes = File.ReadAllBytes(avatarPath);
            
            Texture2D savedTexture = new Texture2D(2, 2);
            savedTexture.LoadImage(bytes);

            ClearAvatarTexture();
            avatarImage.texture = savedTexture;
            Debug.Log("Сохраненный аватар успешно загружен при старте.");
        } 
        else
        {
            avatarImage.texture = defaultProfileIcon;
        }
    }

    private void ClearAvatarTexture()
    {
        if(avatarImage.texture != null && avatarImage.texture != defaultProfileIcon)
        {
            Destroy(avatarImage.texture);
            avatarImage.texture = null;
        }
    }

    private Task<bool> GetIsExists(string username) 
    {
        return FirebaseManager.CheckUserExistsAsync(username);
    }

    IEnumerator GetPlayerEloRequest(string bigUser, Action<int> onEloReceived)
    {   
        string user = bigUser.ToLower();
        string url = "https://airclashserver.onrender.com/getElo";

        EloRequestData data = new EloRequestData();
        data.username = user;
        string jsonPayload = JsonUtility.ToJson(data);

        using(UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("x-game-secret", GameConfig.ApiSecret);

            yield return www.SendWebRequest();

            if(www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"HTTP Код ошибки: {www.responseCode}");
                onEloReceived?.Invoke(-1);
            }
            else
            {
                EloResponseData res = JsonUtility.FromJson<EloResponseData>(www.downloadHandler.text);
                
                if(res.status == "success")
                {
                    Debug.Log($"ELO успешно получено для {user}: {res.elo}");
                    onEloReceived?.Invoke(res.elo);
                }
                else
                {
                    Debug.LogWarning($"Сервер вернул ошибку: {res.message}");
                    onEloReceived?.Invoke(-1);
                }
            }
        }
    }
}