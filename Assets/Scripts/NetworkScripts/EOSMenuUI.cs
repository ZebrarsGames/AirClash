using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using EpicTransport;
using DG.Tweening;
using System.Threading.Tasks;

public class EOSMenuUI : MonoBehaviour
{
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private TMP_InputField idInputField;
    [SerializeField] private TextMeshProUGUI myIdText;
    [SerializeField] private RoomManager roomManager;
    [SerializeField] private GameObject uiPanel;
    [SerializeField] private GameObject placeholderPanel;
    [SerializeField] private GameObject warningPanel;
    [SerializeField] private RectTransform imageTransform;
    [SerializeField] private float rotationSpeed = 90f; 
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private float changeInterval = 5f;
    [SerializeField] private List<string> hints = new List<string>();

    private int lastHintIndex = -1;
    private WaitForSeconds delay;
    private Tweener _rotationTweener;
    private string currentEosId = string.Empty;

    private void Awake()
    {
        CheckMultiplayerRequirments();
        delay = new WaitForSeconds(changeInterval);
    }

    void Start()
    {
        var rect = myIdText.GetComponent<RectTransform>();
        rect.localPosition = new Vector3(0, -400, 0);
        rect.sizeDelta = new Vector3(825, 165);

        float duration = 360f / Mathf.Abs(rotationSpeed);

        float targetAngle = rotationSpeed > 0 ? -360f : 360f;

        _rotationTweener = imageTransform.DOLocalRotate(new Vector3(0f, 0f, targetAngle), duration, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Incremental)
            .SetEase(Ease.Linear)
            .SetUpdate(UpdateType.Normal, true);

        uiPanel.SetActive(true);
        placeholderPanel.SetActive(false);
        warningPanel.SetActive(false);
        myIdText.text = "Авторизация в Epic Games...";
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

        int newIndex = Random.Range(0, hints.Count);

        if(newIndex == lastHintIndex)
        {
            newIndex = (newIndex + 1) % hints.Count;
        }

        hintText.text = hints[newIndex];
        lastHintIndex = newIndex;
    }

    private IEnumerator WaitForEOSLoginRoutine()
    {
        while(!EOSSDKComponent.Initialized)
        {
            yield return new WaitForSeconds(0.2f);
        }

        while(EOSSDKComponent.LocalUserProductId == null || !EOSSDKComponent.LocalUserProductId.IsValid())
        {
            yield return new WaitForSeconds(0.2f);
        }

        currentEosId = EOSSDKComponent.LocalUserProductId.ToString();

        myIdText.text = "Мой EOS ID: " + currentEosId;
        Debug.Log($"[EOS Menu] Авторизация успешна. EOS ID: {currentEosId}");
    }

    public void StartHostGame()
    {
        if(EOSSDKComponent.LocalUserProductId == null || !EOSSDKComponent.LocalUserProductId.IsValid())
        {
            myIdText.text = "Подождите, идёт авторизация EOS...";
            Debug.LogWarning("Попытка создать комнату до завершения авторизации Epic!");
            return;
        }

        currentEosId = EOSSDKComponent.LocalUserProductId.ToString();
        myIdText.text = "Создание комнаты...";

        roomManager.CreateRoom(currentEosId, 
        (roomId) =>
        {
            myIdText.text = "Код комнаты: " + roomId;
            if(networkManager is MyNetworkManager customManager)
            {
                customManager.SetCurrentRoomCode(roomId);
            }
            Debug.Log($"Комната успешно создана на сервере! Код: {roomId}");
            ShowPlaceholder();
            var group = uiPanel.GetComponent<CanvasGroup>();
            group.DOFade(0, 1f).OnComplete(() => uiPanel.SetActive(false));
            
            networkManager.StartHost();
        },
        (errorText) =>
        {
            myIdText.text = "Ошибка создания: " + errorText;
            Debug.LogError($"Ошибка сервера Firebase: {errorText}");
        });
    }

    public void JoinClientGame()
    {
        string inputCode = idInputField.text.Trim().ToUpper();

        if(string.IsNullOrEmpty(inputCode))
        {
            myIdText.text = "Введите код комнаты!";
            return;
        }

        myIdText.text = "Поиск комнаты...";

        roomManager.JoinRoom(inputCode, 
        (roomEosId) =>
        {
            myIdText.text = "Подключение к " + inputCode + "...";
            Debug.Log($"Успешно получен EOS ID хоста: {roomEosId}");
            OnMatchFound();
            
            networkManager.networkAddress = roomEosId.Trim();
            networkManager.StartClient();
        },
        (errorText) =>
        {
            myIdText.text = "Комната не найдена: " + errorText;
            Debug.LogError($"Ошибка при поиске комнаты: {errorText}");
        });
    }

    public void DeleteRoom()
    {
        string roomCode;

        if(networkManager is MyNetworkManager customManager)
        {
            roomCode = customManager.GetCurrentRoomCode();
            myIdText.text = "Удаление комнаты...";

            roomManager.DeleteRoom(roomCode, 
            () =>
            {
                myIdText.text = $"Комната {roomCode} успешно удалена";
                Debug.Log($"Комната {roomCode} успешно удалена");
                HidePlaceholder();
                
                networkManager.StopClient();
                networkManager.StopHost();
            },
            (errorText) =>
            {
                myIdText.text = "Комната не найдена: " + errorText;
                Debug.LogError($"Ошибка при поиске комнаты: {errorText}");
            });
        }
    }

    private void ShowPlaceholder()
    {
        placeholderPanel.SetActive(true);
        var rect = myIdText.GetComponent<RectTransform>();
        rect.localPosition = new Vector3(-700, 26, 0);
        rect.sizeDelta = new Vector3(500, 370);
    }

    private void HidePlaceholder()
    {
        uiPanel.SetActive(true);
        var group = placeholderPanel.GetComponent<CanvasGroup>();
        group.DOFade(0, 1f).OnComplete(() => placeholderPanel.SetActive(false));
    }

    public void OnMatchFound()
    {
        HidePlaceholder();
        uiPanel.SetActive(false);
        myIdText.gameObject.SetActive(false);
    }

    public void OnMatchmakingStart()
    {
        ShowPlaceholder();
    }

    public void OnMatchmakingCancel()
    {
        HidePlaceholder();
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
}