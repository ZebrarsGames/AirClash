using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using System.Threading.Tasks;

public class RouletteHandler : MonoBehaviour
{
    [SerializeField] private RouletteItemData[] rouletteItems;
    [SerializeField] private RouletteCell[] rouletteCells;
    [SerializeField] private SkinData[] skins;
    [SerializeField] private GameObject roulettePanel;
    [SerializeField] private Button stopRouletteBtn;
    [SerializeField] private Text awardText;
    [SerializeField] private Transform centerMarker; 
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip rouletteSound;
    public void StartRoulette()
    {
        for(int i = 0; i < rouletteCells.Length; i++)
        {
            int randomIndex = Random.Range(0, rouletteItems.Length);
            rouletteCells[i].SetData(rouletteItems[randomIndex]);
        }
        stopRouletteBtn.interactable = false;
        awardText.gameObject.SetActive(false);
        roulettePanel.SetActive(true);
        SpinRoulette();
    }

    public void SpinRoulette()
    {
        StartCoroutine(MoveRouletteItems());
    }

    IEnumerator MoveRouletteItems()
    {
        int totalSteps = Random.Range(40, 100);
        for (int step = 0; step < totalSteps; step++)
        {
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
                    awardText.text = "ВЫИГРЫШ: " + bestCell.currentData.award + " денег";
                    break;
                case "Skin":
                    foreach(var i in skins)
                    {
                        if(i.skinName == bestCell.currentData.skinAward)
                        {
                            awardText.text = "ВЫИГРЫШ: " + i.skinGuiName;
                        }
                    } 
                    break;
                default:
                    awardText.text = "Неправильный typeOFAward!";
                    break;   
            }
            yield return new WaitForSeconds(1.3f);
            awardText.gameObject.SetActive(false);
            roulettePanel.SetActive(false);
        }
        
    }
}
