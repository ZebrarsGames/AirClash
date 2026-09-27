using UnityEngine;
using UnityEngine.Android;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Messaging;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using UnityEngine.Events;
using System;
using System.Threading.Tasks;

[Serializable] public class StatusTextEvent : UnityEvent<string> { }
[Serializable] public class IsServerProcessEvent : UnityEvent<bool> { }
[Serializable] public class DataLoadFromCloudEvent : UnityEvent<PlayerData> { }

public class FirebaseManager : MonoBehaviour
{
    [Header("Events")]
    public StatusTextEvent statusTextEvent;
    public IsServerProcessEvent isServerProcessEvent;
    public DataLoadFromCloudEvent dataLoadFromCloudEvent;
    
    [Header("References")]
    public SaveManager saveManager;

    // Кешированные данные для минимизации GC.Alloc
    private string lastSavedToken = "";
    private string saveFilePath;
    private string avatarPath;
    
    // Кеш объектов для запросов (избегаем постоянных new)
    private readonly AuthData authDataPayload = new AuthData();
    private readonly SyncData syncDataPayload = new SyncData();
    
    // Кеш для корутин
    private readonly WaitForSeconds waitOneSec = new WaitForSeconds(1f);
    private readonly WaitForSeconds waitHalfSec = new WaitForSeconds(0.5f);
    private readonly WaitForSeconds waitSmall = new WaitForSeconds(0.7f);

    // Константы
    private const string SERVER_URL = "https://airclashserver.onrender.com/";
    private const string HEADER_SECRET = "x-game-secret";
    private const string CONTENT_TYPE = "application/json";

    #region Data Models
    [Serializable]
    private class AuthData
    {
        public string username;
        public string password;
        public string fcm_token;
        public int timezone_offset;
    }

    [Serializable]
    private class SyncData
    {
        public string username;
        public string password;
        public string action;
        public string game_data;
    }

    [Serializable]
    private class ServerResponse
    {
        public string status;
        public string message;
        public string game_data;
        public string action; 
    }

    [Serializable]
    private class CheckUserResponse
    {
        public string status;
        public bool exists;
    }
    #endregion

    void Awake()
    {
        // Кешируем пути к файлам один раз при создании скрипта
        saveFilePath = Path.Combine(Application.persistentDataPath, "save.json");
        avatarPath = Path.Combine(Application.persistentDataPath, "avatar.png");
    }

    void Start()
    {
        if(Application.isEditor && Application.internetReachability != NetworkReachability.NotReachable)
        {
            Debug.Log("[FirebaseManager] Запущено в редакторе Unity. Симулируем получение токена...");
            lastSavedToken = "TEST_EDITOR_TOKEN_12345";
            string fakeDeviceId = "Editor_" + SystemInfo.deviceUniqueIdentifier.Substring(0, 5);
            StartCoroutine(SendTokenToServer(fakeDeviceId, lastSavedToken));
            return;
        }
        
        if(!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
        {
            Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
        }

        if(Application.internetReachability == NetworkReachability.NotReachable) return;

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => 
        {
            if(task.Result == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError($"[FirebaseManager] Не удалось запустить Firebase: {task.Result}");
            }
        });
    }

    private void InitializeFirebase()
    {
        FirebaseMessaging.TokenReceived += OnTokenReceived;
        FirebaseMessaging.RequestPermissionAsync();
    }

    private void OnTokenReceived(object sender, TokenReceivedEventArgs token)
    {
        Debug.Log($"[Android] Токен устройства получен: {token.Token}");
        lastSavedToken = token.Token;
    }

    #region Public API
    public void AccountAuth(string inputUsername, string inputPassword)
    {
        StartCoroutine(ProcessAuth(inputUsername, inputPassword));
    }

    public void SaveProgress(string inputUsername, string inputPassword)
    {
        GlobalSaveManager.SaveToDisk();

        string jsonPayload = GlobalSaveManager.GetCloudJson();

        if(string.IsNullOrEmpty(jsonPayload))
        {
            Debug.LogError("[FirebaseManager] Ошибка: JSON сохранений пуст!");
            UpdateStatus("Ошибка: Нет данных для сохранения!", false);
            return;
        }

        StartCoroutine(ProcessSync(inputUsername, inputPassword, "save", jsonPayload));
    }

