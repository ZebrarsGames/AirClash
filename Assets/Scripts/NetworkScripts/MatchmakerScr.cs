using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using EpicTransport;
using Mirror;

#region DTOs for Matchmaking
[Serializable]
public class CreateMMRoomRequest
{
    public string eos_id;
    public int elo;
    public int played_matches; 
}

[Serializable]
public class CreateMMRoomResponse
{
    public string status;
    public string message;
    public string host_eos_id;
    public int elo;
}

[Serializable]
public class FindOpponentRequest
{
    public string eos_id;
    public int elo;
    public int played_matches;
    public int max_elo_diff;
}

[Serializable]
public class FindOpponentResponse
{
    public string status; // "found" | "not_found" | "error"
    public string host_eos_id;
    public int host_elo;
    public string message;
}

[Serializable]
public class CancelSearchRequest
{
    public string eos_id;
}

[Serializable]
public class CancelSearchResponse
{
    public string status;
    public string message;
}

[System.Serializable]
public class EloRequestData
{
    public string username;
}

[System.Serializable]
public class EloResponseData
{
    public string status;
    public string message;
    public int elo;
}
#endregion

public class MatchmakerScr : MonoBehaviour
{
    private const string BASE_URL = "https://airclashserver.onrender.com";

    [Header("Elo Settings")]
    [SerializeField] private int defaultElo = 500;

    [Header("Matchmaking Parameters")]
    [SerializeField] private int initialEloRange = 50;
    [SerializeField] private int rangeExpandStep = 50;
    [SerializeField] private float expandInterval = 5f;
    [SerializeField] private int maxEloRange = 500;

    private int currentElo;
    private int currentRange;
    private int currentPlayedMatches;
    private Coroutine matchmakingCoroutine;
    private bool isSearching = false;
    private bool isHostCreated = false;

    public UnityEvent<int, int> OnSearchRangeUpdated; // (minElo, maxElo)
    public UnityEvent OnMatchmakingStart;
    public UnityEvent OnMatchmakingCancel;
    public UnityEvent OnMatchFound;

    void Start()
    {
        string username = PlayerPrefs.GetString("Nick", "Ник"); 
        currentPlayedMatches = PlayerPrefs.GetInt("MyMatches", 0);

        if(string.IsNullOrEmpty(username))
        {
            Debug.LogWarning("[MatchmakerScr] Имя пользователя не найдено! Применяем стандартный ELO.");
            currentElo = defaultElo;
            return;
        }

        StartCoroutine(FetchEloFromServer(username));
    }

    public IEnumerator FetchEloFromServer(string bigUsername)
    {
        string username = bigUsername.ToLower();
        string url = "https://airclashserver.onrender.com/getElo";

        EloRequestData requestData = new EloRequestData
        {
            username = username
        };

        string jsonPayload = JsonUtility.ToJson(requestData);

        using(UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("x-game-secret", GameConfig.ApiSecret);

            yield return www.SendWebRequest();

            if(www.result == UnityWebRequest.Result.Success)
            {
                EloResponseData res = JsonUtility.FromJson<EloResponseData>(www.downloadHandler.text);
                if(res.status == "success")
                {
                    currentElo = res.elo;
                    Debug.Log($"[MatchmakerScr] Актуальный ELO для {username} успешно получен: {currentElo}");
                }
                else
                {
                    Debug.LogError($"[MatchmakerScr] Ошибка сервера: {res.message}");
                }
            }
            else
            {
                Debug.LogError($"[MatchmakerScr] Сетевая ошибка при запросе ELO: {www.error}");
            }
        }
    }

    public void OnOpponentJoinedHost()
    {
        Debug.Log("[MatchmakerScr] Гость успешно подключился к нашему Хосту!");

        isSearching = false;

        if(matchmakingCoroutine != null) 
        {
            StopCoroutine(matchmakingCoroutine);
            matchmakingCoroutine = null;
        }

        OnMatchFound?.Invoke();
    }

    public void StartMatchmaking()
    {
        if(isSearching) return;

        string localEosId = GetLocalEosId();
        if(string.IsNullOrEmpty(localEosId))
        {
            Debug.LogWarning("[MatchmakerScr] Ошибка: Нельзя начать поиск, игрок не авторизован в EOS!");
            return;
        }

        isSearching = true;
        isHostCreated = false;
        currentRange = initialEloRange;

        OnMatchmakingStart?.Invoke();

        matchmakingCoroutine = StartCoroutine(SearchRoutine(localEosId));
        Debug.Log($"[MatchmakerScr] Поиск начат. Ваш Elo: {currentElo}");
    }

