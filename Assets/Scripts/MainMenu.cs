using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject onlinePanel;
    [SerializeField] private GameObject botPanel;
    [SerializeField] private GameObject mentionsPanel;
    [SerializeField] private GameObject achievementsPanel;
    [SerializeField] private GameObject gamemodesPanel;
    [SerializeField] private GameObject userGamemodePanel;
    [SerializeField] private GameObject xpPanel;
    [SerializeField] private GameObject questPanel;
    [SerializeField] private GameObject dailyQuestPanel;
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip menuMusic;
    [Header("UI Elements")]
    [SerializeField] private Text mainMenuText;
    [SerializeField] private Text moneyText;
    [SerializeField] private Slider goalsSlider;
    [SerializeField] private Text goalsText;
    [Header("Scripts")]
    [SerializeField] private MoneyHandler moneyHandler;
    [SerializeField] private CoinMover coinMover;
    [SerializeField] private XpHandler xpHandler;
    [SerializeField] private QuestsHandler questsHandler; 
    [SerializeField] private DailyQuestHandler dailyQuestHandler;
    [Header("Floats")]
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float maxAngle = 6f;  
    private RectTransform rectTransform;
    private string toScene;

    void Start()
    {
        audioSource.clip = menuMusic;
        audioSource.loop = true;
        audioSource.time = PlayerPrefs.GetFloat("Music");
        audioSource.Play();
        Application.targetFrameRate = PlayerPrefs.GetInt("FPS", 60);
        audioSource.volume = PlayerPrefs.GetFloat("MusicVolume", 1.0f);
        rectTransform = mainMenuText.GetComponent<RectTransform>();
        moneyText.text = "Деньги " + moneyHandler.GetMoney();
        if(PlayerPrefs.GetInt("isAfterGame") == 0)
        {
            PlayerPrefs.SetInt("HowMoneyAdds", 0);
            PlayerPrefs.SetInt("HowXpAdds", 0);
            PlayerPrefs.SetInt("isAfterGame", 0);
            PlayerPrefs.Save();    
        } else
        {
            AddMoney(PlayerPrefs.GetInt("HowMoneyAdds"));
            UpdateQuests(PlayerPrefs.GetInt("HowMoneyAdds"));
            PlayerPrefs.SetInt("isAfterGame", 0);
            PlayerPrefs.SetInt("HowMoneyAdds", 0);
            PlayerPrefs.SetInt("HowXpAdds", 0);
            PlayerPrefs.Save();
        }
    }

    void Update()
    {
        float angle = Mathf.Sin(Time.time * rotationSpeed) * maxAngle;
        rectTransform.localRotation = Quaternion.Euler(0, 0, angle);
    }

    public void PlayBots(string difficulty)
    {
        switch(difficulty)
        {
            case "VeryEasy":
                PlayerPrefs.SetFloat("Difficulty", 3.1415926535f);
                PlayerPrefs.SetInt("HowMoneyAdd", 2);
                PlayerPrefs.SetInt("HowMoneyAddAsLose", 1);
                PlayerPrefs.SetInt("HowManyAddXp", 3);
                PlayerPrefs.SetFloat("BotOffsetX", 1f);
                PlayerPrefs.SetFloat("BotOffsetY", 0.8f);
                break;
            case "Easy":
                PlayerPrefs.SetFloat("Difficulty", 7.5f);
                PlayerPrefs.SetInt("HowMoneyAdd", 4);
                PlayerPrefs.SetInt("HowMoneyAddAsLose", 2);
                PlayerPrefs.SetInt("HowManyAddXp", 7);
                PlayerPrefs.SetFloat("BotOffsetX", 0.7f);
                PlayerPrefs.SetFloat("BotOffsetY", 0.7f);
                break;
            case "Medium":
                PlayerPrefs.SetFloat("Difficulty", 13.5f);
                PlayerPrefs.SetInt("HowMoneyAdd", 6);
                PlayerPrefs.SetInt("HowMoneyAddAsLose", 2);
                PlayerPrefs.SetInt("HowManyAddXp", 10);
                PlayerPrefs.SetFloat("BotOffsetX", 0.4f);
                PlayerPrefs.SetFloat("BotOffsetY", 0.5f);
                break;
            case "Hard":
                PlayerPrefs.SetFloat("Difficulty", 25f);
                PlayerPrefs.SetInt("HowMoneyAdd", 10);
                PlayerPrefs.SetInt("HowMoneyAddAsLose", 2);
                PlayerPrefs.SetInt("HowManyAddXp", 20);
                PlayerPrefs.SetFloat("BotOffsetX", 0.3f);
                PlayerPrefs.SetFloat("BotOffsetY", 0.2f);
                break;
            case "Extreme":
                PlayerPrefs.SetFloat("Difficulty", 50f);
                PlayerPrefs.SetInt("HowMoneyAdd", 20);
                PlayerPrefs.SetInt("HowMoneyAddAsLose", 1);
                PlayerPrefs.SetInt("HowManyAddXp", 30);
                PlayerPrefs.SetFloat("BotOffsetX", 0.15f);
                PlayerPrefs.SetFloat("BotOffsetY", 0.1f);
                break;
            default:
                PlayerPrefs.SetFloat("Difficulty", 2f);
                break;
        }
        PlayerPrefs.Save();
        toScene = "BotsGame";
        CloseAllPanels();
        gamemodesPanel.SetActive(true);
    }

    public void CloseAllPanels()
    {
        settingsPanel.SetActive(false);
        onlinePanel.SetActive(false);
        botPanel.SetActive(false);
        mentionsPanel.SetActive(false);
        achievementsPanel.SetActive(false);
        gamemodesPanel.SetActive(false);
        userGamemodePanel.SetActive(false);
        xpPanel.SetActive(false);
        questPanel.SetActive(false);
        dailyQuestPanel.SetActive(false);
    }
    public void ClosePanel(GameObject panel)
    {
        StartCoroutine(AnimateClosePanel(panel));
    }
    public void OpenPanel(GameObject panel)
    {
        var rect = panel.GetComponent<RectTransform>();
        rect.localScale = Vector3.zero;
        panel.SetActive(true);
        rect.DOScale(new Vector3(1.0f, 1.0f, 1.0f), 0.3f).SetEase(Ease.OutBack);
    }
    IEnumerator AnimateClosePanel(GameObject panel)
    {
        var rect = panel.GetComponent<RectTransform>();
        rect.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);
        yield return new WaitForSeconds(0.35f);
        panel.SetActive(false);
    }
    public void VeryEasyMode()
    {
        PlayBots("VeryEasy");
    }
    public void Easy()
    {
        PlayBots("Easy");
    }
    public void Normal()
    {
        PlayBots("Medium");
    }
    public void Hard()
    {
        PlayBots("Hard");
    }
    public void Extreme()
    {
        PlayBots("Extreme");
    }

    public void QuitGame()
    {
        PlayerPrefs.SetFloat("Music", 0);
        PlayerPrefs.Save();
        Application.Quit();
    }

    public void StartGame()
    {
        toScene = "GameScene";
    }

    public void OpenShop()
    {
        PlayerPrefs.SetFloat("Music", audioSource.time);
        PlayerPrefs.Save();
        SceneManager.LoadScene("ShopScene");
    }
    public void SwitchToQuestPanel()
    {
        questPanel.SetActive(true);
    }
    public void OpenDailyQuestPanel()
    {
        dailyQuestPanel.SetActive(true);
    }
    public void OpenUserGamemode()
    {
        CloseAllPanels();
        userGamemodePanel.SetActive(true);
        if(PlayerPrefs.GetInt("Goals") == 0) PlayerPrefs.SetInt("Goals", 5);
        else PlayerPrefs.SetInt("Goals", PlayerPrefs.GetInt("Goals"));
        goalsSlider.value = PlayerPrefs.GetInt("Goals");
        goalsText.text = Convert.ToString(PlayerPrefs.GetInt("Goals"));
    }

    public void AddMoney(int amount)
    {
        coinMover.AddCoins(new Vector3(0, 0, 0), amount);
    }
    public void SetGamemode(int howManyGoals)
    {
        PlayerPrefs.SetInt("Goals", howManyGoals);
        PlayerPrefs.Save();
        SceneManager.LoadScene(toScene);
    }
    public void SetUserGamemode()
    {
        PlayerPrefs.SetInt("Goals", Convert.ToInt32(goalsSlider.value));
        PlayerPrefs.Save();
        SceneManager.LoadScene(toScene);
    }
    private void UpdateQuests(int amount)
    {
        questsHandler.UpdateQuestProgress("money10", amount);
        questsHandler.UpdateQuestProgress("money50", amount);
        questsHandler.UpdateQuestProgress("money100", amount);
        questsHandler.UpdateQuestProgress("money200", amount);
        questsHandler.UpdateQuestProgress("money300", amount);
        questsHandler.UpdateQuestProgress("money500", amount);
        dailyQuestHandler.UpdateQuestProgress("money50", amount);
    }
}