    public void LoadProgress(string inputUsername, string inputPassword)
    {
        StartCoroutine(ProcessSync(inputUsername, inputPassword, "load", ""));
    }

    public void DeleteAccount(string inputUsername, string inputPassword)
    {
        StartCoroutine(ProcessDelete(inputUsername, inputPassword));
    }
    #endregion

    #region Core Logic Coroutines
    private IEnumerator ProcessAuth(string user, string pass)
    {
        UpdateStatus("Заходим в аккаунт...", true);

        authDataPayload.username = user;
        authDataPayload.password = pass;
        authDataPayload.fcm_token = lastSavedToken;
        authDataPayload.timezone_offset = (int)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalMinutes;

        yield return SendPostRequest<ServerResponse>("registerOrLogin", authDataPayload, res => 
        {
            if(res.action == "register" || res.action == "login")
            {
                SaveCredentials(user, pass);
                string msg = res.action == "register" ? "Аккаунт успешно создан!" : $"Добро пожаловать, {user}!";
                Debug.Log($"[FirebaseManager] {msg}");
                UpdateStatus(msg, false);
            }
            else
            {
                UpdateStatus($"Авторизация: {res.message}", false);
            }
        });
    }

    private IEnumerator ProcessSync(string user, string pass, string actionType, string gameDataJson)
    {
        UpdateStatus("Синхронизируемся...", true);

        syncDataPayload.username = user;
        syncDataPayload.password = pass;
        syncDataPayload.action = actionType;
        syncDataPayload.game_data = gameDataJson;

        yield return SendPostRequest<ServerResponse>("syncProgress", syncDataPayload, res => 
        {
            if(actionType == "save")
            {
                SaveCredentials(user, pass);
                Debug.Log("[FirebaseManager] Прогресс успешно загружен на сервер!");
                UpdateStatus("Прогресс успешно загружен на сервер!", false);
            }
            else if(actionType == "load")
            {
                StartCoroutine(HandleLoadSuccess(user, pass, res));
            }
        });
    }

    private IEnumerator HandleLoadSuccess(string user, string pass, ServerResponse res)
    {
        PlayerData loadedProgress;

        if(string.IsNullOrEmpty(res.game_data) || res.game_data == "{}")
        {
            Debug.LogWarning("[FirebaseManager] res.game_data пуст! Генерируем стандартные данные...");
            statusTextEvent.Invoke("Данные на сервере отсутствуют!");
            yield return waitOneSec;
            
            statusTextEvent.Invoke("Генерируем стандартные данные...");
            yield return waitOneSec;

            GlobalSaveManager.OverwriteFromCloud(JsonUtility.ToJson(new GlobalSaveData()));
            loadedProgress = saveManager.GetDefaultData();
        }
        else
        {
            GlobalSaveManager.OverwriteFromCloud(res.game_data);

            loadedProgress = JsonUtility.FromJson<PlayerData>(res.game_data);
        }

        if(loadedProgress != null && !string.IsNullOrEmpty(loadedProgress.avatarBase64))
        {
            byte[] avatarBytes = Convert.FromBase64String(loadedProgress.avatarBase64);
            File.WriteAllBytes(avatarPath, avatarBytes);
            Debug.Log("[FirebaseManager] Аватарка успешно скачана из облака и сохранена на устройство!");
        }

        SaveCredentials(user, pass);
        Debug.Log("[FirebaseManager] Прогресс успешно скачан из облака!");
        UpdateStatus("Прогресс успешно скачан из облака и перезаписан на телефоне!", false);
        
        dataLoadFromCloudEvent.Invoke(loadedProgress);
        
        yield return waitSmall;
        statusTextEvent.Invoke("Перезагружаем игру...");
        yield return waitHalfSec;
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private IEnumerator ProcessDelete(string user, string pass)
    {
        UpdateStatus("Удаление аккаунта...", true);

        authDataPayload.username = user;
        authDataPayload.password = pass;

        yield return SendPostRequest<ServerResponse>("deleteAccount", authDataPayload, res => 
        {
            Debug.Log($"[FirebaseManager] Аккаунт полностью удалён: {res.message}");
            UpdateStatus("Ваш аккаунт успешно удалён с серверов.", false);
        });
    }
    #endregion

    #region Universal Network Methods
    private IEnumerator SendPostRequest<T>(string endpoint, object payload, Action<T> onSuccess)
    {
        string jsonPayload = JsonUtility.ToJson(payload);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);

        using(UnityWebRequest www = new UnityWebRequest(SERVER_URL + endpoint, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", CONTENT_TYPE);
            www.SetRequestHeader("x-game-secret", GameConfig.ApiSecret);

            yield return www.SendWebRequest();

            if(www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"[FirebaseManager] HTTP Код ошибки: {www.responseCode}");
                HandleServerError(www.downloadHandler.text, www.responseCode);
            }
            else
            {
                try
                {
                    T responseData = JsonUtility.FromJson<T>(www.downloadHandler.text);
                    onSuccess?.Invoke(responseData);
                }
                catch(Exception e)
                {
                    Debug.LogError($"[FirebaseManager] Ошибка парсинга JSON: {e.Message}");
                    UpdateStatus("Ошибка обработки данных с сервера.", false);
                }
            }
        }
    }

