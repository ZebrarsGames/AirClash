using UnityEngine;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

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

    void Start()
    {
        usernameInput.text = saveManager.GetData().NickName;
        LoadBadWords();
    }

    public void OnClickLoginOrRegister()
    {
        bool isTextValid = IsTextValid();
        if(!isTextValid) return;
        firebaseManager.AccountAuth(usernameInput.text, passwordInput.text);
    }

    public void OnClickSave()
    {
        bool isTextValid = IsTextValid();
        if(!isTextValid) return;
        firebaseManager.SaveProgress(usernameInput.text, passwordInput.text);
    }

    public void OnClickLoad()
    {
        bool isTextValid = IsTextValid();
        if(!isTextValid) return;
        firebaseManager.LoadProgress(usernameInput.text, passwordInput.text);
    }

    public void OnInputField()
    {
        cloudPanel.GetComponent<RectTransform>().DOLocalMoveY(228, 0.3f).SetEase(Ease.OutSine);
    }

    public void OnInputFieldEnd()
    {
        cloudPanel.GetComponent<RectTransform>().DOLocalMoveY(0, 0.3f).SetEase(Ease.OutSine);
    }

    public void SetStatusText(string status)
    {
        statusText.text = "Статус: " + status;
    }

    public void SetActiveBtns(bool isActive)
    {
        for(int i = 0; i < buttons.Length; i++)
        {
            buttons[i].interactable = !isActive;
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
        } else
        {
            saveManager.SaveDefaultData();
        }
    }

    public void ShowSurePanel()
    {
        audioSource.PlayOneShot(sureSound);
        var rect = surePanel.GetComponent<RectTransform>();
        rect.localScale = Vector3.zero;
        surePanel.SetActive(true);
        rect.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
    }

    public void NoSurePanel()
    {
        var rect = surePanel.GetComponent<RectTransform>();
        rect.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).OnComplete(() => surePanel.SetActive(false));
        rect.localScale = Vector3.one;
    }

    public void YesSurePanel()
    {
        firebaseManager.DeleteAccount(usernameInput.text, passwordInput.text);
        var rect = surePanel.GetComponent<RectTransform>();
        rect.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).OnComplete(() => surePanel.SetActive(false));
        rect.localScale = Vector3.one;
    }

    private void SetMoneyQuests(int amount)
    {
        questsHandler.SetQuestProgress("money10", amount);
        questsHandler.SetQuestProgress("money50", amount);
        questsHandler.SetQuestProgress("money100", amount);
        questsHandler.SetQuestProgress("money200", amount);
        questsHandler.SetQuestProgress("money300", amount);
        questsHandler.SetQuestProgress("money500", amount);
        dailyQuestHandler.UpdateQuestProgress("daily_money50", amount);
        dailyQuestHandler.UpdateQuestProgress("money70", amount);
        dailyQuestHandler.UpdateQuestProgress("daily_money100", amount);
    }

    private void SetXpQuests(int amount)
    {
        questsHandler.SetQuestProgress("xp100", amount);
        questsHandler.SetQuestProgress("xp200", amount);
        questsHandler.SetQuestProgress("xp400", amount);
        questsHandler.SetQuestProgress("xp500", amount);
        questsHandler.SetQuestProgress("xp700", amount);
        questsHandler.SetQuestProgress("xp1000", amount);
        dailyQuestHandler.UpdateQuestProgress("xp50", amount);
    }

    private void SetGoalQuests(int amount)
    {
        questsHandler.SetQuestProgress("goal10", amount);
        questsHandler.SetQuestProgress("goal50", amount);
        questsHandler.SetQuestProgress("goal100", amount);
        questsHandler.SetQuestProgress("goal200", amount);
        questsHandler.SetQuestProgress("goal300", amount);
        questsHandler.SetQuestProgress("goal500", amount);
        dailyQuestHandler.UpdateQuestProgress("goal20", amount);
    }

    private bool IsTextValid()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;

        foreach(char c in username)
        {
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
                
                HashSet<string> uniqueWords = new HashSet<string>();

                foreach(var word in lines)
                {
                    string preparedBadWord = PrepareText(word);
                    if (!string.IsNullOrEmpty(preparedBadWord) && preparedBadWord.Length > 2)
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
        }
        else
        {
            Debug.LogError("Файл network_config не найден в Resources!");
        }
    }

    private string PrepareText(string input)
    {
        if(string.IsNullOrEmpty(input)) return "";

        string text = input.ToLowerInvariant();

        text = text.Replace("@", "а")
                .Replace("$", "с")
                .Replace("0", "о")
                .Replace("1", "и")
                .Replace("3", "з")
                .Replace("4", "ч")
                .Replace("5", "с")
                .Replace("6", "б")
                .Replace("9", "д")
                .Replace("a", "а")
                .Replace("o", "о")
                .Replace("e", "е")
                .Replace("c", "с")
                .Replace("p", "р")
                .Replace("x", "х")
                .Replace("y", "у")
                .Replace("k", "к")
                .Replace("m", "м")
                .Replace("t", "т");

        text = new string(text.Where(char.IsLetterOrDigit).ToArray());

        text = Regex.Replace(text, @"(.)\1+", "$1");

        return text;
    }
}
