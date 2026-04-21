using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class RouletteHandler : MonoBehaviour
{
    public RouletteItemData[] rouletteItems;
    public Image[] rouletteImages;
    [SerializeField] GameObject roulettePanel;
    [SerializeField] Button stopRouletteBtn;
    public void StartRoulette()
    {
        for(int i = 0; i < rouletteImages.Length; i++)
        {
            rouletteImages[i].sprite = rouletteItems[Random.Range(0, rouletteItems.Length)].itemSprite;
        }
        stopRouletteBtn.interactable = false;
        roulettePanel.SetActive(true);
        Debug.Log("Рулетка начата!");
        SpinRoulette();
    }

    public void SpinRoulette()
    {
        Debug.Log("Рулетка крутиться...");
        int result = Random.Range(0, 37);
        Debug.Log("Результат рулетки: " + result);
        StartCoroutine(MoveRouletteItems());
    }

    IEnumerator MoveRouletteItems()
    {
        int totalSteps = Random.Range(40, 100);
        
        for (int step = 0; step < totalSteps; step++)
        {
            float delay = 0.05f + (step / (float)totalSteps) * 0.2f;
            yield return new WaitForSeconds(delay);


            for (int i = 0; i < rouletteImages.Length - 1; i++)
            {
                rouletteImages[i].sprite = rouletteImages[i + 1].sprite;
                
                rouletteImages[i].rectTransform.DOPunchAnchorPos(new Vector2(0, 10), 0.05f);
            }

            rouletteImages[rouletteImages.Length - 1].sprite = rouletteItems[Random.Range(0, rouletteItems.Length)].itemSprite;
        }
        yield return new WaitForSeconds(0.6f);
        stopRouletteBtn.interactable = true;
    }

    public void StopRoulette()
    {
        roulettePanel.SetActive(false);
    }
}