    private IEnumerator SendTokenToServer(string deviceId, string token)
    {
        string jsonPayload = $"{{\"device_id\":\"{deviceId}\",\"fcm_token\":\"{token}\",\"timezone_offset\":{(int)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalMinutes}}}";
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);

        using(UnityWebRequest www = new UnityWebRequest(SERVER_URL + "saveToken", "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", CONTENT_TYPE);
            www.SetRequestHeader("x-game-secret", GameConfig.ApiSecret);

            yield return www.SendWebRequest();

            if(www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[FirebaseManager] Ошибка отправки токена: {www.error}");
            }
            else
            {
                Debug.Log("[FirebaseManager] Успех! Токен привязан к устройству.");
            }
        }
    }

    public static async Task<bool> CheckUserExistsAsync(string username)
    {
        string url = $"{SERVER_URL}checkUser?username={UnityWebRequest.EscapeURL(username)}";

        using(UnityWebRequest www = UnityWebRequest.Get(url))
        {
            www.SetRequestHeader(HEADER_SECRET, GameConfig.ApiSecret);
            var operation = www.SendWebRequest();
            
            while(!operation.isDone)
            {
                await Task.Yield();
            }

            if(www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[FirebaseManager] Ошибка сети: {www.error}");
                return false;
            }

            try
            {
                CheckUserResponse res = JsonUtility.FromJson<CheckUserResponse>(www.downloadHandler.text);
                return res.exists;
            }
            catch(Exception e)
            {
                Debug.LogError($"[FirebaseManager] Ошибка парсинга: {e.Message}");
                return false;
            }
        }
    }
    #endregion

    #region Helpers
    private void HandleServerError(string responseText, long responseCode)
    {
        try
        {
            ServerResponse res = JsonUtility.FromJson<ServerResponse>(responseText);
            string msg = res != null && !string.IsNullOrEmpty(res.message) ? res.message : $"HTTP {responseCode}";
            UpdateStatus($"Ошибка сервера: {msg}", false);
        }
        catch
        {
            UpdateStatus("Неизвестная ошибка сети или сервера.", false);
        }
    }

    private void UpdateStatus(string message, bool isProcessing)
    {
        statusTextEvent?.Invoke(message);
        isServerProcessEvent?.Invoke(isProcessing);
    }

    private void SaveCredentials(string user, string pass)
    {
        PlayerPrefs.SetString("AccountPassword", pass);
        PlayerPrefs.SetString("Nick", user);
        PlayerPrefs.Save();
    }

    private string GetCompressedAvatarBase64()
    {
        if(!File.Exists(avatarPath)) return "";

        byte[] rawPngBytes = File.ReadAllBytes(avatarPath);
        Texture2D tex = new Texture2D(2, 2);
        string base64 = "";

        if(tex.LoadImage(rawPngBytes))
        {
            byte[] compressedJpgBytes = tex.EncodeToJPG(75);
            base64 = Convert.ToBase64String(compressedJpgBytes);
            Debug.Log($"[FirebaseManager] Аватарка сжата в JPG. Размер: {compressedJpgBytes.Length / 1024} КБ");
        }
        
        Destroy(tex);
        return base64;
    }
    #endregion
}