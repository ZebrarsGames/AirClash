using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System;
using System.Text;

public class LeaderboardHandler : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject leaderboardCardPrefab;
    [SerializeField] private GameObject loadingIndicator;
    [SerializeField] private TextMeshProUGUI emptyText;

    private const string LEADERBOARD_URL = "https://airclashserver.onrender.com/leaderboard/top100";
    private const string ELO_URL = "https://airclashserver.onrender.com/getElo";

    private void OnEnable()
    {
        StartCoroutine(FetchLeaderboardCoroutine());
    }

    private IEnumerator FetchLeaderboardCoroutine()
    {
        if(Application.internetReachability == NetworkReachability.NotReachable)
        {
            ShowEmpty("Нельзя прочитать лидерборд без интернета!");
            Debug.LogWarning("Нельзя прочитать лидерборд без интернета!");
            yield break;
        }

        if(loadingIndicator != null) loadingIndicator.SetActive(true);
        if(emptyText != null) emptyText.gameObject.SetActive(false);

        foreach(Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        using(UnityWebRequest www = UnityWebRequest.Get(LEADERBOARD_URL))
        {
            www.SetRequestHeader("x-game-secret", GameConfig.ApiSecret);
            www.timeout = 10;
            yield return www.SendWebRequest();

            if(loadingIndicator != null) loadingIndicator.SetActive(false);

            if(www.result == UnityWebRequest.Result.Success)
            {
                string json = www.downloadHandler.text;
                LeaderboardResponse response = JsonUtility.FromJson<LeaderboardResponse>(json);

                if(response != null && response.status == "success" && response.leaderboard != null && response.leaderboard.Length > 0)
                {
                    foreach(string username in response.leaderboard)
                    {
                        GameObject cardObj = Instantiate(leaderboardCardPrefab, contentParent);
                        LeaderboardCard card = cardObj.GetComponent<LeaderboardCard>();

                        if(card != null)
                        {
                            card.Setup(username, 0, Array.IndexOf(response.leaderboard, username) + 1);

                            StartCoroutine(GetPlayerEloRequest(username, elo =>
                            {
                                if (card != null)
                                {
                                    card.Setup(username, elo, Array.IndexOf(response.leaderboard, username) + 1);
                                }
                            }));
                        }
                    }

                    RectTransform rectTransform = contentParent.GetComponent<RectTransform>();
                    if(rectTransform != null)
                    {
                        float cardHeight = 200;
                        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, response.leaderboard.Length * cardHeight);
                    }
                }
                else
                {
                    ShowEmpty("Лидерборд пуст");
                }
            }
            else
            {
                ShowEmpty("Не удалось загрузить лидерборд");
                Debug.LogError($"[LeaderboardHandler] Ошибка запроса: {www.error}");
            }
        }
    }

    private IEnumerator GetPlayerEloRequest(string user, Action<int> onEloReceived)
    {
        EloRequestData data = new EloRequestData();
        data.username = user;
        string jsonPayload = JsonUtility.ToJson(data);

        using(UnityWebRequest www = new UnityWebRequest(ELO_URL, "POST"))
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
                Debug.LogWarning($"[LeaderboardHandler] Не удалось получить ELO для {user}: {www.error}");
                onEloReceived?.Invoke(0);
            }
            else
            {
                string json = www.downloadHandler.text;
                EloResponseData res = JsonUtility.FromJson<EloResponseData>(json);

                if(res != null && res.status == "success")
                {
                    onEloReceived?.Invoke(res.elo);
                }
                else
                {
                    onEloReceived?.Invoke(0);
                }
            }
        }
    }

    private void ShowEmpty(string message)
    {
        if(loadingIndicator != null) loadingIndicator.SetActive(false);
        if(emptyText != null)
        {
            emptyText.SetText(message);
            emptyText.gameObject.SetActive(true);
        }
    }
}

[Serializable]
public class LeaderboardResponse
{
    public string status;
    public int count;
    public string[] leaderboard;
}