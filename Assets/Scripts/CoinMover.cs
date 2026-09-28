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
    [SerializeField] private float duration = 1.5f;
    [SerializeField] private float delayBetween = 0.03f;
    [SerializeField] private int maxVisualCount = 50;
    [SerializeField] private float minIntervalBetweenSounds = 0.05f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip coinSound;

    [Header("External Handlers")]
    [SerializeField] private MoneyHandler moneyHandler;
    [SerializeField] private XpUiScr xpUiScr;
    [SerializeField] private XpHandler xpHandler;

    private int _currentCoinsCount;
    private float _nextSoundTime;
    
    private WaitForSeconds _delayWait;
    private readonly WaitForSeconds _xpStartDelayWait = new(0.5f);

    private readonly Queue<GameObject> _coinPool = new();
    private readonly Queue<GameObject> _xpPool = new();

    private void Awake()
    {
        _delayWait = new WaitForSeconds(delayBetween);
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        
        PrewarmPool(_coinPool, coinPrefab, maxVisualCount);
        PrewarmPool(_xpPool, xpPrefab, maxVisualCount);
    }

    private void Start()
    {
        _currentCoinsCount = moneyHandler.GetMoney();
        UpdateUI();
    }

    public void AddCoins(Vector3 spawnPosition, int amount)
    {
        if(amount <= 0) return;
        moneyHandler.AddMoney(amount);
        StartCoroutine(AnimateCoinsRoutine(spawnPosition, amount));
    }

    public void AddXp(Vector3 spawnPosition, int amount, int startValue)
    {
        if(amount <= 0 || xpUiScr.GetIsAnim()) return;
        StartCoroutine(AnimateXPRoutine(spawnPosition, amount, startValue));
    }

    private IEnumerator AnimateCoinsRoutine(Vector3 spawnPosition, int amount)
    {
        int spawnCount = Mathf.Min(amount, maxVisualCount);
        int coinsPerVisual = Mathf.CeilToInt((float)amount / spawnCount);

        for(int i = 0; i < spawnCount; i++)
        {
            GameObject coin = GetFromPool(_coinPool, coinPrefab);
            Transform coinTransform = coin.transform;
            coinTransform.position = spawnPosition;
            coinTransform.localScale = Vector3.zero;

            Vector2 randomCircle = Random.insideUnitCircle * 6f;
            Vector3 targetOffset = spawnPosition + new Vector3(randomCircle.x, randomCircle.y, 0f);

            Sequence coinSequence = DOTween.Sequence();
            coinSequence.Append(coinTransform.DOScale(1f, 0.2f))
                        .Join(coinTransform.DOMove(targetOffset, 0.2f))
                        .Append(coinTransform.DOMove(targetPosition.position, duration).SetEase(Ease.InBack))
                        .OnComplete(() =>
                        {
                            _currentCoinsCount += coinsPerVisual;
                            UpdateUI();
                            TryPlayEffects();
                            AnimateTargetPunch();
                            ReturnToPool(_coinPool, coin);
                        });

            yield return _delayWait;
        }
    }

    private IEnumerator AnimateXPRoutine(Vector3 spawnPosition, int amount, int startValue)
    {
        yield return _xpStartDelayWait;

        int spawnCount = Mathf.Min(amount, maxVisualCount);
        float step = (float)amount / spawnCount;

        for(int i = 0; i < spawnCount; i++)
        {
            GameObject xp = GetFromPool(_xpPool, xpPrefab);
            Transform xpTransform = xp.transform;
            xpTransform.position = spawnPosition;
            xpTransform.localScale = Vector3.zero;

            int currentStepIndex = Mathf.RoundToInt((i + 1) * step);
            int valueForThisCoin = startValue + currentStepIndex;

            Vector2 randomCircle = Random.insideUnitCircle * 6f;
            Vector3 targetOffset = spawnPosition + new Vector3(randomCircle.x, randomCircle.y, 0f);

            Sequence xpSequence = DOTween.Sequence();
            xpSequence.Append(xpTransform.DOScale(1f, 0.2f))
                      .Join(xpTransform.DOMove(targetOffset, 0.2f))
                      .Append(xpTransform.DOMove(targetPosition.position, duration).SetEase(Ease.InBack))
                      .OnComplete(() =>
                      {
                          TryPlayEffects();
                          xpUiScr.SetProgress(xpHandler.GetXPProgress(valueForThisCoin), valueForThisCoin);
                          AnimateTargetPunch();
                          ReturnToPool(_xpPool, xp);
                      });

            yield return _delayWait;
        }
    }

    private void UpdateUI()
    {
        coinText.SetText("{0} <sprite=0>", _currentCoinsCount);
    }

    private void AnimateTargetPunch()
    {
        targetPosition.DOComplete(); 
        targetPosition.DOPunchScale(Vector3.one * 0.15f, 0.15f, 1, 0.5f);
    }

    private void TryPlayEffects()
    {
        if(Time.time < _nextSoundTime) return;
        _nextSoundTime = Time.time + minIntervalBetweenSounds;

        if(coinSound != null)
        {
            audioSource.pitch = 1f + ((_currentCoinsCount % 10) * 0.03f);
            audioSource.PlayOneShot(coinSound);
        }
        
        if(_currentCoinsCount % 3 == 0)
        {
            VibrationHandler.Vibrate(2, 2);
        }
    }

    private void PrewarmPool(Queue<GameObject> pool, GameObject prefab, int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(prefab, canvasParent);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    private GameObject GetFromPool(Queue<GameObject> pool, GameObject prefab)
    {
        if(pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        return Instantiate(prefab, canvasParent);
    }

    private void ReturnToPool(Queue<GameObject> pool, GameObject obj)
    {
        obj.transform.DOKill();
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}