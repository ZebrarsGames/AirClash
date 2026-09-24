using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using EpicTransport;
using DG.Tweening;
using System.Threading.Tasks;
using System;
using System.Text;
using UnityEngine.Networking;

public enum TypeOfGame
{
    matchmaking,
    roomCode
}

public class EOSMenuUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject uiPanel;
    [SerializeField] private GameObject placeholderPanel;
    [SerializeField] private GameObject warningPanel;
    [SerializeField] private GameObject matchmakingPanel;
    [SerializeField] private GameObject roomPanel;

    [Header("UI")]
    [SerializeField] private TMP_InputField roomCodeInputfield;
    [SerializeField] private TextMeshProUGUI debugText;
    [SerializeField] private TextMeshProUGUI roomCodeText;
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private RectTransform imageTransform;
    [SerializeField] private TextMeshProUGUI eloText;

    [Header("Floats")]
    [SerializeField] private float rotationSpeed = 90f; 
    [SerializeField] private float hintChangeInterval = 5f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip bgMusic;

    [Header("Other")]
    [SerializeField] private RoomManager roomManager;
    [SerializeField] private List<string> hints = new List<string>();

    private int lastHintIndex = -1;
    private WaitForSeconds delay;
    private Tweener _rotationTweener;
    private string currentEosId = string.Empty;
    public static TypeOfGame typeOfCurrentGame;

    private void Awake()
    {
        CheckMultiplayerRequirments();
        delay = new WaitForSeconds(hintChangeInterval);
    }

    void Start()
    {
        MyNetworkManager.singleton.StopClient();
        MyNetworkManager.singleton.StopHost();
        float duration = 360f / Mathf.Abs(rotationSpeed);

        float targetAngle = rotationSpeed > 0 ? -360f : 360f;

        _rotationTweener = imageTransform.DOLocalRotate(new Vector3(0f, 0f, targetAngle), duration, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Incremental)
            .SetEase(Ease.Linear)
            .SetUpdate(UpdateType.Normal, true);

        uiPanel.SetActive(true);
        placeholderPanel.SetActive(false);
        warningPanel.SetActive(false);
        matchmakingPanel.SetActive(false);
        roomPanel.SetActive(false);
        debugText.gameObject.SetActive(true);
        roomCodeText.gameObject.SetActive(false);
        audioSource.loop = true;
        audioSource.clip = bgMusic;
        audioSource.loop = true;
        audioSource.Play();
        debugText.text = "Авторизация в Epic Games...";
        StartCoroutine(GetPlayerEloRequest(PlayerPrefs.GetString("Nick", "Ник"), (elo) => eloText.text = $"Ваш эло: {elo}"));
        StartCoroutine(WaitForEOSLoginRoutine());
    }

    private void OnEnable()
    {
        if(hints == null || hints.Count == 0) return;

        ShowRandomHint();
        StartCoroutine(ChangeHintRoutine());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public void OnInputField(GameObject panel)
    {
        panel.GetComponent<RectTransform>().DOLocalMoveY(228, 0.3f).SetEase(Ease.OutSine);
    }

    public void OnInputFieldEnd(GameObject panel)
    {
        panel.GetComponent<RectTransform>().DOLocalMoveY(0, 0.3f).SetEase(Ease.OutSine);
    }

    private IEnumerator ChangeHintRoutine()
    {
        while(true)
        {
            yield return delay;
            ShowRandomHint();
        }
    }

    private void ShowRandomHint()
    {
        if(hints.Count == 1)
        {
            hintText.text = hints[0];
            return;
        }

        int newIndex = UnityEngine.Random.Range(0, hints.Count);

        if(newIndex == lastHintIndex)
        {
            newIndex = (newIndex + 1) % hints.Count;
        }

        hintText.text = hints[newIndex];
        lastHintIndex = newIndex;
    }

    private IEnumerator WaitForEOSLoginRoutine()
    {
        float timeout = 15f;
        float timer = 0f;

        while(!EOSSDKComponent.Initialized)
        {
            timer += 0.2f;
            if(timer > timeout)
            {
                debugText.text = "Ошибка: EOS SDK не инициализировался!";
                yield break; 
            }
            yield return new WaitForSeconds(0.2f);
        }

        timer = 0f;
        while(EOSSDKComponent.LocalUserProductId == null || !EOSSDKComponent.LocalUserProductId.IsValid())
        {
            timer += 0.2f;
            if(timer > timeout)
            {
                debugText.text = "Ошибка: Не удалось получить PUID от Epic (Таймаут)";
                Debug.LogError("[EOS Menu] Epic не вернул PUID. Проверьте Logcat!");
                
                yield break;
            }
            yield return new WaitForSeconds(0.2f);
        }

        currentEosId = EOSSDKComponent.LocalUserProductId.ToString();
        debugText.text = "Мой EOS ID: " + currentEosId;
        Debug.Log($"[EOS Menu] Авторизация успешна. EOS ID: {currentEosId}");
    }

    public void StartHostGame()
    {
        if(EOSSDKComponent.LocalUserProductId == null || !EOSSDKComponent.LocalUserProductId.IsValid())
        {
            debugText.text = "Подождите, идёт авторизация EOS...";
            Debug.LogWarning("Попытка создать комнату до завершения авторизации Epic!");
            return;
        }

        currentEosId = EOSSDKComponent.LocalUserProductId.ToString();
        debugText.text = "Создание комнаты...";

        roomManager.CreateRoom(currentEosId, 
        (roomId) =>
        {
            if(MyNetworkManager.singleton == null)
            {
                var foundManager = FindAnyObjectByType<MyNetworkManager>();
                if(foundManager != null)
                {
                    foundManager.gameObject.SetActive(true);
                }
                else
                {
                    debugText.text = "Ошибка: Сетевой менеджер не найден!";
                    Debug.LogError("Критическая ошибка: Компонент MyNetworkManager отсутствует на сцене!");
                    return;
                }
            }
            else if(!MyNetworkManager.singleton.gameObject.activeInHierarchy)
            {
                MyNetworkManager.singleton.gameObject.SetActive(true);
            }
            roomCodeText.text = "Код комнаты: " + roomId;
            if(MyNetworkManager.singleton is MyNetworkManager customManager)
            {
                customManager.SetCurrentRoomCode(roomId);
            }
            Debug.Log($"Комната успешно создана на сервере! Код: {roomId}");
            typeOfCurrentGame = TypeOfGame.roomCode;
            roomCodeText.gameObject.SetActive(true);
            ShowPlaceholder();
            ClosePanel(uiPanel);
            
            MyNetworkManager.singleton.StartHost();
        },
        (errorText) =>
        {
            debugText.text = "Ошибка создания: " + errorText;
            Debug.LogError($"Ошибка сервера Firebase: {errorText}");
        });
    }

    public void JoinClientGame()
    {
        string inputCode = roomCodeInputfield.text.Trim().ToUpper();

        if(string.IsNullOrEmpty(inputCode))
        {
            debugText.text = "Введите код комнаты!";
            return;
        }

        debugText.text = "Поиск комнаты...";

        roomManager.JoinRoom(inputCode, 
        (roomEosId) =>
        {
            debugText.text = "Подключение к " + inputCode + "...";
            Debug.Log($"Успешно получен EOS ID хоста: {roomEosId}");
            OnMatchFound();

            if(MyNetworkManager.singleton == null)
            {
                var foundManager = FindAnyObjectByType<MyNetworkManager>();
                if (foundManager != null)
                {
                    foundManager.gameObject.SetActive(true);
                }
                else
                {
                    debugText.text = "Ошибка: Сетевой менеджер не найден!";
                    Debug.LogError("Критическая ошибка: Компонент MyNetworkManager отсутствует на сцене!");
                    return;
                }
            }
            else if(!MyNetworkManager.singleton.gameObject.activeInHierarchy)
            {
                MyNetworkManager.singleton.gameObject.SetActive(true);
            }

            typeOfCurrentGame = TypeOfGame.roomCode;
            MyNetworkManager.singleton.networkAddress = roomEosId.Trim();
            MyNetworkManager.singleton.StartClient();
        },
        (errorText) =>
        {
            debugText.text = "Комната не найдена: " + errorText;
            Debug.LogError($"Ошибка при поиске комнаты: {errorText}");
        });
    }

    public void DeleteRoom()
    {
        string roomCode;

        if(MyNetworkManager.singleton is MyNetworkManager customManager)
        {
            roomCode = customManager.GetCurrentRoomCode();
            debugText.text = "Удаление комнаты...";

            roomManager.DeleteRoom(roomCode, 
            () =>
            {
                debugText.text = $"Комната {roomCode} успешно удалена";
                Debug.Log($"Комната {roomCode} успешно удалена");
                HidePlaceholder();

                if(MyNetworkManager.singleton == null)
                {
                    var foundManager = FindAnyObjectByType<MyNetworkManager>();
                    if (foundManager != null)
                    {
                        foundManager.gameObject.SetActive(true);
                    }
                    else
                    {
                        debugText.text = "Ошибка: Сетевой менеджер не найден!";
                        Debug.LogError("Критическая ошибка: Компонент MyNetworkManager отсутствует на сцене!");
                        return;
                    }
                }
                else if(!MyNetworkManager.singleton.gameObject.activeInHierarchy)
                {
                    MyNetworkManager.singleton.gameObject.SetActive(true);
                }
                
                MyNetworkManager.singleton.StopClient();
                MyNetworkManager.singleton.StopHost();
            },
            (errorText) =>
            {
                debugText.text = "Комната не найдена: " + errorText;
                Debug.LogError($"Ошибка при поиске комнаты: {errorText}");
            });
        }
    }

    private void ShowPlaceholder()
    {
        placeholderPanel.SetActive(true);
        debugText.gameObject.SetActive(false);
    }

    private void HidePlaceholder()
    {
        matchmakingPanel.SetActive(false);
        roomPanel.SetActive(false);
        debugText.gameObject.SetActive(true);
        roomCodeText.gameObject.SetActive(false);
        uiPanel.SetActive(true);
        ClosePanel(placeholderPanel);
    }

    public void OnMatchFound()
    {
        HidePlaceholder();
        uiPanel.SetActive(false);
        debugText.gameObject.SetActive(false);
    }

    public void OnMatchmakingStart()
    {
        typeOfCurrentGame = TypeOfGame.matchmaking;
        ShowPlaceholder();
    }

    public void OnMatchmakingCancel()
    {
        HidePlaceholder();
    }

    public void OpenPanel(GameObject panel)
    {
        if(panel == null) return;

        CanvasGroup group = panel.GetComponent<CanvasGroup>();
        group.DOKill();
        group.alpha = 0;
        panel.SetActive(true);
        group.DOFade(1, 0.3f);
    }

    public void ClosePanel(GameObject panel)
    {
        if(panel == null) return;

        CanvasGroup group = panel.GetComponent<CanvasGroup>();
        group.DOKill();
        group.alpha = 1;
        group.DOFade(0, 0.3f).OnComplete(() => panel.SetActive(false));
    }

    public void OpenMainMenu()
    {
        MyNetworkManager.singleton.StopHost();
        MyNetworkManager.singleton.StopClient();
        PlayerPrefs.Save();
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    private Task<bool> GetIsExists(string username) 
    {
        return FirebaseManager.CheckUserExistsAsync(username);
    }

    private async void CheckMultiplayerRequirments()
    {
        if(Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.LogWarning("Нельзя играть в мультиплеер без интернета!");
            var rect = warningPanel.GetComponent<RectTransform>();
            rect.localScale = Vector3.zero;
            warningPanel.SetActive(true);
            rect.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);

            await Task.Delay(5000);

            MyNetworkManager.singleton.StopClient();
            MyNetworkManager.singleton.StopHost();
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
        bool isAccountExists = await GetIsExists(PlayerPrefs.GetString("Nick", "Ник"));
        if(!isAccountExists)
        {
            Debug.LogWarning("Нельзя играть в мультиплеер без аккаунта!");
            var rect = warningPanel.GetComponent<RectTransform>();
            rect.localScale = Vector3.zero;
            warningPanel.SetActive(true);
            rect.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);

            await Task.Delay(5000);

            MyNetworkManager.singleton.StopClient();
            MyNetworkManager.singleton.StopHost();
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }

        await Task.Delay(10000);

        if(!CheckEOSID()) UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    private bool CheckEOSID()
    {
        if(EOSSDKComponent.LocalUserProductId == null || !EOSSDKComponent.LocalUserProductId.IsValid())
        {
            return false;
        } else
        {
            return true;
        }
    }

    IEnumerator GetPlayerEloRequest(string bigUser, Action<int> onEloReceived)
    {   
        string user = bigUser.ToLower();
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
}