    public void CancelMatchmaking()
    {
        if(!isSearching && !isHostCreated) return;

        if(matchmakingCoroutine != null)
            StopCoroutine(matchmakingCoroutine);

        isSearching = false;

        string localEosId = GetLocalEosId();
        if(!string.IsNullOrEmpty(localEosId))
        {
            StartCoroutine(CancelSearchRoutine(localEosId));
        }

        if(isHostCreated)
        {
            MyNetworkManager.singleton.StopHost();
            isHostCreated = false;
        }

        OnMatchmakingCancel?.Invoke();
        Debug.Log("[MatchmakerScr] Поиск отменен.");
    }

    private IEnumerator SearchRoutine(string localEosId)
    {
        while(isSearching)
        {
            int minElo = Mathf.Max(0, currentElo - currentRange);
            int maxElo = currentElo + currentRange;

            OnSearchRangeUpdated?.Invoke(minElo, maxElo);
            Debug.Log($"[MatchmakerScr] Ищем соперника (Elo: {currentElo}, Допуск: ±{currentRange})...");

            yield return StartCoroutine(FindOpponentRoutine(localEosId, currentRange, (foundHostId) =>
            {
                if(!string.IsNullOrEmpty(foundHostId))
                {
                    Debug.Log($"[MatchmakerScr] Найден хост: {foundHostId}. Подключаемся...");
                    OnOpponentFound(foundHostId);
                }
            }));

            if(!isSearching) yield break;

            if(!isHostCreated)
            {
                yield return StartCoroutine(CreateRoomRoutine(localEosId));
            }

            yield return new WaitForSeconds(expandInterval);

            if(currentRange < maxEloRange)
            {
                currentRange += rangeExpandStep;
            }
        }
    }

    private IEnumerator FindOpponentRoutine(string localEosId, int maxDiff, Action<string> onResult)
    {
        string url = BASE_URL + "/mm/findOpponent";
        FindOpponentRequest requestData = new FindOpponentRequest
        {
            eos_id = localEosId,
            elo = currentElo,
            played_matches = currentPlayedMatches,
            max_elo_diff = maxDiff
        };

        string json = JsonUtility.ToJson(requestData);

        using(UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("x-game-secret", GameConfig.ApiSecret);

            yield return www.SendWebRequest();

            if(www.result == UnityWebRequest.Result.Success)
            {
                FindOpponentResponse res = JsonUtility.FromJson<FindOpponentResponse>(www.downloadHandler.text);

                if(res.status == "found")
                {
                    onResult?.Invoke(res.host_eos_id);
                    yield break;
                }
            }
            else
            {
                Debug.LogWarning($"[MatchmakerScr] Ошибка поиска комнат: {www.error}");
            }
        }

        onResult?.Invoke(null);
    }

    private IEnumerator CreateRoomRoutine(string localEosId)
    {
        string url = BASE_URL + "/mm/createRoom";
        CreateMMRoomRequest requestData = new CreateMMRoomRequest
        {
            eos_id = localEosId,
            elo = currentElo,
            played_matches = currentPlayedMatches 
        };

        string json = JsonUtility.ToJson(requestData);

        using(UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("x-game-secret", GameConfig.ApiSecret);

            yield return www.SendWebRequest();

            if(www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[MatchmakerScr] Комната поиска создана. Запускаем Mirror Host в ожидании подключения...");
                isHostCreated = true;
                MyNetworkManager.singleton.StartHost();
            }
            else
            {
                Debug.LogError($"[MatchmakerScr] Ошибка создания комнаты поиска: {www.error}");
            }
        }
    }

    private IEnumerator CancelSearchRoutine(string localEosId)
    {
        string url = BASE_URL + "/mm/cancelSearch";
        CancelSearchRequest requestData = new CancelSearchRequest { eos_id = localEosId };
        string json = JsonUtility.ToJson(requestData);

        using(UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("x-game-secret", GameConfig.ApiSecret);

            yield return www.SendWebRequest();
        }
    }

    public void OnOpponentFound(string hostEosId)
    {
        isSearching = false;
        if(matchmakingCoroutine != null) StopCoroutine(matchmakingCoroutine);

        Debug.Log("[MatchmakerScr] OnOpponentFound()");
        OnMatchFound?.Invoke();

        MyNetworkManager.singleton.networkAddress = hostEosId;
        MyNetworkManager.singleton.StartClient();
    }

    private string GetLocalEosId()
    {
        if(EOSSDKComponent.LocalUserProductId != null && EOSSDKComponent.LocalUserProductId.IsValid())
        {
            return EOSSDKComponent.LocalUserProductId.ToString();
        }
        return null;
    }
}