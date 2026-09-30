using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using DG.Tweening;
using System.Threading.Tasks;
using NUnit.Framework;

public class DailyAwardHandler : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject rewardPanel;
    [SerializeField] private TextMeshProUGUI warningText;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI xpText;
    [SerializeField] private RectTransform skinImage;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip whooshSfx;

    [Header("Scripts")]
    [SerializeField] private MoneyHandler moneyHandler;
    [SerializeField] private XpHandler xpHandler;

    private Sequence rewardSequence;
    private const string serverUrl = "https://airclashserver.onrender.com/claimDailyReward";

    [Serializable]
    public class RewardData
    {
        public string rarity;
        public int coins;
        public int xp;
        public string skin_id;
    }

    [Serializable]
    public class DailyRewardResponse
    {
        public string status;
        public string message;
        public long remaining_ms;
        public RewardData reward;
    }

    [Serializable]
    public class DailyClaimRequest
    {
        public string username;
    }

    private void Start()
    {
        if(rewardPanel != null) rewardPanel.SetActive(false);
        if(warningText != null) warningText.SetText("Загружаем...");
        if(moneyText != null) moneyText.gameObject.SetActive(false);
        if(xpText != null) xpText.gameObject.SetActive(false);
        if(skinImage != null) skinImage.gameObject.SetActive(false);
    }

    public async void CheckReward()
    {
        string nickname = PlayerPrefs.GetString("Nick", "Ник");
        bool isAccountExists = await FirebaseManager.CheckUserExistsAsync(nickname);
        if(!isAccountExists)
        {
            Debug.LogWarning("Нельзя получить награды без аккаунта!");
            warningText.SetText("Нельзя получить награды без аккаунта!");
            warningText.gameObject.SetActive(true);
            return;
        }
        StartCoroutine(SendClaimRequestRoutine(nickname));
    }

    private IEnumerator SendClaimRequestRoutine(string username)
    {
        DailyClaimRequest requestData = new DailyClaimRequest { username = username };
        string jsonBody = JsonUtility.ToJson(requestData);

        using(UnityWebRequest www = new UnityWebRequest(serverUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("x-game-secret", GameConfig.ApiSecret);

            yield return www.SendWebRequest();

            if(www.result == UnityWebRequest.Result.Success)
            {
                DailyRewardResponse response = JsonUtility.FromJson<DailyRewardResponse>(www.downloadHandler.text);

                if(response.status == "success" && response.reward != null)
                {
                    moneyHandler.AddMoney(response.reward.coins);
                    xpHandler.AddXp(response.reward.xp);
                    bool isSkin = false;
                    if(!string.IsNullOrEmpty(response.reward.skin_id))
                    {
                        isSkin = true;
                        string skins = PlayerPrefs.GetString("AllBuySkins", "DefSkin");
                        string[] parts = skins.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        string[] new_parts = new string[parts.Length+1];
                        new_parts[parts.Length+1] = response.reward.skin_id;
                        string serializedSkins = string.Join(",", new_parts);
                        PlayerPrefs.SetString("AllBuySkins", serializedSkins);
                        PlayerPrefs.Save();
                    }
                    warningText.gameObject.SetActive(false);
                    ShowRewards(response.reward.coins, response.reward.xp, isSkin);
                }
                else if(response.status == "already_claimed")
                {
                    TimeSpan t = TimeSpan.FromMilliseconds(response.remaining_ms);
                    Debug.Log($"Награда уже забрана! До сброса: {t.Hours}ч {t.Minutes}мин {t.Seconds}сек");
                    warningText.SetText("Награда уже забрана! До сброса: {0}ч {1}мин {2}сек", t.Hours, t.Minutes, t.Seconds);
                }
            }
            else
            {
                Debug.LogError($"Ошибка сети при получении награды: {www.error}");
            }
        }
    }

    public void ShowRewards(int coins, int xp, bool isSkin)
    {
        rewardSequence?.Kill(true);
        
        moneyText.SetText($"{coins} <sprite=0>");
        xpText.SetText($"{xp} <sprite=0>");
        if(isSkin) skinImage.gameObject.SetActive(true);

        moneyText.transform.localScale = Vector3.zero;
        xpText.transform.localScale = Vector3.zero;

        moneyText.gameObject.SetActive(true);
        xpText.gameObject.SetActive(true);

        rewardSequence = DOTween.Sequence();

        if(!isSkin)
        {
            rewardSequence
            .Append(moneyText.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack))
            .AppendCallback(() => PlaySound())
            
            .AppendInterval(0.2f)
            .Append(xpText.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack))
            .AppendCallback(() => PlaySound())

            .SetUpdate(UpdateType.Normal, true)
            .OnKill(() => rewardSequence = null);
        } else
        {
            rewardSequence
                .Append(moneyText.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack))
                .AppendCallback(() => PlaySound())
                
                .AppendInterval(0.2f)
                .Append(xpText.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack))
                .AppendCallback(() => PlaySound())

                .AppendInterval(0.2f)
                .Append(skinImage.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack))
                .AppendCallback(() => PlaySound())

                .SetUpdate(UpdateType.Normal, true)
                .OnKill(() => rewardSequence = null);
        }
    }

    private void PlaySound()
    {
        if(audioSource && whooshSfx)
        {
            audioSource.PlayOneShot(whooshSfx);
        }
    }

    private void OnDestroy()
    {
        rewardSequence?.Kill();
    }
}