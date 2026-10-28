using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Networking;
using System;
using System.Text;

public class FriendsUIHandler : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI myFriendCodeText;
    [SerializeField] private TextMeshProUGUI debugText;
    [SerializeField] private TextMeshProUGUI noFriendsText;
    [SerializeField] private TextMeshProUGUI noFriendsRequestsText;
    [SerializeField] private TMP_InputField friendCodeInputField;
    [SerializeField] private RectTransform friendsListContent;
    [SerializeField] private RectTransform friendsRequestsContent;

    [Header("Prefabs")]
    [SerializeField] private GameObject friendCardPrefab;

    [Header("Scripts")]
    [SerializeField] private FriendsHandler friendsHandler;

    public void Setup()
    {
        if(friendsHandler != null)
        {
            myFriendCodeText.SetText("Мой код: получение...");
            string currentUsernameBig = PlayerPrefs.GetString("Nick", "Ник");
            string currentUsername = currentUsernameBig.ToLower();
            friendsHandler.GetMyFriendCode(currentUsername, (code) => {
                myFriendCodeText.SetText($"Мой код: {code}");
            });

            foreach(Transform child in friendsListContent)
            {
                Destroy(child.gameObject);
            }

            foreach(Transform child in friendsRequestsContent)
            {
                Destroy(child.gameObject);
            }

            friendsHandler.GetFriendsList(currentUsername, (friends) => {
                if(friends == null || friends.Length == 0)
                {
                    noFriendsText.SetText("У вас пока нет друзей. Добавьте их по коду!");
                    noFriendsText.gameObject.SetActive(true);
                    return;
                }
                else
                {
                    noFriendsText.gameObject.SetActive(false);
                }
                foreach(var friend in friends)
                {
                    GameObject friendCard = Instantiate(friendCardPrefab, friendsListContent);
                    FriendCard card = friendCard.GetComponent<FriendCard>();
                    if(card != null)
                    {
                        int elo = 0;
                        bool isOnline = false;

                        StartCoroutine(GetPlayerEloRequest(friend, (currentElo) => {
                            elo = currentElo;
                            friendsHandler.CheckFriendOnline(friend, (onlineStatus) => {
                                isOnline = onlineStatus;
                                card.Setup(friend, elo, isOnline);
                                card.sendBattleInviteBtnText.SetText("Пригласить в бой");
                                card.sendBattleInviteBtn.onClick.AddListener(() => 
                                    friendsHandler.SendBattleInvite(currentUsername, friend, $"{currentUsernameBig} приглашает вас в бой!", (isComplete) => 
                                    { 
                                        if(isComplete) 
                                        { 
                                            debugText.SetText($"Приглашение в бой отправлено {friend}!"); 
                                        } 
                                        else 
                                        { 
                                            debugText.SetText($"Не удалось отправить приглашение в бой {friend}."); 
                                        } 
                                    })
                                );
                            });
                        }));
                    }
                }

                RectTransform rectTransform = friendsListContent;
                if(rectTransform != null)
                {
                    float cardHeight = 250;
                    rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, friends.Length * cardHeight);
                }
            });

            friendsHandler.GetPendingRequests(currentUsername, (pendingRequests) => {
                if(pendingRequests == null || pendingRequests.Length == 0)
                {
                    noFriendsRequestsText.SetText("У вас пока нет запросов в друзья");
                    noFriendsRequestsText.gameObject.SetActive(true);
                    return;
                }
                else
                {
                    noFriendsRequestsText.gameObject.SetActive(false);
                }
                foreach(var request in pendingRequests)
                {
                    GameObject friendCard = Instantiate(friendCardPrefab, friendsRequestsContent);
                    FriendCard card = friendCard.GetComponent<FriendCard>();
                    if(card != null)
                    {
                        int elo = 0;
                        bool isOnline = false;

                        StartCoroutine(GetPlayerEloRequest(request, (currentElo) => {
                            elo = currentElo;
                            friendsHandler.CheckFriendOnline(request, (onlineStatus) => {
                                isOnline = onlineStatus;
                                card.Setup(request, elo, isOnline);
                                card.sendBattleInviteBtnText.SetText("Принять запрос");
                                card.sendBattleInviteBtn.onClick.AddListener(() => friendsHandler.AcceptFriendRequest(currentUsername, request, (isComplete) => 
                                { 
                                    if(isComplete) 
                                    { 
                                        debugText.SetText($"Вы приняли запрос в друзья от {request}!"); 
                                    } 
                                    else 
                                    { 
                                        debugText.SetText($"Не удалось принять запрос в друзья от {request}."); 
                                    } 
                                }));
                            });
                        }));
                    }
                }

                RectTransform rectTransform = friendsRequestsContent;
                if(rectTransform != null)
                {
                    float cardHeight = 250;
                    rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, pendingRequests.Length * cardHeight);
                }
            });
        }
    }

    public void CopyFriendCodeToClipboard()
    {
        string friendCode = myFriendCodeText.text.Replace("Мой код: ", "");
        GUIUtility.systemCopyBuffer = friendCode;
        debugText.SetText("Код друга скопирован в буфер обмена!");
    }

    public void SendFriendRequest()
    {
        string friendCodeSmall = friendCodeInputField.text.Trim();
        string friendCode = friendCodeSmall.ToUpper();
        if(string.IsNullOrEmpty(friendCode))
        {
            debugText.SetText("Введите код друга.");
            return;
        }

        SendFriendRequestByCode(friendCode);
    }

    private void SendFriendRequestByCode(string friendCode)
    {
        debugText.SetText($"Отправка запроса в друзья...");
        friendsHandler.SendFriendRequestByCode(PlayerPrefs.GetString("Nick", "Ник").ToLower(), friendCode, (success, message) => {
            if(success)
            {
                debugText.SetText(message);
                Debug.Log(message);
            }
            else
            {
                debugText.SetText($"Ошибка: {message}");
                Debug.LogError($"Не удалось отправить запрос в друзья: {message}");
            }
        });
    }

    private IEnumerator GetPlayerEloRequest(string user, Action<int> onEloReceived)
    {
        EloRequestData data = new EloRequestData();
        data.username = user;
        string jsonPayload = JsonUtility.ToJson(data);

        using(UnityWebRequest www = new UnityWebRequest("https://airclashserver.onrender.com/getElo", "POST"))
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
                    Debug.Log($"[LeaderboardHandler] Получен ELO для {user}: {res.elo}");
                }
                else
                {
                    onEloReceived?.Invoke(0);
                }
            }
        }
    }
}
