using UnityEngine;
using TMPro;
using System;

public class NewsCard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI tagText;
    [SerializeField] private TextMeshProUGUI contentText;
    [SerializeField] private TextMeshProUGUI dateText;

    public void Setup(NewsItem item)
    {
        if(titleText != null) titleText.text = item.title;
        if(tagText != null) tagText.text = item.tag;
        if(contentText != null) contentText.text = item.content;

        if(dateText != null && !string.IsNullOrEmpty(item.createdAt))
        {
            if(DateTime.TryParse(item.createdAt, out DateTime utcDate))
            {
                DateTime localDate = utcDate.ToLocalTime();
                
                dateText.text = localDate.ToString("dd.MM.yyyy HH:mm");
            }
            else
            {
                dateText.text = "";
            }
        }
    }
}