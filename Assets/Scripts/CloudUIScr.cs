using UnityEngine;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using System.Text;

public class CloudUIScr : MonoBehaviour
{
    [Header("Input Fields")]
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("Buttons")]
    [SerializeField] private Button[] buttons;

    [Header("Panels")]
    [SerializeField] private GameObject surePanel;
    [SerializeField] private GameObject cloudPanel;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sureSound;

    [Header("Scripts")]
    [SerializeField] private QuestsHandler questsHandler;
    [SerializeField] private DailyQuestHandler dailyQuestHandler;
    [SerializeField] private AchievementsHandler achievementsHandler;
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private MoneyHandler moneyHandler;
    [SerializeField] private XpHandler xpHandler;
    [SerializeField] private FirebaseManager firebaseManager;

    private HashSet<string> forbiddenWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private RectTransform surePanelRect;
    private RectTransform cloudPanelRect;
    private StringBuilder stringBuilder = new StringBuilder(64);

    private static readonly string[] MoneyQuests = new string[] { "money10", "money50", "money100", "money200", "money300", "money500" };
    private static readonly string[] MoneyDailyQuests = new string[] { "daily_money50", "money70", "daily_money100" };
    private static readonly string[] XpQuests = new string[] { "xp100", "xp200", "xp400", "xp500", "xp700", "xp1000" };
    private static readonly string[] GoalQuests = new string[] { "goal10", "goal50", "goal100", "goal200", "goal300", "goal500" };

    private void Awake()
    {
        if(surePanel != null)
        {
            surePanelRect = surePanel.GetComponent<RectTransform>();
        }
        if(cloudPanel != null)
        {
            cloudPanelRect = cloudPanel.GetComponent<RectTransform>();
        }
    }

    private void Start()
    {
        usernameInput.text = saveManager.GetData().NickName;
        LoadBadWords();
    }

    private void OnDestroy()
    {
        if(surePanelRect != null) surePanelRect.DOKill();
        if(cloudPanelRect != null) cloudPanelRect.DOKill();
    }

    public void OnClickLoginOrRegister()
    {
        if(!IsTextValid()) return;
        firebaseManager.AccountAuth(usernameInput.text, passwordInput.text);
    }

    public void OnClickSave()
    {
        if(!IsTextValid()) return;
        firebaseManager.SaveProgress(usernameInput.text, passwordInput.text);
    }

    public void OnClickLoad()
    {
        if(!IsTextValid()) return;
        firebaseManager.LoadProgress(usernameInput.text, passwordInput.text);
    }

    public void OnInputField()
    {
        cloudPanelRect.DOKill();
        cloudPanelRect.DOLocalMoveY(228f, 0.3f).SetEase(Ease.OutSine);
    }

    public void OnInputFieldEnd()
    {
        cloudPanelRect.DOKill();
        cloudPanelRect.DOLocalMoveY(0f, 0.3f).SetEase(Ease.OutSine);
    }

    public void SetStatusText(string status)
    {
        stringBuilder.Clear();
        stringBuilder.Append("Статус: ").Append(status);
        statusText.text = stringBuilder.ToString();
    }

    public void SetActiveBtns(bool isActive)
    {
        bool interactableState = !isActive;
        for(int i = 0; i < buttons.Length; i++)
        {
            buttons[i].interactable = interactableState;
        }
    }

    public void SetPlayerData(PlayerData playerData)
    {
        if(playerData != null)
        {
            SetMoneyQuests(playerData.TotalMoney);
            SetXpQuests(playerData.TotalXP);
            SetGoalQuests(playerData.Goals);

            PlayerPrefs.SetString("Nick", usernameInput.text);
            PlayerPrefs.SetInt("TotalGoals", playerData.Goals);
            PlayerPrefs.SetString("CurrentSkin", playerData.CurrentSkinName);

            moneyHandler.SetMoney(playerData.Money);
            moneyHandler.SetTotalMoney(playerData.TotalMoney);
            xpHandler.SetLevel(playerData.XpLevel);
            xpHandler.SetTotalXp(playerData.TotalXP);
            xpHandler.SetXp(playerData.XP);
            xpHandler.SetXpToNextLevel(playerData.XpToNextLevel);
            PlaytimeTracker.Instance.SetSecondsPlaytime(playerData.Playtime);

            int achievementsCount = achievementsHandler.GetCountOfAchievements();
            for(int i = 0; i < achievementsCount; i++)
            {
                string id = achievementsHandler.GetStringId(i);
                achievementsHandler.SetProgress(id, playerData.AchievementsProgress[i]);
            }

            string[] parts = playerData.AllBuySkins;
            for(int i = 0; i < parts.Length; i++)
            {
                PlayerPrefs.SetInt(parts[i], 1);
            }

            PlayerPrefs.Save();
            saveManager.SaveData();
        }
        else
        {
            saveManager.SaveDefaultData();
        }
    }

    public void ShowSurePanel()
    {
        audioSource.PlayOneShot(sureSound);
        surePanelRect.DOKill();
        surePanelRect.localScale = Vector3.zero;
        surePanel.SetActive(true);
        surePanelRect.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
    }

    public void NoSurePanel()
    {
        surePanelRect.DOKill();
        surePanelRect.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).OnComplete(() => surePanel.SetActive(false));
    }

    public void YesSurePanel()
    {
        firebaseManager.DeleteAccount(usernameInput.text, passwordInput.text);
        surePanelRect.DOKill();
        surePanelRect.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).OnComplete(() => surePanel.SetActive(false));
    }

    private void SetMoneyQuests(int amount)
    {
        for(int i = 0; i < MoneyQuests.Length; i++)
        {
            questsHandler.SetQuestProgress(MoneyQuests[i], amount);
        }
        for(int i = 0; i < MoneyDailyQuests.Length; i++)
        {
            dailyQuestHandler.UpdateQuestProgress(MoneyDailyQuests[i], amount);
        }
    }

    private void SetXpQuests(int amount)
    {
        for(int i = 0; i < XpQuests.Length; i++)
        {
            questsHandler.SetQuestProgress(XpQuests[i], amount);
        }
        dailyQuestHandler.UpdateQuestProgress("xp50", amount);
    }

    private void SetGoalQuests(int amount)
    {
        for(int i = 0; i < GoalQuests.Length; i++)
        {
            questsHandler.SetQuestProgress(GoalQuests[i], amount);
        }
        dailyQuestHandler.UpdateQuestProgress("goal20", amount);
    }

    private bool IsTextValid()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;

        for(int i = 0; i < username.Length; i++)
        {
            char c = username[i];
            if(!char.IsLetterOrDigit(c) && c != '_' && c != ' ')
            {
                Debug.LogWarning($"Найден запрещенный символ в логине: {c}");
                statusText.text = $"Статус: Недопустимый символ '{c}' в логине.";
                return false;
            }
        }

        if(string.IsNullOrEmpty(password) || password.Length < 6)
        {
            statusText.text = "Статус: Пароль слишком короткий (минимум 6 символов).";
            return false;
        }

        string preparedInput = PrepareText(username);

        foreach(var badWord in forbiddenWords)
        {
            if(preparedInput.Contains(badWord))
            {
                statusText.text = "Статус: Логин содержит запрещенное слово!";
                Debug.LogWarning($"Блокировка: Ввод '{username}' содержит запрещенную комбинацию.");
                return false;
            }
        }

        return true;
    }

    private void LoadBadWords()
    {
        TextAsset textAsset = Resources.Load<TextAsset>("network_config");

        if(textAsset != null)
        {
            try
            {
                byte[] decodedBytes = Convert.FromBase64String(textAsset.text);
                string decodedText = Encoding.UTF8.GetString(decodedBytes);

                string[] lines = decodedText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                HashSet<string> uniqueWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                for(int i = 0; i < lines.Length; i++)
                {
                    string preparedBadWord = PrepareText(lines[i]);
                    if(!string.IsNullOrEmpty(preparedBadWord) && preparedBadWord.Length > 2)
                    {
                        uniqueWords.Add(preparedBadWord);
                    }
                }

                forbiddenWords = uniqueWords;
                Debug.Log($"[System] База сети загружена. Оптимизированных элементов: {forbiddenWords.Count}");
            }
            catch(Exception e)
            {
                Debug.LogError($"Ошибка чтения конфигурации сети: {e.Message}");
            }
            finally
            {
                Resources.UnloadAsset(textAsset);
            }
        }
        else
        {
            Debug.LogError("Файл network_config не найден в Resources!");
        }
    }

    private string PrepareText(string input)
    {
        if(string.IsNullOrEmpty(input)) return string.Empty;

        stringBuilder.Clear();

        for(int i = 0; i < input.Length; i++)
        {
            char c = char.ToLowerInvariant(input[i]);

            switch(c)
            {
                case '@': c = 'а'; break;
                case '$': c = 'с'; break;
                case '0': c = 'о'; break;
                case '1': c = 'и'; break;
                case '3': c = 'з'; break;
                case '4': c = 'ч'; break;
                case '5': c = 'с'; break;
                case '6': c = 'б'; break;
                case '9': c = 'д'; break;
                case 'a': c = 'а'; break;
                case 'o': c = 'о'; break;
                case 'e': c = 'е'; break;
                case 'c': c = 'с'; break;
                case 'p': c = 'р'; break;
                case 'x': c = 'х'; break;
                case 'y': c = 'у'; break;
                case 'k': c = 'к'; break;
                case 'm': c = 'м'; break;
                case 't': c = 'т'; break;
            }

            if(char.IsLetterOrDigit(c))
            {
                stringBuilder.Append(c);
            }
        }

        if(stringBuilder.Length <= 1) return stringBuilder.ToString();

        int writeIndex = 1;
        for(int readIndex = 1; readIndex < stringBuilder.Length; readIndex++)
        {
            if(stringBuilder[readIndex] != stringBuilder[readIndex - 1])
            {
                stringBuilder[writeIndex] = stringBuilder[readIndex];
                writeIndex++;
            }
        }

        stringBuilder.Length = writeIndex;
        return stringBuilder.ToString();
    }
}