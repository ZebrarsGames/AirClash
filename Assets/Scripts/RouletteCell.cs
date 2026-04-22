using UnityEngine;
using UnityEngine.UI;

public class RouletteCell : MonoBehaviour
{
    public Image icon;
    public RouletteItemData currentData; // Здесь хранятся данные конкретной ячейки
    public RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void SetData(RouletteItemData data)
    {
        currentData = data;
        icon.sprite = data.itemSprite;
    }
}
