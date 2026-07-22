using UnityEngine;
using UnityEngine.Android;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Messaging;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using UnityEngine.Events;

[System.Serializable]
public class StatusTextEvent : UnityEvent<string> { }
[System.Serializable]
public class IsServerProcessEvent : UnityEvent<bool> { }
[System.Serializable]
public class DataLoadFromCloudEvent : UnityEvent<PlayerData> { }
public class FirebaseManager : MonoBehaviour
{
    private string lastSavedToken = ""; // Переменная для хранения токена
    public StatusTextEvent statusTextEvent;
    public IsServerProcessEvent isServerProcessEvent;
    public DataLoadFromCloudEvent dataLoadFromCloudEvent;

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
        string saveFilePath = Path.Combine(Application.persistentDataPath, "save.json");
        string avatarPath = Path.Combine(Application.persistentDataPath, "avatar.png");

        if (!File.Exists(saveFilePath))
        {
            Debug.LogError("❌ Локальный файл сохранения не найден!");
            return;
        }

        // 1. Читаем ваш текущий JSON сохранения
        string localJsonData = File.ReadAllText(saveFilePath);
        
        // Десериализуем его в объект
        PlayerData progress = JsonUtility.FromJson<PlayerData>(localJsonData);

        // 2. Если файл аватарки существует, обрабатываем его
        if (File.Exists(avatarPath))
        {
            byte[] rawPngBytes = File.ReadAllBytes(avatarPath);

            // Создаем временную текстуру для конвертации
            Texture2D tex = new Texture2D(2, 2);
            if (tex.LoadImage(rawPngBytes))
            {
                // СЖИМАЕМ В JPEG: 75 — это идеальный баланс веса и качества (от 1 до 100)
                byte[] compressedJpgBytes = tex.EncodeToJPG(75);
                
                // Конвертируем байты картинки в безопасную строку Base64
                progress.avatarBase64 = System.Convert.ToBase64String(compressedJpgBytes);
                Debug.Log($"📸 Аватарка сжата в JPG. Размер: {compressedJpgBytes.Length / 1024} КБ");
            }
            Destroy(tex); // Очищаем память
        }
        else
        {
            progress.avatarBase64 = ""; // Если аватарки нет
        }

        // 3. Запаковываем обновленный объект с аватаркой обратно в JSON строку
        string finalJsonToSend = JsonUtility.ToJson(progress);

        // Отправляем на сервер
        StartCoroutine(SendSyncRequest(inputUsername, inputPassword, "save", finalJsonToSend));
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
        statusTextEvent.Invoke("Заходим в аккаунт...");
        isServerProcessEvent.Invoke(true);
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
            statusTextEvent.Invoke($"Авторизация: {res.message}");
            isServerProcessEvent.Invoke(false);
        }
    }

    IEnumerator SendSyncRequest(string user, string pass, string actionType, string gameDataJson)
    {
        statusTextEvent.Invoke("Синхронизируемся...");
        isServerProcessEvent.Invoke(true);
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
                statusTextEvent.Invoke("Прогресс успешно загружен на сервер!");
                isServerProcessEvent.Invoke(false);
            }
            else if (actionType == "load")
            {
                // Сохраняем полученный JSON из облака в локальный файл телефона
                string saveFilePath = Path.Combine(Application.persistentDataPath, "save.json");
                File.WriteAllText(saveFilePath, res.game_data);
                PlayerData playerData = JsonUtility.FromJson<PlayerData>(res.game_data);
                
                Debug.Log("✅ Прогресс успешно скачан из облака и перезаписан на телефоне!");
                statusTextEvent.Invoke("Прогресс успешно скачан из облака и перезаписан на телефоне!");
                isServerProcessEvent.Invoke(false);
                dataLoadFromCloudEvent.Invoke(playerData);
                yield return new WaitForSeconds(1.0f);
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }

    void HandleServerError(string responseText)
    {
        try
        {
            ServerResponse res = JsonUtility.FromJson<ServerResponse>(responseText);
            Debug.LogWarning($"❌ Ошибка сервера: {res.message}");
            statusTextEvent.Invoke($"Ошибка сервера: {res.message}");
            isServerProcessEvent.Invoke(false);
        }
        catch
        {
            Debug.LogWarning("❌ Неизвестная ошибка сети или сервера.");
            statusTextEvent.Invoke("Неизвестная ошибка сети или сервера.");
            isServerProcessEvent.Invoke(false);
        }
    }
}