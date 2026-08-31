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
    [SerializeField] RawImage avatarImage;
    [SerializeField] TextMeshProUGUI nickText;
    [SerializeField] TextMeshProUGUI moneyText;
    [SerializeField] TextMeshProUGUI goalText;
    [SerializeField] TextMeshProUGUI playtimeText;
    [SerializeField] TextMeshProUGUI eloText;
    [SerializeField] Image eloLevelImg;
    [SerializeField] Texture defaultProfileIcon;

    [Header("Elo Levels Icons")]
    [SerializeField] private Sprite[] eloLevelsIcons;
    [SerializeField] private Sprite noInternetIcon;

    [Header("Scripts")]
    [SerializeField] SaveManager saveManager;
    private string avatarPath;
    private float _nextUpdate;
    private int _lastRenderedSeconds = -1; 
    private static readonly string PlaytimeTemplate = "Наиграно: {0:00}:{1:00}:{2:00}";
    private bool isSetElo = true;

    private readonly int[] levelThresholds = {
        100,  //Уровень 1
        400,  //Уровень 2
        600,  //Уровень 3
        800,  //Уровень 4
        1050, //Уровень 5
        1300, //Уровень 6
        1600, //Уровень 7
        1900, //Уровень 8
        2250, //Уровень 9
        2600, //Уровень 10
        3000 //Уровень Мастер
    };

    async void Start()
    {   if(Application.internetReachability == NetworkReachability.NotReachable)
        {
            eloText.text = "Ваш эло: нет подключения к интернету!";
            eloLevelImg.sprite = noInternetIcon;
            isSetElo = false;
        } else
        {
            bool isAccountExists = await GetIsExists(PlayerPrefs.GetString("Nick", "Ник"));
            if(!isAccountExists)
            {
                eloText.text = "Ваш эло: аккаунт не создан!";
                eloLevelImg.sprite = noInternetIcon;
                isSetElo = false;
            }
        }
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
        
        moneyText.text = "Общие деньги: " + currentData.TotalMoney;
        goalText.text = "Голы: " + currentData.Goals;
        nickText.text = currentData.NickName;
        playtimeText.text = "Наиграно: " + PlaytimeTracker.Instance.GetFormattedPlaytime();
        if(isSetElo) SetElo();
        
        if(File.Exists(avatarPath))
        {
            byte[] bytes = File.ReadAllBytes(avatarPath);
            
            Texture2D savedTexture = new Texture2D(2, 2);
            savedTexture.LoadImage(bytes);

            avatarImage.texture = savedTexture;
            Debug.Log("Сохраненный аватар успешно загружен при старте.");
        } 
        else
        {
            avatarImage.texture = defaultProfileIcon;
        }
    }

    private async void SetElo()
    {
        eloLevelImg.enabled = false;

        string savedNick = PlayerPrefs.GetString("Nick", "Ник");
        bool isAccountExists = await GetIsExists(savedNick);
        
        if(isAccountExists)
        {
            StartCoroutine(GetPlayerEloRequest(savedNick, (elo) => {
                if(elo < 100) 
                {
                    eloText.text = "Эло: Ошибка сети";
                    return;
                }

                eloText.text = $"Эло: {elo}";
                int targetIndex = 0;

                for(int i = levelThresholds.Length - 1; i >= 0; i--)
                {
                    if(elo >= levelThresholds[i])
                    {
                        targetIndex = i;
                        break;
                    }
                }

                if(targetIndex < eloLevelsIcons.Length)
                {
                    eloLevelImg.sprite = eloLevelsIcons[targetIndex];
                    eloLevelImg.enabled = true;

                    bool isMaxLevel = (targetIndex == levelThresholds.Length - 1);
                    if(isMaxLevel)
                    {
                        eloLevelImg.gameObject.GetComponent<AspectRatioFitter>().aspectRatio = 2;
                        eloLevelImg.gameObject.GetComponent<RectTransform>().localPosition = new Vector2(-50, 0);
                        eloText.gameObject.GetComponent<RectTransform>().localPosition = new Vector3(-270, 0);
                    } 
                    else
                    {
                        eloLevelImg.gameObject.GetComponent<AspectRatioFitter>().aspectRatio = 1;
                        eloLevelImg.gameObject.GetComponent<RectTransform>().localPosition = Vector2.zero;
                        eloText.gameObject.GetComponent<RectTransform>().localPosition = new Vector3(-180, 0);
                    }
                    
                }
            }));
        } 
        else 
        {
            eloText.text = "Эло: 100";
            if(eloLevelsIcons.Length > 0)
            {
                eloLevelImg.sprite = eloLevelsIcons[0];
                eloLevelImg.enabled = true;
            }
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