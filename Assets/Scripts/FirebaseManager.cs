using UnityEngine;
using UnityEngine.Android;
using Firebase;
using Firebase.Messaging;
using UnityEngine.Networking;
using System.Collections;
using System.IO;

public class FirebaseManager : MonoBehaviour
{
    private string lastSavedToken = ""; // Переменная для хранения токена

    // Вспомогательные классы для отправки и получения JSON-данных
    [System.Serializable]
    public class AuthData
    {
        public string username;
        public string password;
        public string fcm_token;
        public int timezone_offset;
    }

    [System.Serializable]
    public class SyncData
    {
        public string username;
        public string password;
        public string action;
        public string game_data;
    }

    [System.Serializable]
    public class ServerResponse
    {
        public string status;
        public string message;
        public string game_data;
    }

    void Start()
    {
        if (Application.isEditor)
        {
            Debug.Log("⚠️ Запущено в редакторе Unity. Симулируем получение токена...");
            lastSavedToken = "TEST_EDITOR_TOKEN_12345";
            string fakeDeviceId = "Editor_Computer_" + SystemInfo.deviceUniqueIdentifier.Substring(0, 5);
            StartCoroutine(SendTokenToServer(fakeDeviceId, lastSavedToken));
        }
        else 
        {
            if (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
            {
                Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
            }
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
                var dependencyStatus = task.Result;
                if (dependencyStatus == DependencyStatus.Available) {
                    InitializeFirebase();
                } else {
                    Debug.LogError($"Не удалось запустить Firebase: {dependencyStatus}");
                }
            });
        }
    }

    void InitializeFirebase()
    {
        FirebaseMessaging.TokenReceived += OnTokenReceived;
        FirebaseMessaging.RequestPermissionAsync();
    }

    void OnTokenReceived(object sender, TokenReceivedEventArgs token)
    {
        Debug.Log($"[Android] Токен устройства получен: {token.Token}");
        lastSavedToken = token.Token;
        StartCoroutine(SendTokenToServer(SystemInfo.deviceUniqueIdentifier, token.Token));
    }

    IEnumerator SendTokenToServer(string deviceId, string token)
    {
        string serverUrl = "https://airclashserver.onrender.com/saveToken"; 
        string jsonPayload = $"{ItemsString(deviceId, token)}";

        UnityWebRequest www = new UnityWebRequest(serverUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success) {
            Debug.LogError($"❌ Ошибка отправки токена: {www.error}");
        } else {
            Debug.Log("✅ Успех! Токен привязан к устройству.");
        }
    }

    private string ItemsString(string deviceId, string token)
    {
        return $"{{\"device_id\":\"{deviceId}\",\"fcm_token\":\"{token}\",\"timezone_offset\":{(int)System.TimeZoneInfo.Local.GetUtcOffset(System.DateTime.Now).TotalMinutes}}}";
    }

    // =========================================================================
    // [НОВЫЕ ПУБЛИЧНЫЕ МЕТОДЫ] Вызывайте их из UI (кнопки или другие менеджеры)
    // =========================================================================

    /// Вызывать при нажатии на кнопку "Войти / Регистрация"
    public void AccountAuth(string inputUsername, string inputPassword)
    {
        StartCoroutine(SendAuthRequest(inputUsername, inputPassword));
    }

    /// Вызывать при нажатии на кнопку "SAVE" (Сохранение прогресса в облако)
    public void SaveProgress(string inputUsername, string inputPassword)
    {
        // Читаем локальный JSON файл вашей игры (подставьте ваш правильный путь к файлу!)
        string saveFilePath = Path.Combine(Application.persistentDataPath, "save.json");
        
        if (!File.Exists(saveFilePath))
        {
            Debug.LogError("❌ Локальный файл сохранения не найден!");
            return;
        }

        string localJsonData = File.ReadAllText(saveFilePath);
        StartCoroutine(SendSyncRequest(inputUsername, inputPassword, "save", localJsonData));
    }

    /// Вызывать при нажатии на кнопку "LOAD" (Загрузка прогресса из облака)
    public void LoadProgress(string inputUsername, string inputPassword)
    {
        StartCoroutine(SendSyncRequest(inputUsername, inputPassword, "load", ""));
    }

    // =========================================================================
    // КОРОУТИНЫ ДЛЯ СЕТЕВЫХ ЗАПРОСОВ К СЕРВЕРУ RENDER
    // =========================================================================

    IEnumerator SendAuthRequest(string user, string pass)
    {
        string url = "https://airclashserver.onrender.com/registerOrLogin";

        AuthData data = new AuthData();
        data.username = user;
        data.password = pass;
        data.fcm_token = lastSavedToken; // Передаем токен пушей, чтобы связать с аккаунтом
        data.timezone_offset = (int)System.TimeZoneInfo.Local.GetUtcOffset(System.DateTime.Now).TotalMinutes;

        string jsonPayload = JsonUtility.ToJson(data);

        UnityWebRequest www = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            HandleServerError(www.downloadHandler.text);
        }
        else
        {
            ServerResponse res = JsonUtility.FromJson<ServerResponse>(www.downloadHandler.text);
            Debug.Log($"✅ Авторизация: {res.message}");
            // Здесь можно вызвать метод открытия основного игрового меню
        }
    }

    IEnumerator SendSyncRequest(string user, string pass, string actionType, string gameDataJson)
    {
        string url = "https://airclashserver.onrender.com/syncProgress";

        SyncData data = new SyncData();
        data.username = user;
        data.password = pass;
        data.action = actionType;
        data.game_data = gameDataJson;

        string jsonPayload = JsonUtility.ToJson(data);

        UnityWebRequest www = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            HandleServerError(www.downloadHandler.text);
        }
        else
        {
            ServerResponse res = JsonUtility.FromJson<ServerResponse>(www.downloadHandler.text);
            
            if (actionType == "save")
            {
                Debug.Log("✅ Прогресс успешно загружен на сервер!");
            }
            else if (actionType == "load")
            {
                // Сохраняем полученный JSON из облака в локальный файл телефона
                string saveFilePath = Path.Combine(Application.persistentDataPath, "save.json");
                File.WriteAllText(saveFilePath, res.game_data);
                
                Debug.Log("✅ Прогресс успешно скачан из облака и перезаписан на телефоне!");
                // ВНИМАНИЕ: Здесь нужно вызвать метод перезагрузки данных в вашей игре,
                // чтобы интерфейс (монеты, скины) обновился под новое сохранение!
            }
        }
    }

    void HandleServerError(string responseText)
    {
        try
        {
            ServerResponse res = JsonUtility.FromJson<ServerResponse>(responseText);
            Debug.LogError($"❌ Ошибка сервера: {res.message}");
        }
        catch
        {
            Debug.LogError("❌ Неизвестная ошибка сети или сервера.");
        }
    }
}