using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using TMPro;

public class GoalHandler : MonoBehaviour
{
    [Header("Constants & Settings")]
    private const float FOG_FADE_TIME = 3f;
    private const float END_SCREEN_ANIM_TIME = 0.3f;
    private const float WIND_UPDATE_INTERVAL = 5f;
    private const float MOD_BIG_SCALE = 2.0f;
    private const float MOD_SMALL_SCALE = 0.6f;
    private const float PUCK_BIG_SCALE = 1.1f;
    private const float PUCK_SMALL_SCALE = 0.45f;
    private const float PUCK_MAX_SPEED_X2 = 40f;
    private const string BOTS_SCENE_NAME = "BotsGame";

    private readonly string[] scoreStrings = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20" };
    private readonly Color DEFAULT_CAM_COLOR = new Color(0f, 0.243f, 0.6f); // #003E99

    [Header("UI Elements")]
    public TextMeshProUGUI scoreText1;
    public TextMeshProUGUI scoreText2;
    [SerializeField] private TextMeshProUGUI goalText;
    [SerializeField] private GameObject endSreenPanel;

    [Header("Players & Puck")]
    [SerializeField] private GameObject player1;
    [SerializeField] private GameObject player2;
    public GameObject puck;
    [SerializeField] private BotsAI botsAI;

    private Rigidbody2D puckRb;
    private PlayersController p1Controller;
    private PlayersController p2Controller;
    private Rigidbody2D p1Rb;
    private Rigidbody2D p2Rb;
    private Light2D p1Light;
    private Light2D p2Light;

    [Header("Positions")]
    private Vector2 player1startPos;
    private Vector2 player2startPos;
    private Vector2 puckStartPos;

    [Header("Game Logic & Scoring")]
    public int score1 = 0;
    public int score2 = 0;
    public int howManyGoals;
    [SerializeField] private TimerScr timer;
    [SerializeField] private EndScreen endScreen;
    
    // Оптимизация: вместо string храним ссылку на последний объект
    private GameObject lastCollisionGO; 
    private bool isBotsGame;
    private float currentDifficulty;

    [Header("Audio")]
    public AudioSource audioSourceSfx;
    public AudioSource audioSourceBgMusic;
    public AudioClip puckSound;
    public AudioClip StartGameSound;
    [SerializeField] private AudioClip[] gameMusics;

    [Header("Effects")]
    public GameObject particlePrefab;
    private bool isWind;

    [Header("Economy & Achievements")]
    public MoneyHandler moneyHandler;
    public int howMoneyAdd;
    private int howMoneyAddAsLose;
    private float _nextUpdate;
    [SerializeField] private AchievementsHandler achievementsHandler;

    [Header("Xp Logic")]
    [SerializeField] private XpHandler xpHandler;
    private int howManyXpAddAsWin;
    private int howManyXpAddForGoal;
    private int howManyXpAddAsLose;
    private int totalXpEarned;

    [Header("Quests")]
    [SerializeField] private DailyQuestHandler dailyQuestHandler;
    [SerializeField] private QuestsHandler questsHandler;

    [Header("Modificators")]
    [SerializeField] private Light2D mainLight2D;
    [SerializeField] private GameObject[] additionalWalls;
    [SerializeField] private AreaEffector2D areaEffector2D;
    private float modificatorsMoney = 1;
    private bool isFog = false;
    private bool isBigPlayer = false;
    private bool isSmallPlayer = false;
    private Camera mainCamera;

