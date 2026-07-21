using UnityEngine;
using Firebase;
using Firebase.Messaging;
using UnityEngine.Networking;
using System.Collections;

public class FirebaseManager : MonoBehaviour
{
    void Start()
    {
        if (Application.isEditor)
        {
            Debug.Log("⚠️ Запущено в редакторе Unity. Симулируем получение токена...");
            // Генерируем тестовый токен и ID для проверки
            string fakeToken = "TEST_EDITOR_TOKEN_12345";
            string fakeDeviceId = "Editor_Computer_" + SystemInfo.deviceUniqueIdentifier.Substring(0, 5);
            
            StartCoroutine(SendTokenToServer(fakeDeviceId, fakeToken));
        }
        else // Если запускаем на реальном Android-телефоне
        {
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
        // Подписываемся на событие получения токена
        FirebaseMessaging.TokenReceived += OnTokenReceived;
        
        // Запрашиваем разрешения на пуши (критично для Android 13+)
        FirebaseMessaging.RequestPermissionAsync();
    }

    void OnTokenReceived(object sender, TokenReceivedEventArgs token)
    {
        Debug.Log($"[Android] Токен устройства получен: {token.Token}");
        StartCoroutine(SendTokenToServer(SystemInfo.deviceUniqueIdentifier, token.Token));
    }

    IEnumerator SendTokenToServer(string deviceId, string token)
    {
        string serverUrl = "https://airclashserver.onrender.com/saveToken"; 
        
        string jsonPayload = $"{{\"device_id\":\"{deviceId}\",\"fcm_token\":\"{token}\",\"timezone_offset\":{(int)System.TimeZoneInfo.Local.GetUtcOffset(System.DateTime.Now).TotalMinutes}}}";

        // 1. Создаем пустой POST запрос
        UnityWebRequest www = new UnityWebRequest(serverUrl, "POST");
        
        // 2. Кодируем JSON в байты и прикрепляем к телу запроса
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        
        // 3. Устанавливаем заголовки
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success) {
            Debug.LogError($"❌ Ошибка отправки на Render: {www.error} (Код: {www.responseCode})");
        } else {
            Debug.Log("✅ Успех! Данные успешно отправлены на сервер!");
        }
    }
}
