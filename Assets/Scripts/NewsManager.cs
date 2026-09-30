using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class NewsManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject newsCardPrefab;
    [SerializeField] private GameObject loadingIndicator;
    [SerializeField] private TextMeshProUGUI emptyText;

    private const string SERVER_URL = "https://airclashserver.onrender.com/news";

    private void OnEnable()
    {
        StartCoroutine(FetchNewsCoroutine());
    }

    private IEnumerator FetchNewsCoroutine()
    {
        if(Application.internetReachability == NetworkReachability.NotReachable)
        {
            if(emptyText != null)
            {
                emptyText.gameObject.SetActive(true);
                emptyText.SetText("Нельзя прочитать новости без интернета!");
                Debug.LogWarning("Нельзя прочитать новости без интернета!");
                yield break;
            }
        }
        if(loadingIndicator != null) loadingIndicator.SetActive(true);
        if(emptyText != null) emptyText.gameObject.SetActive(false);

        foreach(Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        using(UnityWebRequest www = UnityWebRequest.Get(SERVER_URL))
        {
            www.SetRequestHeader("x-game-secret", GameConfig.ApiSecret);
            www.timeout = 10;
            yield return www.SendWebRequest();

            if(loadingIndicator != null) loadingIndicator.SetActive(false);

            if(www.result == UnityWebRequest.Result.Success)
            {
                string json = www.downloadHandler.text;
                NewsResponse response = JsonUtility.FromJson<NewsResponse>(json);

                if(response != null && response.success && response.data != null && response.data.Count > 0)
                {
                    string[] contents = new string[response.data.Count];
                    foreach(var newsItem in response.data)
                    {
                        GameObject cardObj = Instantiate(newsCardPrefab, contentParent);
                        NewsCard card = cardObj.GetComponent<NewsCard>();
                        if(card != null)
                        {
                            card.Setup(newsItem);
                        }
                    }
                    for(int i = 0; i < contents.Length; i++)
                    {
                        contents[i] = response.data[i].content;
                    }
                    contentParent.gameObject.GetComponent<RectTransform>().sizeDelta = CalculateContentSizeDelta(contents);
                }
                else
                {
                    ShowEmpty("Новостей пока нет");
                }
            }
            else
            {
                ShowEmpty("Не удалось загрузить новости");
                Debug.LogError($"[NewsManager] Ошибка запроса: {www.error}");
            }
        }
    }

    private void ShowEmpty(string message)
    {
        if(emptyText != null)
        {
            emptyText.SetText(message);
            emptyText.gameObject.SetActive(true);
        }
    }

    private Vector2 CalculateContentSizeDelta(string[] contents)
    {
        if(contents == null || contents.Length == 0) return Vector2.zero;

        int y = 0;

        for(int i = 0; i < contents.Length; i++)
        {
            string content = contents[i];
            int count = content != null ? content.Length : 0;

            if(count <= 180)
            {
                y += 270;
            } 
            else if(count <= 300)
            {
                y += 540;
            }
            else if(count <= 550)
            {
                y += 700;
            }
            else
            {
                y += 1200;
            }
        }

        return new Vector2(0, y);
    }
}