    void Awake()
    {
        mainCamera = Camera.main; 
        isBotsGame = SceneManager.GetActiveScene().name.Equals(BOTS_SCENE_NAME);
        
        puckRb = puck.GetComponent<Rigidbody2D>();
        p1Controller = player1.GetComponent<PlayersController>();
        p2Controller = player2.GetComponent<PlayersController>();
        p1Rb = player1.GetComponent<Rigidbody2D>();
        p2Rb = player2.GetComponent<Rigidbody2D>();
        p1Light = player1.GetComponentInChildren<Light2D>();
        p2Light = player2.GetComponentInChildren<Light2D>();

        player1startPos = player1.transform.position;
        player2startPos = player2.transform.position;
        puckStartPos = puck.transform.position;

        mainLight2D.intensity = 1.0f;
        if(p1Light != null) p1Light.intensity = 0;
        if(p2Light != null) p2Light.intensity = 0;

        areaEffector2D.gameObject.SetActive(false);
        isWind = false;
        isFog = false;

        mainCamera.backgroundColor = DEFAULT_CAM_COLOR;

        for(int i = 0; i < additionalWalls.Length; i++)
        {
            additionalWalls[i].SetActive(false);
        }
    }

    void Start()
    {      
        totalXpEarned = 0;
        xpHandler.ResetOldXp();   
        timer.TimerStart();
        audioSourceSfx.PlayOneShot(StartGameSound);
        
        CheckModificators();
        
        if(PlayerPrefs.GetInt("BgMusicInGame", 1) != 0 && gameMusics.Length > 0)
        {
            int rand = UnityEngine.Random.Range(0, gameMusics.Length);
            audioSourceBgMusic.clip = gameMusics[rand];
            audioSourceBgMusic.loop = true;
            audioSourceBgMusic.time = 0;
            audioSourceBgMusic.Play();
        }

        currentDifficulty = PlayerPrefs.GetFloat("Difficulty", 12f);
        howManyGoals = PlayerPrefs.GetInt("Goals", 4);
        howMoneyAdd = Mathf.RoundToInt((currentDifficulty / 3f * Mathf.Max(1, howManyGoals)) * modificatorsMoney);
        howManyXpAddForGoal = Mathf.RoundToInt(currentDifficulty / 2f);
        howManyXpAddAsWin = howManyXpAddForGoal * Mathf.Max(1, Mathf.RoundToInt(howManyGoals / 1.5f));
        howManyXpAddAsLose = 1;
        howMoneyAddAsLose = 1;  
        
        endSreenPanel.SetActive(false);
        
        if(puck.TryGetComponent<TrailRenderer>(out var trail))
        {
            trail.enabled = PlayerPrefs.GetInt("PuckTrail", 1) != 0;
        }
    }

    public void OnGoalTrigger(Collider2D collision)
    {
        VibrationHandler.Vibrate(35, 20);
        if(collision.gameObject.CompareTag("GoalTrigger1"))
        {
            if(lastCollisionGO == player1 && isBotsGame) 
                achievementsHandler.UpdateProgress("own_goal", 1);
                
            score1++;
            UpdateScoreUI(scoreText1, score1);
            
            if(score1 >= howManyGoals)
            {
                if (isBotsGame) Lose();
                else Win();
            } 
            else
            {
                HandleGoalReset(score1, score2);
            }
        }
        else if (collision.gameObject.CompareTag("GoalTrigger2"))
        {
            score2++;
            UpdateScoreUI(scoreText2, score2);
            
            if(score2 >= howManyGoals)
            {
                if(isBotsGame)
                {
                    if(score2 >= 10) achievementsHandler.UpdateProgress("ten", 10);
                    UpdateGoalQuests();
                }
                Win();
            } 
            else
            {
                if(isBotsGame)
                {
                    if (score2 >= 10) achievementsHandler.UpdateProgress("ten", 10);
                    totalXpEarned += howManyXpAddForGoal;
                    UpdateGoalQuests();
                    UpdateAchievements();
                }
                HandleGoalReset(score1, score2);
            }
        }
    }

    private void UpdateScoreUI(TextMeshProUGUI textUI, int currentScore)
    {
        if(currentScore < scoreStrings.Length) textUI.text = scoreStrings[currentScore];
        else textUI.text = currentScore.ToString();
    }

    private void HandleGoalReset(int s1, int s2)
    {
        if(isBotsGame) botsAI.UpdateBotSpeed(s1, s2);
        ResetPosition();
        timer.Goal();
    }

