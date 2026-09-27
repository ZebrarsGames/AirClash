using System;
using System.Collections.Generic;
using Mirror;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [System.Serializable]
    public struct BotDifficultySettings
    {
        public float difficulty;
        public int moneyWin;
        public int moneyLose;
        public int xpAdd;
        public Vector2 botOffset;
    }

    [Header("Menu Panels")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject botPanel;
    [SerializeField] private GameObject mentionsPanel;
    [SerializeField] private GameObject achievementsPanel;
    [SerializeField] private GameObject gamemodesPanel;
    [SerializeField] private GameObject userGamemodePanel;
    [SerializeField] private GameObject xpPanel;
    [SerializeField] private GameObject questPanel;
    [SerializeField] private GameObject dailyQuestPanel;
    [SerializeField] private GameObject profilePanel;
    [SerializeField] private GameObject editProfilePanel;
    [SerializeField] private GameObject cloudPanel;
    [SerializeField] private GameObject modificatorsPanel;
    [SerializeField] private GameObject hostDisconnectedPanel;
    [SerializeField] private GameObject warningPanel;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip menuMusic;

    [Header("UI Elements")]
    [SerializeField] private RectTransform mainMenuTextRect;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private Slider goalsSlider;
    [SerializeField] private TextMeshProUGUI goalsText;
    [SerializeField] private Slider speedSlider;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private GameObject userGamemodeBtn;
    [SerializeField] private GameObject speedPanel;
    [SerializeField] private GameObject modificatorsMultiplyText;

    [Header("Scripts")]
    [SerializeField] private MoneyHandler moneyHandler;
    [SerializeField] private CoinMover coinMover;
    [SerializeField] private QuestsHandler questsHandler; 
    [SerializeField] private DailyQuestHandler dailyQuestHandler;
    [SerializeField] private SaveManager saveManager;

    [Header("Animation Settings")]
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float maxAngle = 6f; 
    [SerializeField] private float tweenDuration = 0.3f;

    private List<GameObject> _allPanels;
    private Tweener _wobbleTweener;

    private string _toScene = "GameScene";
    private string _lastOpenMenu;

    private const string SCENE_BOTS = "BotsGame";
    private const string SCENE_GAME = "GameScene";
    private const string SCENE_SHOP = "ShopScene";
    private const string SCENE_MULTIPLAYER = "MultiPlayerScene";

    private static readonly string[] QuestKeys = { "money10", "money50", "money100", "money200", "money300", "money500" };
    private static readonly string[] DailyQuestKeys = { "daily_money50", "money70", "daily_money100" };

    private readonly Dictionary<string, BotDifficultySettings> _difficulties = new()
    {
        { "VeryEasy", new BotDifficultySettings { difficulty = 5.0f, moneyWin = 2, moneyLose = 1, xpAdd = 3, botOffset = new Vector2(1.0f, 0.8f) } },
        { "Easy",     new BotDifficultySettings { difficulty = 7.5f, moneyWin = 3, moneyLose = 2, xpAdd = 7, botOffset = new Vector2(0.7f, 0.7f) } },
        { "Medium",   new BotDifficultySettings { difficulty = 12f,  moneyWin = 5, moneyLose = 2, xpAdd = 10, botOffset = new Vector2(0.4f, 0.5f) } },
        { "Hard",     new BotDifficultySettings { difficulty = 17.5f,moneyWin = 10, moneyLose = 2, xpAdd = 20, botOffset = new Vector2(0.3f, 0.2f) } },
        { "Extreme",  new BotDifficultySettings { difficulty = 27.5f,moneyWin = 20, moneyLose = 1, xpAdd = 30, botOffset = new Vector2(0.15f, 0.1f) } }
    };

    private void Awake()
    {
        CachePanels();
        HandleFirstLaunch();
    }

    private void Start()
    {
        Debug.Log("Версия до обновления Unity");
        InitAudio();
        InitMainMenuAnimation();
        InitGameStateAndMoney();
        CheckHostDisconnect();

        Application.targetFrameRate = PlayerPrefs.GetInt("FPS", 60);
    }

    private void OnDestroy()
    {
        _wobbleTweener?.Kill();
    }

    private void CachePanels()
    {
        _allPanels = new List<GameObject>
        {
            settingsPanel, botPanel, mentionsPanel, achievementsPanel, gamemodesPanel,
            userGamemodePanel, xpPanel, questPanel, dailyQuestPanel, profilePanel,
            editProfilePanel, cloudPanel, modificatorsPanel
        };
    }

    private void HandleFirstLaunch()
    {
        if(PlayerPrefs.GetInt("IsFirstTimePlayed", 1) == 1)
        {
            saveManager.SaveDefaultData();
            PlayerPrefs.SetInt("IsFirstTimePlayed", 0);
            PlayerPrefs.Save();
        }
    }

    private void InitAudio()
    {
        if(audioSource == null) return;

        audioSource.clip = menuMusic;
        audioSource.loop = true;
        audioSource.time = PlayerPrefs.GetFloat("MainMenuMusicTime", 0f);
        audioSource.Play();
    }

    private void InitMainMenuAnimation()
    {
        if(mainMenuTextRect == null) return;

        float duration = Mathf.PI / rotationSpeed;

        mainMenuTextRect.localRotation = Quaternion.Euler(0f, 0f, -maxAngle);

        _wobbleTweener = mainMenuTextRect.DOLocalRotate(new Vector3(0f, 0f, maxAngle), duration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetUpdate(UpdateType.Normal, true);
    }

    private void InitGameStateAndMoney()
    {
        if(moneyText != null && moneyHandler != null)
            moneyText.SetText($"{moneyHandler.GetMoney()} <sprite=0>");

        saveManager.SaveData();

        if(PlayerPrefs.GetInt("isAfterGame", 0) != 0)
        {
            int addedMoney = PlayerPrefs.GetInt("HowMoneyAdds", 0);
            AddMoney(addedMoney);
            UpdateQuests(addedMoney);
        }

        PlayerPrefs.SetInt("HowMoneyAdds", 0);
        PlayerPrefs.SetInt("HowXpAdds", 0);
        PlayerPrefs.SetInt("isAfterGame", 0);
        PlayerPrefs.Save();
    }

    private void CheckHostDisconnect()
    {
        bool isHostDisconnect = PlayerPrefs.GetInt("IsHostDisconnect", 0) != 0;

        if(!NetworkClient.active && !NetworkServer.active && isHostDisconnect)
        {
            PlayerPrefs.SetInt("IsHostDisconnect", 0);
            PlayerPrefs.Save();
            OpenPanel(hostDisconnectedPanel);
        }
    }

    public void CloseAllPanels()
    {
        for(int i = 0; i < _allPanels.Count; i++)
        {
            if(_allPanels[i] != null)
                _allPanels[i].SetActive(false);
        }
    }

    public void OpenPanel(GameObject panel)
    {
        if(panel == null) return;

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.DOKill();
        rect.localScale = Vector3.zero;
        panel.SetActive(true);
        rect.DOScale(Vector3.one, tweenDuration).SetEase(Ease.OutBack);
    }

    public void ClosePanel(GameObject panel)
    {
        if(panel == null) return;

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.DOKill();
        rect.DOScale(Vector3.zero, tweenDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                panel.SetActive(false);
                rect.localScale = Vector3.one;
            });
    }

    public void PlayBots(string difficultyKey)
    {
        if(_difficulties.TryGetValue(difficultyKey, out var settings))
        {
            PlayerPrefs.SetFloat("Difficulty", settings.difficulty);
            PlayerPrefs.SetInt("HowMoneyAdd", settings.moneyWin);
            PlayerPrefs.SetInt("HowMoneyAddAsLose", settings.moneyLose);
            PlayerPrefs.SetInt("HowManyAddXp", settings.xpAdd);
            PlayerPrefs.SetFloat("BotOffsetX", settings.botOffset.x);
            PlayerPrefs.SetFloat("BotOffsetY", settings.botOffset.y);
        }
        else
        {
            PlayerPrefs.SetFloat("Difficulty", 11.5f);
        }

        PlayerPrefs.Save();
        _toScene = SCENE_BOTS;
        
        CloseAllPanels();
        gamemodesPanel.SetActive(true);
        OnGamemodePanel();
    }

    public void VeryEasyMode() => PlayBots("VeryEasy");
    public void Easy()         => PlayBots("Easy");
    public void Normal()       => PlayBots("Medium");
    public void Hard()         => PlayBots("Hard");
    public void Extreme()      => PlayBots("Extreme");

    public void OnBotBtn() => _toScene = SCENE_BOTS;
    public void StartGame() => _toScene = SCENE_GAME;

    public void OnGamemodePanel()
    {
        _lastOpenMenu = "GamemodePanel";
        userGamemodeBtn.SetActive(_toScene == SCENE_GAME);
    }

    public void SwitchToGameModesPanel()
    {
        CloseAllPanels();
        if(_lastOpenMenu == "UserGameModePanel")
            OpenUserGamemode();
        else
        {
            gamemodesPanel.SetActive(true);
            OnGamemodePanel();
        }
    }

    public void OpenUserGamemode()
    {
        CloseAllPanels();
        userGamemodePanel.SetActive(true);
        _lastOpenMenu = "UserGameModePanel";

        int defaultGoals = PlayerPrefs.GetInt("Goals", 4);
        float defaultDifficulty = PlayerPrefs.GetFloat("Difficulty", 7.5f);

        goalsSlider.value = defaultGoals;
        speedSlider.value = defaultDifficulty;

        bool isBots = _toScene == SCENE_BOTS;
        speedPanel.SetActive(isBots);

        if(isBots)
        {
            speedText.text = $"{speedSlider.value:F1}";
            goalsText.text = defaultGoals.ToString();
        }
    }

    public void OnGoalsSliderChanged() => goalsText.SetText(goalsSlider.value.ToString("F0"));
    public void OnSpeedSliderChanged() => speedText.SetText(speedSlider.value.ToString("F1"));

    public void OpenShop()
    {
        if(audioSource != null)
            PlayerPrefs.SetFloat("MainMenuMusicTime", audioSource.time);

        PlayerPrefs.Save();
        SceneManager.LoadScene(SCENE_SHOP);
    }

    public void SetGamemode(int howManyGoals)
    {
        PlayerPrefs.SetInt("Goals", howManyGoals);
        LoadToScene();
    }

    public void SetUserGamemode()
    {
        PlayerPrefs.SetInt("Goals", Mathf.RoundToInt(goalsSlider.value));
        PlayerPrefs.SetFloat("Difficulty", speedSlider.value);
        LoadToScene();
    }

    public void LoadToScene()
    {
        PlayerPrefs.Save();
        SceneManager.LoadScene(_toScene);
    }

    private void LoadMultiplayer() => SceneManager.LoadScene(SCENE_MULTIPLAYER);

    public void SwitchToQuestPanel() => TogglePanels(dailyQuestPanel, questPanel);
    public void OpenDailyQuestPanel() => TogglePanels(questPanel, dailyQuestPanel);
    public void SwitchToEditProfilePanel() => TogglePanels(profilePanel, editProfilePanel);
    
    public void SwithcToProfilePanel()
    {
        editProfilePanel.SetActive(false);
        cloudPanel.SetActive(false);
        profilePanel.SetActive(true);
    }

    public void SwithcToCloudPanel() => TogglePanels(profilePanel, cloudPanel);

    public void SwithcToModificatorsPanel()
    {
        modificatorsMultiplyText.SetActive(_toScene == SCENE_BOTS);
        CloseAllPanels();
        modificatorsPanel.SetActive(true);
    }

    private void TogglePanels(GameObject hidePanel, GameObject showPanel)
    {
        if(hidePanel != null) hidePanel.SetActive(false);
        if(showPanel != null) showPanel.SetActive(true);
    }

    public void AddMoney(int amount)
    {
        if(coinMover != null)
            coinMover.AddCoins(Vector3.zero, amount);
    }

    public void OpenMultiplayer()
    {
        if(Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.LogWarning("Нельзя играть в мультиплеер без интернета!");
            OpenPanel(warningPanel);
            return;
        }
        LoadMultiplayer();
    }

    private void UpdateQuests(int amount)
    {
        if(questsHandler != null)
        {
            foreach(var key in QuestKeys)
                questsHandler.UpdateQuestProgress(key, amount);
        }

        if(dailyQuestHandler != null)
        {
            foreach(var key in DailyQuestKeys)
                dailyQuestHandler.UpdateQuestProgress(key, amount);
        }
    }
}