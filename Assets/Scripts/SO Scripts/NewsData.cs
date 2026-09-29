using System;
using System.Collections.Generic;

[System.Serializable]
public class NewsItem
{
    public string id;
    public string title;
    public string content;
    public string tag;
    public string createdAt;
}

[Serializable]
public class NewsResponse
{
    public bool success;
    public int count;
    public List<NewsItem> data;
}