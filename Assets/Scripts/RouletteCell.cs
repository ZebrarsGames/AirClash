using UnityEngine;
using UnityEngine.UI;

public class RouletteCell : MonoBehaviour
{
    public Image icon;
    public RouletteItemData currentData; // Здесь хранятся данные конкретной ячейки
    public RectTransform rectTransform;
    public Image cellBg;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        cellBg = GetComponent<Image>();
    }

    public void SetData(RouletteItemData data)
    {
        currentData = data;
        icon.sprite = data.itemSprite;
    }
}
