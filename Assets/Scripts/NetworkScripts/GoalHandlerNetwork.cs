using DG.Tweening;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Text;
using UnityEngine.Networking;

public class GoalHandlerNetwork : NetworkBehaviour
{
    [System.Serializable]
    public class EloRequestData
    {
        public string username;
    }

    [System.Serializable]
    public class EloResponseData
    {
        public string status;
        public int elo;
        public string message;
    }

    [Serializable]
    public class MatchesRequestData
    {
        public string username;
    }

    [Serializable]
    public class MatchesResponseData
    {
        public string status;
        public string message;
        public int played_matches;
    }

    [System.Serializable]
    public class MatchStatsData
    {
        public string username;
        public string password;
        public int elo;
        public int played_matches;
    }

    [System.Serializable]
    public class MatchStatsResponse
    {
        public string status;
        public string message;
    }

        public static GoalHandlerNetwork Instance;

    [Header("UI Elements")]
    public TextMeshProUGUI scoreText1;
    public TextMeshProUGUI scoreText2;
    [SerializeField] private TextMeshProUGUI winOrLoseText;
    [SerializeField] private TextMeshProUGUI rematchButtonText;
    [SerializeField] private Button mainMenuBtn;
    [SerializeField] private Button rematchButton;
    [SerializeField] private GameObject goalTextCanvas;
    [SerializeField] private GameObject endSreenPanel;

    [Header("Players & Puck")]
    [SerializeField] private GameObject player1;
    [SerializeField] private GameObject player2;
    public GameObject puck;

    [Header("Positions")]
    private Vector2 player1startPos;
    private Vector2 player2startPos;
    private Vector2 puckStartPos;

    [Header("Game Logic & Scoring")]
    private int score1 = 0;
    private int score2 = 0;
    public int howManyGoals;
    [SerializeField] private TimerScr timer;

    [Header("Audio")]
    public AudioSource audioSourceSfx;
    public AudioSource audioSourceBgMusic;
    public AudioClip puckSound;
    public AudioClip StartGameSound;
    [SerializeField] private AudioClip[] gameMusics;

    [SyncVar(hook = nameof(OnPlayer1RematchChanged))]
    private bool player1Ready = false;

    [SyncVar(hook = nameof(OnPlayer2RematchChanged))]
    private bool player2Ready = false;

    private int playerARating, playerBRating;
    private int playerAMatches, playerBMatches;

    void Awake()
    {
        Instance = this;
        player1startPos = player1.transform.position;
        player2startPos = player2.transform.position;
        puckStartPos = puck.transform.position;
        player1.GetComponentInChildren<Light2D>().intensity = 0;
        player2.GetComponentInChildren<Light2D>().intensity = 0;
    }

    void Start()
    {        
        PlayerPrefs.SetInt("IsHostDisconnect", 1);
        timer.TimerStart();
        audioSourceSfx.PlayOneShot(StartGameSound);
        bool isMusic = PlayerPrefs.GetInt("BgMusicInGame", 1) != 0;
        if(isMusic)
        {
            int rand = UnityEngine.Random.Range(0, gameMusics.Length);
            audioSourceBgMusic.clip = gameMusics[rand];
            audioSourceBgMusic.loop = true;
            audioSourceBgMusic.time = 0;
            audioSourceBgMusic.Play();
        }
        howManyGoals = 4;
        puck.GetComponent<TrailRenderer>().enabled = PlayerPrefs.GetInt("PuckTrail", 1) != 0;
    }

    public void SetRating(string username, int playerIndex)
    {
        string smallUsername = username.ToLower();
        Debug.Log($"Set Rating for {smallUsername}, playerIndex: {playerIndex}");
        StartCoroutine(GetPlayerEloRequest(smallUsername, (elo) =>
        {
            if(playerIndex == 1) playerARating = elo;
            else if(playerIndex == 2) playerBRating = elo;
        }));
    }

    public void SetPlayedMatches(string username, int playerIndex)
    {
        string smallUsername = username.ToLower();
        Debug.Log($"Set PlayerMatches for {smallUsername}, playerIndex: {playerIndex}");
        StartCoroutine(GetPlayerMatchesRequest(smallUsername, (matches) =>
        {
            if(playerIndex == 1) playerAMatches = matches;
            else if(playerIndex == 2) playerBMatches = matches;
        }));
    }

    [Server] 
    public void ServerProcessGoal(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("GoalTrigger1"))
        {
            score1++;
        }
        else if(collision.gameObject.CompareTag("GoalTrigger2"))
        {
            score2++;
        }

