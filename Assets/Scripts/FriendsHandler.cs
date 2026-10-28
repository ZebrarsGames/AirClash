using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class FriendsHandler : MonoBehaviour
{
    private const string BASE_URL = "https://airclashserver.onrender.com";

    public void GetFriendsList(string username, Action<string[]> onFriendsReceived)
    {
        StartCoroutine(GetFriendsListRoutine(username, onFriendsReceived));
    }

    public void CheckFriendOnline(string friendUsername, Action<bool> onOnlineStatusReceived)
    {
        StartCoroutine(CheckFriendOnlineRoutine(friendUsername, onOnlineStatusReceived));
    }

    public void SendBattleInvite(string senderUsername, string friendUsername, string message, Action<bool> onComplete)
    {
        StartCoroutine(SendBattleInviteRoutine(senderUsername, friendUsername, message, onComplete));
    }

    public void GetMyFriendCode(string username, Action<string> onCodeReceived)
    {
        StartCoroutine(GetMyFriendCodeRoutine(username, onCodeReceived));
    }

    public void SendFriendRequestByCode(string username, string friendCode, Action<bool, string> onComplete)
    {
        StartCoroutine(SendFriendRequestByCodeRoutine(username, friendCode, onComplete));
    }

    public void GetPendingRequests(string username, Action<string[]> onRequestsReceived)
    {
        StartCoroutine(GetPendingRequestsRoutine(username, onRequestsReceived));
    }

    public void AcceptFriendRequest(string username, string requesterUsername, Action<bool> onComplete)
    {
        StartCoroutine(AcceptFriendRequestRoutine(username, requesterUsername, onComplete));
    }

    IEnumerator GetFriendsListRoutine(string username, Action<string[]> onFriendsReceived)
    {
        string url = $"{BASE_URL}/friends/list";

        FriendRequestData data = new FriendRequestData { username = username };
        string jsonPayload = JsonUtility.ToJson(data);

        using(UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("x-game-secret", GameConfig.ApiSecret);
            www.timeout = 10;

            yield return www.SendWebRequest();

            if(www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"[FriendsHandler] Ошибка получения списка друзей: {www.error}");
                onFriendsReceived?.Invoke(new string[0]);
            }
            else
            {
                string json = www.downloadHandler.text;
                FriendsListResponse res = JsonUtility.FromJson<FriendsListResponse>(json);

                if(res != null && res.status == "success")
                {
                    onFriendsReceived?.Invoke(res.friends);
                }
                else
                {
                    onFriendsReceived?.Invoke(new string[0]);
                }
            }
        }
    }

    IEnumerator CheckFriendOnlineRoutine(string friendUsername, Action<bool> onOnlineStatusReceived)
    {
        string url = $"{BASE_URL}/friends/checkOnline";

        FriendRequestData data = new FriendRequestData { username = friendUsername };
        string jsonPayload = JsonUtility.ToJson(data);

        using(UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("x-game-secret", GameConfig.ApiSecret);
            www.timeout = 10;

            yield return www.SendWebRequest();

            if(www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"[FriendsHandler] Не удалось проверить статус игрока {friendUsername}: {www.error}");
                onOnlineStatusReceived?.Invoke(false);
            }
            else
            {
                string json = www.downloadHandler.text;
                OnlineStatusResponse res = JsonUtility.FromJson<OnlineStatusResponse>(json);

                if(res != null && res.status == "success")
                {
                    onOnlineStatusReceived?.Invoke(res.is_online);
                }
                else
                {
                    onOnlineStatusReceived?.Invoke(false);
                }
            }
        }
    }

    IEnumerator SendBattleInviteRoutine(string senderUsername, string friendUsername, string message, Action<bool> onComplete)
    {
        string url = $"{BASE_URL}/friends/invite";

        BattleInviteData data = new BattleInviteData
        {
            senderUsername = senderUsername,
            friendUsername = friendUsername,
            message = message
        };
        string jsonPayload = JsonUtility.ToJson(data);

        using(UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("x-game-secret", GameConfig.ApiSecret);
            www.timeout = 10;

            yield return www.SendWebRequest();

            if(www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[FriendsHandler] Ошибка отправки приглашения: {www.error} | Ответ: {www.downloadHandler.text}");
                onComplete?.Invoke(false);
            }
            else
            {
                Debug.Log("[FriendsHandler] Приглашение на битву успешно отправлено!");
                onComplete?.Invoke(true);
            }
        }
    }

    IEnumerator GetMyFriendCodeRoutine(string username, Action<string> onCodeReceived)
    {
        string url = $"{BASE_URL}/friends/getCode";

        FriendRequestData data = new FriendRequestData { username = username };
        string jsonPayload = JsonUtility.ToJson(data);

        using(UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("x-game-secret", GameConfig.ApiSecret);
            www.timeout = 10;

            yield return www.SendWebRequest();

            if(www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[FriendsHandler] Ошибка получения кода друга: {www.error}");
                onCodeReceived?.Invoke(string.Empty);
            }
            else
            {
                FriendCodeResponse res = JsonUtility.FromJson<FriendCodeResponse>(www.downloadHandler.text);
                if(res != null && res.status == "success")
                {
                    onCodeReceived?.Invoke(res.friend_code);
                }
                else
                {
                    onCodeReceived?.Invoke(string.Empty);
                }
            }
        }
    }

    IEnumerator SendFriendRequestByCodeRoutine(string username, string friendCode, Action<bool, string> onComplete)
    {
        string url = $"{BASE_URL}/friends/sendByCode";

        FriendCodeRequestData data = new FriendCodeRequestData
        {
            username = username,
            friendCode = friendCode
        };
        string jsonPayload = JsonUtility.ToJson(data);

        using(UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("x-game-secret", GameConfig.ApiSecret);
            www.timeout = 10;

            yield return www.SendWebRequest();

            if(www.result != UnityWebRequest.Result.Success)
            {
                string errorMsg = "Ошибка соединения";
                try
                {
                    ErrorResponse errRes = JsonUtility.FromJson<ErrorResponse>(www.downloadHandler.text);
                    if (errRes != null) errorMsg = errRes.message;
                }
                catch { }

                Debug.LogWarning($"[FriendsHandler] Не удалось отправить запрос по коду: {errorMsg}");
                onComplete?.Invoke(false, errorMsg);
            }
            else
            {
                Debug.Log("[FriendsHandler] Запрос в друзья успешно отправлен!");
                onComplete?.Invoke(true, "Запрос успешно отправлен!");
            }
        }
    }

    IEnumerator GetPendingRequestsRoutine(string username, Action<string[]> onRequestsReceived)
    {
        string url = $"{BASE_URL}/friends/pending";

        FriendRequestData data = new FriendRequestData { username = username };
        string jsonPayload = JsonUtility.ToJson(data);

        using(UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("x-game-secret", GameConfig.ApiSecret);
            www.timeout = 10;

            yield return www.SendWebRequest();

            if(www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"[FriendsHandler] Ошибка получения входящих запросов: {www.error}");
                onRequestsReceived?.Invoke(new string[0]);
            }
            else
            {
                PendingRequestsResponse res = JsonUtility.FromJson<PendingRequestsResponse>(www.downloadHandler.text);
                if(res != null && res.status == "success")
                {
                    onRequestsReceived?.Invoke(res.requests);
                }
                else
                {
                    onRequestsReceived?.Invoke(new string[0]);
                }
            }
        }
    }

    IEnumerator AcceptFriendRequestRoutine(string username, string requesterUsername, Action<bool> onComplete)
    {
        string url = $"{BASE_URL}/friends/accept";

        AcceptRequestData data = new AcceptRequestData
        {
            username = username,
            requesterUsername = requesterUsername
        };
        string jsonPayload = JsonUtility.ToJson(data);

        using(UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("x-game-secret", GameConfig.ApiSecret);
            www.timeout = 10;

            yield return www.SendWebRequest();

            if(www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[FriendsHandler] Ошибка принятия запроса в друзья: {www.error} | Ответ: {www.downloadHandler.text}");
                onComplete?.Invoke(false);
            }
            else
            {
                Debug.Log("[FriendsHandler] Запрос в друзья успешно принят!");
                onComplete?.Invoke(true);
            }
        }
    }
}


[Serializable]
public class FriendRequestData { public string username; }

[Serializable]
public class FriendCodeRequestData { public string username; public string friendCode; }

[Serializable]
public class AcceptRequestData { public string username; public string requesterUsername; }

[Serializable]
public class BattleInviteData { public string senderUsername; public string friendUsername; public string message; }

[Serializable]
public class FriendsListResponse { public string status; public string[] friends; }

[Serializable]
public class OnlineStatusResponse { public string status; public string username; public bool is_online; }

[Serializable]
public class FriendCodeResponse { public string status; public string friend_code; }

[Serializable]
public class PendingRequestsResponse { public string status; public string[] requests; }

[Serializable]
public class ErrorResponse { public string status; public string message; }