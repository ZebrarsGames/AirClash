using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class RouletteHandler : MonoBehaviour
{
    [SerializeField] private RouletteItemData[] rouletteItems;
    [SerializeField] private RouletteCell[] rouletteCells;
    [SerializeField] private SkinItem[] skins;
    [SerializeField] private GameObject roulettePanel;
    [SerializeField] private Button stopRouletteBtn;
    [SerializeField] private Text awardText;
    [SerializeField] private Transform centerMarker; 
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip rouletteSound;
    [SerializeField] private MoneyHandler moneyHandler;
    [SerializeField] private Text moneyText;
    [SerializeField] private AudioClip cancelSound;
    [SerializeField] private AudioClip buySound;
    public int rouletteCost;
    public void StartRoulette()
    {
        if(moneyHandler.GetMoney() >= rouletteCost)
        {
            audioSource.PlayOneShot(buySound);
            moneyHandler.RemoveMoney(rouletteCost);
            moneyText.text = "Деньги " + moneyHandler.GetMoney();
            for(int i = 0; i < rouletteCells.Length; i++)
            {
                int randomIndex = Random.Range(0, rouletteItems.Length);
                rouletteCells[i].SetData(rouletteItems[randomIndex]);
            }
            stopRouletteBtn.interactable = false;
            awardText.gameObject.SetActive(false);
            roulettePanel.SetActive(true);
            roulettePanel.GetComponent<CanvasGroup>().alpha = 0;
            roulettePanel.GetComponent<CanvasGroup>().DOFade(1f, 1f);
            StartCoroutine(SpinRoulette());
        } else
        {
            audioSource.PlayOneShot(cancelSound);
        }
        
    }

    IEnumerator SpinRoulette()
    {
        yield return new WaitForSeconds(1.5f);
        StartCoroutine(MoveRouletteItems());
    }

    IEnumerator MoveRouletteItems()
    {
        int totalSteps = Random.Range(40, 100);
        for (int step = 0; step < totalSteps; step++)
        {
            if (this == null || !gameObject.activeInHierarchy) yield break;
            audioSource.PlayOneShot(rouletteSound);
            float delay = Mathf.Lerp(0.05f, 0.2f, (float)step / totalSteps);
            yield return new WaitForSeconds(delay);

            // Сдвигаем данные всех ячеек
            for (int i = 0; i < rouletteCells.Length - 1; i++)
            {
                rouletteCells[i].SetData(rouletteCells[i + 1].currentData);
            }

            // Последней ячейке даем новые случайные данные
            rouletteCells[rouletteCells.Length - 1].SetData(rouletteItems[Random.Range(0, rouletteItems.Length)]);

            foreach (var cell in rouletteCells)
            {
                cell.rectTransform.DOComplete(); 
                // Подпрыгивание через масштаб (увеличение на 10% и возврат)
                cell.rectTransform.DOPunchScale(new Vector3(0.05f, 0.05f, 0.05f), 0.05f, 1, 0.5f);
            }

        }
        yield return new WaitForSeconds(0.7f);
        stopRouletteBtn.interactable = true;
    }

    public void StopRoulette()
    {
        StartCoroutine(GetReward());
    }

    IEnumerator GetReward()
    {
        stopRouletteBtn.interactable = false;
        RouletteCell bestCell = null;
        float minDistance = float.MaxValue;

        foreach (var cell in rouletteCells)
        {
            float dist = Vector3.Distance(cell.transform.position, centerMarker.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                bestCell = cell;
            }
        }

        if (bestCell != null && bestCell.currentData != null)
        {
            awardText.gameObject.SetActive(true);
            switch(bestCell.currentData.typeOfAward)
            {
                case "Money":
                    awardText.text = "ВЫИГРЫШ: " + bestCell.currentData.award + " монет";
                    moneyHandler.AddMoney(bestCell.currentData.award);
                    moneyText.text = "Деньги " + moneyHandler.GetMoney();
                    break;
                case "Skin":
                    foreach(var i in skins)
                    {
                        if(i.skinName == bestCell.currentData.skinAward)
                        {
                            if(i.isBuy)
                            {
                                awardText.text = "ВЫИГРЫШ: " + i.skinPrice + " (скин уже куплен)";
                                moneyHandler.AddMoney(i.skinPrice);
                                moneyText.text = "Деньги " + moneyHandler.GetMoney();
                            } else
                            {
                                awardText.text = "ВЫИГРЫШ: " + i.guiSkinName;
                                i.isBuy = true;
                                i.checkmark.gameObject.SetActive(true);
                            }
                            break;
                        }
                    } 
                    break;
                default:
                    awardText.text = "Неправильный typeOFAward!";
                    break;   
            }
            yield return new WaitForSeconds(1.3f);
            roulettePanel.GetComponent<CanvasGroup>().alpha = 1f;
            roulettePanel.GetComponent<CanvasGroup>().DOFade(0f, 1f);
            yield return new WaitForSeconds(1.2f);
            awardText.gameObject.SetActive(false);
            roulettePanel.SetActive(false);
        }
        
    }
}