    void Update() 
    {
        if(!isWind || Time.time < _nextUpdate) return;
        _nextUpdate = Time.time + WIND_UPDATE_INTERVAL;

        areaEffector2D.forceAngle = UnityEngine.Random.Range(-360f, 360f);
    }

    public void OnPuckCollisionEnter2D(Collision2D collision) 
    {
        lastCollisionGO = collision.gameObject;
        VibrationHandler.Vibrate(10, 15);
        
        if(lastCollisionGO != player1 && lastCollisionGO != player2)
        {
            audioSourceSfx.PlayOneShot(puckSound);
        }
    }

    public void ResetPosition()
    {
        if(puckRb != null)
        {
            puck.transform.position = puckStartPos;
            puckRb.linearVelocity = Vector2.zero;
            puckRb.angularVelocity = 0f;
        }

        if(p1Controller != null) p1Controller.TeleportToPosition(player1startPos);
        else if(p1Rb != null)
        {
            player1.transform.position = player1startPos;
            p1Rb.linearVelocity = Vector2.zero;
        }

        if(p2Controller != null) p2Controller.TeleportToPosition(player2startPos);
        else if(p2Rb != null)
        {
            player2.transform.position = player2startPos;
            p2Rb.linearVelocity = Vector2.zero;
        }
    }

    public void RestartGame()
    {
        score1 = 0;
        score2 = 0;
        scoreText1.text = scoreStrings[0];
        scoreText2.text = scoreStrings[0];
        
        PlayerPrefs.SetInt("HowMoneyAdds", 0);
        PlayerPrefs.SetInt("HowXpAdds", 0);
        PlayerPrefs.Save();
        
        ResetPosition();
        audioSourceSfx.PlayOneShot(StartGameSound);
        timer.TimerStart();
    }

    public void Win()
    {
        VibrationHandler.Vibrate(200, 45);
        if(isBotsGame)
        {
            int xpBefore = xpHandler.GetXP();   
            int actuallyEarned = howManyXpAddAsWin + PlayerPrefs.GetInt("HowXpAdds");
            
            UpdateXpQuests(actuallyEarned);
            xpHandler.AddXp(actuallyEarned);
            UpdateAchievements();
            UpdateWinQuests();
            
            endScreen.StartEndScreen(actuallyEarned, xpBefore); 
            CheckDifficultyAchievements();
            
            PlayerPrefs.SetInt("Money", moneyHandler.GetMoney());
            PlayerPrefs.SetInt("HowMoneyAdds", PlayerPrefs.GetInt("HowMoneyAdds") + howMoneyAdd);
            PlayerPrefs.SetInt("isAfterGame", 1);
            
            if (isFog) achievementsHandler.UpdateProgress("the_fog", 1);
            if (isWind) achievementsHandler.UpdateProgress("wind", 1);
            if (isBigPlayer) achievementsHandler.UpdateProgress("big", 1);
            if (isSmallPlayer) achievementsHandler.UpdateProgress("small", 1);
            
            PlayerPrefs.Save();
        } 
        else
        {
            endScreen.StartEndScreen(0, xpHandler.GetXP());
        }
        
        ShowEndScreen();
    }

    public void Lose()
    {
        if(isBotsGame)
        {
            int xpBefore = xpHandler.GetXP();
            xpHandler.AddXp(howManyXpAddAsLose + PlayerPrefs.GetInt("HowXpAdds"));
            int xpAfter = xpHandler.GetXP();
            
            int actuallyEarned = xpAfter - xpBefore;
            UpdateXpQuests(actuallyEarned);
            endScreen.StartEndScreen(actuallyEarned, xpBefore); 
            
            if (Mathf.Approximately(currentDifficulty, 7.5f)) achievementsHandler.UpdateProgress("seriously", 1);
            
            PlayerPrefs.SetInt("Money", moneyHandler.GetMoney());
            PlayerPrefs.SetInt("HowMoneyAdds", PlayerPrefs.GetInt("HowMoneyAdds") + howMoneyAddAsLose);
            PlayerPrefs.SetInt("isAfterGame", 1);
            PlayerPrefs.Save();
        }
        
        ShowEndScreen();
    }
    
