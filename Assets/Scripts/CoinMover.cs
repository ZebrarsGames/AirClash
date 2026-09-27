using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class CoinMover : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private GameObject xpPrefab;
    [SerializeField] private Transform targetPosition;
    [SerializeField] private TextMeshProUGUI coinText;
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
    private WaitForSeconds delayWait;
    private WaitForSeconds xpStartDelayWait = new WaitForSeconds(0.7f);

    private Queue<GameObject> coinPool = new Queue<GameObject>();
    private Queue<GameObject> xpPool = new Queue<GameObject>();

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        delayWait = new WaitForSeconds(delayBetween);
    }

    private void Start()
    {
        currentCoinsCount = moneyHandler.GetMoney();
    }

    public void AddCoins(Vector3 spawnPosition, int amount)
    {
        StartCoroutine(AnimateCoins(spawnPosition, amount));
    }

    public void AddXp(Vector3 spawnPosition, int amount, int startValue)
    {
        if(xpUiScr.GetIsAnim()) return;
        StartCoroutine(AnimateXP(spawnPosition, amount, startValue));
    }

    private IEnumerator AnimateCoins(Vector3 spawnPosition, int amount)
    {
        moneyHandler.AddMoney(amount);

        for(int i = 0; i < amount; i++)
        {
            GameObject coin = GetFromPool(coinPool, coinPrefab);
            Transform coinTransform = coin.transform;
            coinTransform.position = spawnPosition;
            coinTransform.localScale = Vector3.zero;

            Vector2 randomCircle = Random.insideUnitCircle * 6f;
            Vector3 targetOffset = spawnPosition + new Vector3(randomCircle.x, randomCircle.y, 0f);

            coinTransform.DOScale(1f, 0.2f);
            coinTransform.DOMove(targetOffset, 0.2f);

            coinTransform.DOMove(targetPosition.position, duration)
                .SetDelay(0.2f)
                .SetEase(Ease.InBack)
                .OnComplete(() => {
                    currentCoinsCount++;
                    UpdateUI();
                    PlayCoinSound();

                    targetPosition.DOKill();
                    targetPosition.DOScale(1.2f, 0.1f).OnComplete(() => targetPosition.DOScale(1f, 0.1f));

                    ReturnToPool(coinPool, coin);
                });

            yield return delayWait;
        }
    }

    private IEnumerator AnimateXP(Vector3 spawnPosition, int amount, int startValue)
    {
        yield return xpStartDelayWait;

        for(int i = 1; i <= amount; i++)
        {
            GameObject xp = GetFromPool(xpPool, xpPrefab);
            Transform xpTransform = xp.transform;
            xpTransform.position = spawnPosition;
            xpTransform.localScale = Vector3.zero;

            int valueForThisCoin = startValue + i;

            Vector2 randomCircle = Random.insideUnitCircle * 6f;
            Vector3 targetOffset = spawnPosition + new Vector3(randomCircle.x, randomCircle.y, 0f);

            xpTransform.DOScale(1f, 0.2f);
            xpTransform.DOMove(targetOffset, 0.2f);

            xpTransform.DOMove(targetPosition.position, duration)
                .SetDelay(0.2f)
                .SetEase(Ease.InBack)
                .OnComplete(() => {
                    PlayCoinSound();
                    xpUiScr.SetProgress(xpHandler.GetXPProgress(valueForThisCoin), valueForThisCoin);

                    targetPosition.DOKill();
                    targetPosition.DOScale(1.1f, 0.1f).OnComplete(() => targetPosition.DOScale(1f, 0.1f));

                    ReturnToPool(xpPool, xp);
                });

            yield return delayWait;
        }
    }

    private void UpdateUI()
    {
        coinText.SetText($"{currentCoinsCount} <sprite=0>");
    }

    private void PlayCoinSound()
    {
        if(coinSound != null)
        {
            audioSource.pitch = 1f + (currentCoinsCount % 10 * 0.05f);
            audioSource.PlayOneShot(coinSound);
        }
    }

    private GameObject GetFromPool(Queue<GameObject> pool, GameObject prefab)
    {
        GameObject obj;
        if(pool.Count > 0)
        {
            obj = pool.Dequeue();
            obj.SetActive(true);
        }
        else
        {
            obj = Instantiate(prefab, canvasParent);
        }
        return obj;
    }

    private void ReturnToPool(Queue<GameObject> pool, GameObject obj)
    {
        obj.transform.DOKill();
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}