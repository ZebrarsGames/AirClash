using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class CoinMover : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private GameObject xpPrefab;
    [SerializeField] private Transform targetPosition;
    [SerializeField] private Text coinText;
    [SerializeField] private Transform canvasParent;

    [Header("Settings")]
    [SerializeField] private float duration = 2f;
    [SerializeField] private float delayBetween = 0.05f;
    [SerializeField] private AudioClip coinSound;
    [Header("Scripts")]
    [SerializeField] private MoneyHandler moneyHandler;
    [SerializeField] private XpUiScr xpUiScr;
    [SerializeField] private XpHandler xpHandler;
    
    private AudioSource audioSource;
    private int currentCoinsCount = 0;

    void Start()
    {
        currentCoinsCount = moneyHandler.GetMoney();
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void AddCoins(Vector3 spawnPosition, int amount)
    {
        StartCoroutine(AnimateCoins(spawnPosition, amount));
    }

    public void AddXp(Vector3 spawnPosition, int amount, int startValue)
    {
        StartCoroutine(AnimateXP(spawnPosition, amount, startValue));
    }

    private IEnumerator AnimateCoins(Vector3 spawnPosition, int amount)
    {
        moneyHandler.AddMoney(amount);
        for (int i = 0; i < amount; i++)
        {
            // 1. Создаем монету
            GameObject coin = Instantiate(coinPrefab, canvasParent);
            coin.transform.position = spawnPosition;
            
            // Немного разбрасываем монеты в начале для "сочности"
            coin.transform.localScale = Vector3.zero;
            coin.transform.DOScale(1f, 0.2f);
            coin.transform.DOMove(spawnPosition + (Vector3)Random.insideUnitCircle * 6f, 0.2f);

            // 2. Летим к цели через небольшую паузу
            coin.transform.DOMove(targetPosition.position, duration)
                .SetDelay(0.2f)
                .SetEase(Ease.InBack) // Эффект рывка в конце
                .OnComplete(() => {
                    // 3. Когда долетела:
                    currentCoinsCount++;
                    UpdateUI();
                    PlayCoinSound();
                    
                    // Анимация пульсации счетчика
                    targetPosition.DOScale(1.2f, 0.1f).OnComplete(() => targetPosition.DOScale(1f, 0.1f));
                    
                    Destroy(coin);
                });

            yield return new WaitForSeconds(delayBetween);
        }
    }

    private IEnumerator AnimateXP(Vector3 spawnPosition, int amount, int startValue)
    {
        yield return new WaitForSeconds(0.7f);

        for (int i = 1; i <= amount; i++) 
        {
            GameObject xp = Instantiate(xpPrefab, canvasParent);
            xp.transform.position = spawnPosition;
            
            // Значение конкретно для этой летящей монетки
            int valueForThisCoin = startValue + i; 

            xp.transform.localScale = Vector3.zero;
            xp.transform.DOScale(1f, 0.2f);
            xp.transform.DOMove(spawnPosition + (Vector3)Random.insideUnitCircle * 6f, 0.2f);

            xp.transform.DOMove(targetPosition.position, duration)
                .SetDelay(0.2f)
                .SetEase(Ease.InBack)
                .OnComplete(() => {
                    PlayCoinSound();
                    // visualValue теперь точно не обгонит реальность
                    xpUiScr.SetProgress(xpHandler.GetXPProgress(valueForThisCoin), valueForThisCoin);
                    
                    targetPosition.DOScale(1.1f, 0.1f).OnComplete(() => targetPosition.DOScale(1f, 0.1f));
                    Destroy(xp);
                });

            yield return new WaitForSeconds(delayBetween);
        }
    }
    void UpdateUI()
    {
        coinText.text = "Деньги " + currentCoinsCount.ToString();
    }

    void PlayCoinSound()
    {
        if (coinSound != null)
        {
            audioSource.pitch = 1f + (currentCoinsCount % 10 * 0.05f); 
            audioSource.PlayOneShot(coinSound);
        }
    }
}