    private void ShowEndScreen()
    {
        if(isFog) DisableFog();

        var rect = endSreenPanel.GetComponent<RectTransform>();
        rect.localScale = Vector3.zero;
        endSreenPanel.SetActive(true);
        rect.DOScale(Vector3.one, END_SCREEN_ANIM_TIME).SetEase(Ease.OutBack);
    }

    private void DisableFog()
    {
        DOTween.To(() => mainLight2D.intensity, x => mainLight2D.intensity = x, 1f, FOG_FADE_TIME);
        if(p1Light != null) DOTween.To(() => p1Light.intensity, x => p1Light.intensity = x, 0f, FOG_FADE_TIME);
        if(p2Light != null) DOTween.To(() => p2Light.intensity, x => p2Light.intensity = x, 0f, FOG_FADE_TIME);
        
        mainCamera.DOColor(DEFAULT_CAM_COLOR, FOG_FADE_TIME).SetEase(Ease.InOutQuad);
    }

    private void CheckDifficultyAchievements()
    {
        if(Mathf.Approximately(currentDifficulty, 3.1415926535f)) achievementsHandler.UpdateProgress("light_warm-up", 1);
        else if(Mathf.Approximately(currentDifficulty, 7.5f)) achievementsHandler.UpdateProgress("warm-up", 1);
        else if(Mathf.Approximately(currentDifficulty, 13.5f)) 
        {
            achievementsHandler.UpdateProgress("training", 1);
            dailyQuestHandler.UpdateQuestProgress("win_normal_bot", 1);
        }
        else if(Mathf.Approximately(currentDifficulty, 25f)) achievementsHandler.UpdateProgress("fight", 1);
        else if(Mathf.Approximately(currentDifficulty, 50f)) achievementsHandler.UpdateProgress("competitions", 1);
    }

    public void LoadMainMenu()
    {
        PlayerPrefs.Save();
        SceneManager.LoadScene("MainMenu");
    }

    private void CheckModificators()
    {
        string modificators = PlayerPrefs.GetString("CurrentModificators", "None");
        
        if(string.IsNullOrEmpty(modificators) || modificators == "None")
        {
            modificatorsMoney = 1;
            return;
        }

        string[] parts = modificators.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        for(int i = 0; i < parts.Length; i++)
        {
            string currentModifier = parts[i].Trim(); 

            switch(currentModifier)
            {
                case "BigPlayers":
                    player1.transform.DOScale(Vector3.one * MOD_BIG_SCALE, 1.0f).SetEase(Ease.OutBack);
                    player2.transform.DOScale(Vector3.one * MOD_BIG_SCALE, 1.0f).SetEase(Ease.OutBack);
                    modificatorsMoney += 0.3f;
                    isBigPlayer = true;
                    break;
                case "BigPuck":
                    puck.transform.DOScale(Vector3.one * PUCK_BIG_SCALE, 1.0f).SetEase(Ease.OutBack);
                    modificatorsMoney += 0.25f;
                    break;
                case "X2PuckSpeed":
                    if (puck.TryGetComponent<PuckScr>(out var puckScript)) puckScript.maxSpeed = PUCK_MAX_SPEED_X2;
                    modificatorsMoney += 0.3f;
                    break;
                case "Fog":
                    isFog = true;
                    DOTween.To(() => mainLight2D.intensity, x => mainLight2D.intensity = x, 0f, FOG_FADE_TIME);
                    if (p1Light != null) DOTween.To(() => p1Light.intensity, x => p1Light.intensity = x, 1f, FOG_FADE_TIME);
                    if (p2Light != null) DOTween.To(() => p2Light.intensity, x => p2Light.intensity = x, 1f, FOG_FADE_TIME);
                    modificatorsMoney += 0.75f;
                    mainCamera.DOColor(Color.black, FOG_FADE_TIME).SetEase(Ease.InOutQuad);
                    break;
                case "Wind":
                    areaEffector2D.gameObject.SetActive(true);
                    isWind = true;
                    modificatorsMoney += 0.15f;
                    break;
                case "MoreWalls":
                    for (int k = 0; k < additionalWalls.Length; k++) additionalWalls[k].SetActive(true);
                    modificatorsMoney += 0.4f;
                    MoveWallsRelative();
                    break;
                case "SmallPlayers":
                    player1.transform.DOScale(Vector3.one * MOD_SMALL_SCALE, 1.0f).SetEase(Ease.InBack);
                    player2.transform.DOScale(Vector3.one * MOD_SMALL_SCALE, 1.0f).SetEase(Ease.InBack);
                    modificatorsMoney += 0.15f;
                    isSmallPlayer = true;
                    break;
                case "SmallPuck":
                    puck.transform.DOScale(Vector3.one * PUCK_SMALL_SCALE, 1.0f).SetEase(Ease.InBack);
                    modificatorsMoney += 0.1f;
                    break;
            }
        }
        PlayerPrefs.SetString("CurrentModificators", "None");
        PlayerPrefs.Save();
    }

