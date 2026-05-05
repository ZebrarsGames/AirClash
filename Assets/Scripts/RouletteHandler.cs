using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class RouletteHandler : MonoBehaviour
{
    [Header("Arrays")]
    [SerializeField] private RouletteItemData[] rouletteItems;
    [SerializeField] private RouletteItemData[] rareRouletteItems;
    [SerializeField] private RouletteItemData[] veryRareRouletteItems;
    [SerializeField] private SkinItem[] skins;

    [Header("Components of Roulette")]
    [SerializeField] private RouletteCell[] rouletteCells;
    [SerializeField] private GameObject roulettePanel;
    [SerializeField] private Transform centerMarker;
    [SerializeField] private Button stopRouletteBtn;
    [SerializeField] private Text awardText;

    [Header("Economy")]
    public int rouletteCost;
    [SerializeField] private MoneyHandler moneyHandler;
    [SerializeField] private Text moneyText;

    [Header("Sounds Effects")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip rouletteSound;
    [SerializeField] private AudioClip cancelSound;
    [SerializeField] private AudioClip endSound;
    
    [Header("Scripts")]
    [SerializeField] private AchievementsHandler achievementsHandler;
    [SerializeField] private XpHandler xpHandler;

    public void StartRoulette()
    {
        if(moneyHandler.GetMoney() >= rouletteCost)
        {
            achievementsHandler.UpdateProgress("ludoman", 1);
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
        int randKoof = 0;
        for (int step = 0; step < totalSteps; step++)
        {
            if (this == null || !gameObject.activeInHierarchy) yield break;
            audioSource.PlayOneShot(rouletteSound);
            float delay = Mathf.Lerp(0.05f, 0.2f, (float)step / totalSteps);
            yield return new WaitForSeconds(delay);

            for (int i = 0; i < rouletteCells.Length - 1; i++)
            {
                rouletteCells[i].SetData(rouletteCells[i + 1].currentData);
                rouletteCells[i].cellBg.color = rouletteCells[i + 1].cellBg.color;
            }
            randKoof = Random.Range(0, 26);
            if (randKoof >= 0 && randKoof <= 15)
            {
                rouletteCells[rouletteCells.Length - 1].SetData(rouletteItems[Random.Range(0, rouletteItems.Length)]);
                if (ColorUtility.TryParseHtmlString("#56A1CC", out Color cellColor))
                {
                    cellColor.a = 0.39f;
                    rouletteCells[rouletteCells.Length - 1].cellBg.color = cellColor;
                }
            }
            else if (randKoof >= 16 && randKoof <= 23)
            {
                rouletteCells[rouletteCells.Length - 1].SetData(rareRouletteItems[Random.Range(0, rareRouletteItems.Length)]);
                if (ColorUtility.TryParseHtmlString("#ff00d4", out Color cellColor))
                {
                    cellColor.a = 0.39f;
                    rouletteCells[rouletteCells.Length - 1].cellBg.color = cellColor;
                }
            }
            else if (randKoof >= 24)
            {
                rouletteCells[rouletteCells.Length - 1].SetData(veryRareRouletteItems[Random.Range(0, veryRareRouletteItems.Length)]);
                if (ColorUtility.TryParseHtmlString("#FF0000", out Color cellColor))
                {
                    cellColor.a = 0.39f;
                    rouletteCells[rouletteCells.Length - 1].cellBg.color = cellColor;
                }
            }
            foreach (var cell in rouletteCells)
            {
                cell.rectTransform.DOComplete(); 
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
        audioSource.PlayOneShot(endSound);
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
                    if(bestCell.currentData.award == 67) achievementsHandler.UpdateProgress("six_seven", 1);
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
                                awardText.text = "ВЫИГРЫШ: " + i.skinPrice + " монет (скин уже куплен)";
                                moneyHandler.AddMoney(i.skinPrice);
                                moneyText.text = "Деньги " + moneyHandler.GetMoney();
                            } else
                            {
                                awardText.text = "ВЫИГРЫШ: " + i.guiSkinName;
                                if(i.skinName.Equals("GoldSkin")) achievementsHandler.UpdateProgress("lucky", 1);
                                i.isBuy = true;
                                i.checkmark.gameObject.SetActive(true);
                                PlayerPrefs.SetInt(i.skinName, 1);
                                PlayerPrefs.Save();
                            }
                            break;
                        }
                    } 
                    break;
                case "Xp":
                    xpHandler.AddXp(bestCell.currentData.award);
                    awardText.text = "ВЫИГРЫШ: " + bestCell.currentData.award + " XP";
                    break;    
                default:
                    awardText.text = "Неправильный typeOFAward!";
                    break;   
            }
            yield return new WaitForSeconds(1.3f);
            roulettePanel.GetComponent<CanvasGroup>().alpha = 1f;
            roulettePanel.GetComponent<CanvasGroup>().DOFade(0f, 1f);
            yield return new WaitForSeconds(1f);
            awardText.gameObject.SetActive(false);
            roulettePanel.SetActive(false);
        }
        
    }
}