        if(score1 >= howManyGoals)
        {
            playerAMatches++;
            playerBMatches++;
            (int newRatingA, int newRatingB) = EloSystemScr.CalculateNewRatings(
                playerBRating, playerARating, playerBMatches, playerAMatches, score2, score1
            );

            Debug.Log($"Игрок А: {playerARating} -> {newRatingA} (Изменение: {newRatingA - playerARating})");
            Debug.Log($"Игрок Б: {playerBRating} -> {newRatingB} (Изменение: {newRatingB - playerBRating})");

            RpcWinLose(1, newRatingA, newRatingB, playerAMatches, playerBMatches);
        } 
        else if(score2 >= howManyGoals)
        {
            playerAMatches++;
            playerBMatches++;
            (int newRatingA, int newRatingB) = EloSystemScr.CalculateNewRatings(
                playerBRating, playerARating, playerBMatches, playerAMatches, score2, score1
            );

            Debug.Log($"Игрок А: {playerARating} -> {newRatingA} (Изменение: {newRatingA - playerARating})");
            Debug.Log($"Игрок Б: {playerBRating} -> {newRatingB} (Изменение: {newRatingB - playerBRating})");

            RpcWinLose(2, newRatingA, newRatingB, playerAMatches, playerBMatches);
        } else
        {
            RpcOnGoalScored(score1, score2);
            ServerResetPosition();
        }
    }

    [ClientRpc]
    private void RpcOnGoalScored(int newScore1, int newScore2)
    {
        Debug.Log($"Сервер сообщил: Забит гол! Текущий счет: {newScore1} {newScore2}");
        scoreText1.text = newScore1.ToString(); 
        scoreText2.text = newScore2.ToString(); 
        timer.Goal();
    }

    [ClientRpc]
    private void RpcWinLose(int playerIndex, int newRatingA, int newRatingB, int matchesA, int matchesB)
    {
        if(!isClientOnly)
        {
            PlayerPrefs.SetInt("MyElo", newRatingA);
            PlayerPrefs.SetInt("MyMatches", matchesA);
            StartCoroutine(SendMatchStatsRequest(PlayerPrefs.GetString("Nick", "Ник"), PlayerPrefs.GetString("AccountPassword", ""), newRatingA, matchesA));
        }
        else
        {
            PlayerPrefs.SetInt("MyElo", newRatingB);
            PlayerPrefs.SetInt("MyMatches", matchesB);
            StartCoroutine(SendMatchStatsRequest(PlayerPrefs.GetString("Nick", "Ник"), PlayerPrefs.GetString("AccountPassword", ""), newRatingB, matchesB));
        }

        PlayerPrefs.Save();

        if(playerIndex == 1)
        {
            if(isClientOnly)
            {
                Lose();
            } 
            else
            {
            Win();
            }
        } 
        else if(playerIndex == 2)
        {
            if(isClientOnly)
            {
                Lose();
            } 
            else
            {
                Win();
            }
        }
    }

    [ClientRpc]
    private void RpcStartTimer()
    {
        timer.TimerStart();
    }

    [Command]
    public void CmdRequestStartTimer()
    {
        RpcStartTimer();
    }

    public void RegisterPlayer(GameObject player, string name)
    {
        CmdRequestStartTimer();
        if(name == "Player1")
        {
            player1 = player;
        }
        else if(name == "Player2")
        {
            player2 = player;
        }
    }

    public void OnPuckCollisionEnter2D(Collision2D collision) 
    {
        if(!(collision.gameObject.name.Equals("Player1") || collision.gameObject.name.Equals("Player2")))
        {
            audioSourceSfx.PlayOneShot(puckSound);
        }
    }

    [Server]
    private void ServerResetPosition()
    {
        ResetRigidBody(puck, puckStartPos);
        ResetRigidBody(player1, player1startPos);
        ResetRigidBody(player2, player2startPos);

        RpcResetPositions(puckStartPos, player1startPos, player2startPos);
    }

    private void ResetRigidBody(GameObject go, Vector2 targetPos)
    {
        if(go == null) return;
        
        if(go.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.position = targetPos;
            rb.transform.position = targetPos;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        if(go.TryGetComponent<PlayersControllerNetwork>(out var controller))
        {
            controller.ResetTargetPosition(targetPos);
        }
    }

    [ClientRpc]
    private void RpcResetPositions(Vector2 puckPos, Vector2 p1Pos, Vector2 p2Pos)
    {
        if(puck != null)
        {
            if(puck.TryGetComponent<PuckScrNetwork>(out var puckNetwork))
            {
                puckNetwork.ClientForceReset(puckPos);
            }
            else if(puck.TryGetComponent<Rigidbody2D>(out var puckRb))
            {
                puckRb.position = puckPos;
                puckRb.transform.position = puckPos;
                puckRb.linearVelocity = Vector2.zero;
            }
        }

        if(player1 != null)
        {
            ResetClientPlayer(player1, p1Pos);
        }

        if(player2 != null)
        {
            ResetClientPlayer(player2, p2Pos);
        }
    }

    private void ResetClientPlayer(GameObject playerGo, Vector2 targetPos)
    {
        if(playerGo.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.position = targetPos;
            rb.transform.position = targetPos;
            rb.linearVelocity = Vector2.zero;
        }

        if(playerGo.TryGetComponent<PlayersControllerNetwork>(out var controller))
        {
            controller.ResetTargetPosition(targetPos);
        }
    }

    public void OnRematchButtonClicked()
    {
        int playerNumber = isServer ? 1 : 2;
        CmdRequestRematch(playerNumber);
    }

    [Command(requiresAuthority = false)]
    private void CmdRequestRematch(int playerNumber)
    {
        if(playerNumber == 1) player1Ready = true;
        if(playerNumber == 2) player2Ready = true;

        if(player1Ready && player2Ready)
        {
            player1Ready = false;
            player2Ready = false;
            
            score1 = 0;
            score2 = 0;
            
            ServerResetPosition(); 

            RpcRestartGame();
        }
    }

    private void OnPlayer1RematchChanged(bool oldVal, bool newVal)
    {
        UpdateRematchUI();
    }

    private void OnPlayer2RematchChanged(bool oldVal, bool newVal)
    {
        UpdateRematchUI();
    }

    private void UpdateRematchUI()
    {
        bool iAmServer = isServer;
        
        if(iAmServer)
        {
            if(player1Ready && !player2Ready)
            {
                rematchButtonText.text = "Ожидание соперника...";
                mainMenuBtn.interactable = false;
                rematchButton.interactable = false;
            } 
            else if(!player1Ready && player2Ready) rematchButtonText.text = "Соперник хочет реванш!";
            else rematchButtonText.text = "Реванш";
        }
        else
        {
            if(player2Ready && !player1Ready)
            {
                rematchButtonText.text = "Ожидание соперника...";
                mainMenuBtn.interactable = false;
                rematchButton.interactable = false;
            }
            else if(!player2Ready && player1Ready) rematchButtonText.text = "Соперник хочет реванш!";
            else rematchButtonText.text = "Реванш";
        }
    }

    [ClientRpc]
    private void RpcRestartGame()
    {
        scoreText1.text = "0";
        scoreText2.text = "0";
        
        if(audioSourceSfx && StartGameSound)
            audioSourceSfx.PlayOneShot(StartGameSound);
            
        timer.TimerStart();
        
        rematchButtonText.text = "Реванш"; 
        mainMenuBtn.interactable = true;
        rematchButton.interactable = true;
        endSreenPanel.SetActive(false);
    }

    public void Win()
    {
        goalTextCanvas.SetActive(true);
        var rect = endSreenPanel.GetComponent<RectTransform>();
        rect.localScale = Vector3.zero;
        endSreenPanel.SetActive(true);
        rect.DOScale(new Vector3(1.0f, 1.0f, 1.0f), 0.3f).SetEase(Ease.OutBack);
        winOrLoseText.text = "Победа!";
    }
    public void Lose()
    {
        goalTextCanvas.SetActive(true);
        var rect = endSreenPanel.GetComponent<RectTransform>();
        rect.localScale = Vector3.zero;
        endSreenPanel.SetActive(true);
        rect.DOScale(new Vector3(1.0f, 1.0f, 1.0f), 0.3f).SetEase(Ease.OutBack);
        winOrLoseText.text = "Поражение!";
    }
    public void LoadMainMenu()
    {
        PlayerPrefs.SetInt("IsHostDisconnect", 0);
        PlayerPrefs.Save();
        MyNetworkManager.singleton.StopHost();
        MyNetworkManager.singleton.StopClient();
        SceneManager.LoadScene("MainMenu");
    }

    IEnumerator GetPlayerEloRequest(string user, Action<int> onEloReceived)
    {   
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
                }
            }
        }
    }

    IEnumerator GetPlayerMatchesRequest(string user, Action<int> onMatchesReceived)
    {   
        string url = "https://airclashserver.onrender.com/getPlayedMatches";

        MatchesRequestData data = new MatchesRequestData();
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
                onMatchesReceived?.Invoke(-1);
            }
            else
            {
                MatchesResponseData res = JsonUtility.FromJson<MatchesResponseData>(www.downloadHandler.text);
                
                if(res.status == "success")
                {
                    Debug.Log($"Матчи успешно получены для {user}: {res.played_matches}");
                    onMatchesReceived?.Invoke(res.played_matches);
                }
                else
                {
                    Debug.LogWarning($"Сервер вернул ошибку: {res.message}");
                    onMatchesReceived?.Invoke(-1);
                }
            }
        }
    }

    IEnumerator SendMatchStatsRequest(string bigUser, string pass, int eloValue, int playedMatchesValue)
    {
        string url = "https://airclashserver.onrender.com/saveMatchStats";
        string user = bigUser.Trim().ToLower();

        MatchStatsData data = new MatchStatsData
        {
            username = user,
            password = pass,
            elo = eloValue,
            played_matches = playedMatchesValue
        };

        string jsonPayload = JsonUtility.ToJson(data);

        using(UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();

            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("x-game-secret", GameConfig.ApiSecret);

            yield return www.SendWebRequest();

            if(www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"HTTP {www.responseCode} | Ответ сервера: {www.downloadHandler.text}");
            }
            else
            {
                MatchStatsResponse res = JsonUtility.FromJson<MatchStatsResponse>(www.downloadHandler.text);
                Debug.Log($"Статистика матча успешно сохранена! ELO: {eloValue}, Матчей: {playedMatchesValue}");
            }
        }
    }
}