    private void MoveWallsRelative()
    {
        for(int i = 0; i < additionalWalls.Length; i++)
        {
            additionalWalls[i].transform.DOBlendableMoveBy(new Vector3(0, 6.9f, 0), 5.0f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }

    public void UpdateAchievements()
    {
        PlayerPrefs.SetInt("TotalGoals", PlayerPrefs.GetInt("TotalGoals", 0) + 1);
        PlayerPrefs.Save();
        achievementsHandler.UpdateProgress("a_start_has_been_made", 1);
        achievementsHandler.UpdateProgress("begginer", 1);
        achievementsHandler.UpdateProgress("amateur", 1);
        achievementsHandler.UpdateProgress("professional", 1);
        achievementsHandler.UpdateProgress("master", 1);
        achievementsHandler.UpdateProgress("world_champion", 1);
        achievementsHandler.UpdateProgress("best_in_the_galaxy", 1);
        achievementsHandler.UpdateProgress("best_in_the_universe", 1);
    }

    private void UpdateWinQuests()
    {
        dailyQuestHandler.UpdateQuestProgress("win_1_matches", 1);
        dailyQuestHandler.UpdateQuestProgress("win_3_matches", 1);
        dailyQuestHandler.UpdateQuestProgress("win_5_matches", 1);
        dailyQuestHandler.UpdateQuestProgress("win_7_matches", 1);
        dailyQuestHandler.UpdateQuestProgress("win_10_matches", 1);
    }

    private void UpdateGoalQuests()
    {
        questsHandler.UpdateQuestProgress("goal10", 1);
        questsHandler.UpdateQuestProgress("goal50", 1);
        questsHandler.UpdateQuestProgress("goal100", 1);
        questsHandler.UpdateQuestProgress("goal200", 1);
        questsHandler.UpdateQuestProgress("goal300", 1);
        questsHandler.UpdateQuestProgress("goal500", 1);
        dailyQuestHandler.UpdateQuestProgress("goal20", 1);
    }

    private void UpdateXpQuests(int amount)
    {
        questsHandler.UpdateQuestProgress("xp100", amount);
        questsHandler.UpdateQuestProgress("xp200", amount);
        questsHandler.UpdateQuestProgress("xp400", amount);
        questsHandler.UpdateQuestProgress("xp500", amount);
        questsHandler.UpdateQuestProgress("xp700", amount);
        questsHandler.UpdateQuestProgress("xp1000", amount);
        dailyQuestHandler.UpdateQuestProgress("xp50", amount);
    }
}