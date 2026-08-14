using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using EpicTransport;
using Mirror;

public class MatchmakingManager : MonoBehaviour
{
    [Header("Elo Settings")]
    [SerializeField] private string eloKey = "Player_Elo";
    [SerializeField] private int defaultElo = 1000;

    [Header("Matchmaking Parameters")]
    [SerializeField] private int initialEloRange = 50;
    [SerializeField] private int rangeExpandStep = 50;
    [SerializeField] private float expandInterval = 5f;
    [SerializeField] private int maxEloRange = 500;

    private int currentElo;
    private int currentRange;
    private Coroutine matchmakingCoroutine;
    private bool isSearching = false;
    private LobbyInterface lobbyInterface;

    public UnityEvent<int, int> OnSearchRangeUpdated; // (minElo, maxElo)
    public UnityEvent OnMatchFound;

    void Start()
    {
        currentElo = PlayerPrefs.GetInt(eloKey, defaultElo);
        if(EOSBootstrap.PlatformHandle != null)
        {
            lobbyInterface = EOSBootstrap.PlatformHandle.GetLobbyInterface();
            Debug.Log("[Matchmaking] LobbyInterface готов к работе.");
        }
        else
        {
            Debug.LogError("[Matchmaking] PlatformHandle равен null! Платформа не инициализирована.");
        }
    }

    public void StartMatchmaking()
    {
        if(isSearching) return;

        isSearching = true;
        currentRange = initialEloRange;
        matchmakingCoroutine = StartCoroutine(SearchRoutine());
        Debug.Log($"[Matchmaking] Поиск начат. Ваш Elo: {currentElo}");
    }

    public void CancelMatchmaking()
    {
        if(!isSearching) return;

        if(matchmakingCoroutine != null)
            StopCoroutine(matchmakingCoroutine);

        isSearching = false;
        Debug.Log("[Matchmaking] Поиск отменен.");
    }

    private IEnumerator SearchRoutine()
    {
        while(isSearching)
        {
            int minElo = Mathf.Max(0, currentElo - currentRange);
            int maxElo = currentElo + currentRange;

            OnSearchRangeUpdated?.Invoke(minElo, maxElo);
            Debug.Log($"[Matchmaking] Ищем игрока с Elo от {minElo} до {maxElo} (Диапазон: ±{currentRange})");

            SearchMatch(minElo, maxElo);

            yield return new WaitForSeconds(expandInterval);

            if(currentRange < maxEloRange)
            {
                currentRange += rangeExpandStep;
            }
        }
    }

    public void CheckUserStatusBeforeSearch(ProductUserId puid)
    {
        var connectInterface = EOSBootstrap.PlatformHandle.GetConnectInterface();
        
        LoginStatus status = connectInterface.GetLoginStatus(puid);
        Debug.Log($"[EOS Connect] Статус авторизации пользователя {puid}: {status}");
        
        // Если статус != LoginStatus.LoggedIn, вызывать LobbySearch.Find нельзя!
    }

    public void SearchMatch(int minElo, int maxElo)
    {
        if(lobbyInterface == null)
        {
            Debug.LogError("[EOS] LobbyInterface не инициализирован!");
            return;
        }

        CreateLobbySearchOptions createSearchOptions = new CreateLobbySearchOptions
        {
            MaxResults = 10
        };

        Result result = lobbyInterface.CreateLobbySearch(ref createSearchOptions, out LobbySearch lobbySearch);

        if(result != Result.Success || lobbySearch == null)
        {
            Debug.LogError($"[EOS] Ошибка создания поиска: {result}");
            return;
        }

        LobbySearchSetParameterOptions minEloOptions = new LobbySearchSetParameterOptions
        {
            Parameter = new AttributeData
            {
                Key = "PlayerElo",
                Value = new AttributeDataValue { AsInt64 = minElo }
            },
            ComparisonOp = ComparisonOp.Greaterthanorequal
        };
        lobbySearch.SetParameter(ref minEloOptions);

        LobbySearchSetParameterOptions maxEloOptions = new LobbySearchSetParameterOptions
        {
            Parameter = new AttributeData
            {
                Key = "PlayerElo",
                Value = new AttributeDataValue { AsInt64 = maxElo }
            },
            ComparisonOp = ComparisonOp.Lessthanorequal
        };
        lobbySearch.SetParameter(ref maxEloOptions);

        ProductUserId localUserId = ProductUserId.FromString(EOSSDKComponent.LocalUserAccountIdString);
        if(localUserId == null || !localUserId.IsValid())
        {
            Debug.LogError($"[EOS] Ошибка: LocalUserId невалиден! PUID string: '{EOSSDKComponent.LocalUserProductIdString}'. Убедитесь, что прошли авторизацию Connect.");
            return;
        }
        CheckUserStatusBeforeSearch(localUserId);
        LobbySearchFindOptions findOptions = new LobbySearchFindOptions
        {
            LocalUserId = localUserId
        };

        lobbySearch.Find(ref findOptions, null, (ref LobbySearchFindCallbackInfo callbackInfo) =>
        {
            if(callbackInfo.ResultCode == Result.Success)
            {
                LobbySearchGetSearchResultCountOptions options = new LobbySearchGetSearchResultCountOptions();
                uint searchResultCount = lobbySearch.GetSearchResultCount(ref options);
                Debug.Log($"[EOS] Поиск завершен. Найдено лобби ({searchResultCount} штук)!");
            }
            else
            {
                Debug.LogWarning($"[EOS] Поиск не дал результатов: {callbackInfo.ResultCode}");
            }
        });
    }

    public void OnOpponentFound(string matchConnectCode)
    {
        CancelMatchmaking();
        OnMatchFound?.Invoke();
        
        NetworkManager.singleton.networkAddress = matchConnectCode;
        NetworkManager.singleton.StartClient();
    }
}