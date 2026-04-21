using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using System.Threading.Tasks;

public class RouletteHandler : MonoBehaviour
{
    public RouletteItemData[] rouletteItems;
    public RouletteCell[] rouletteCells;
    [SerializeField] GameObject roulettePanel;
    [SerializeField] Button stopRouletteBtn;
    [SerializeField] Text awardText;
    public Transform centerMarker; 
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
            float delay = Mathf.Lerp(0.05f, 0.3f, (float)step / totalSteps);
            yield return new WaitForSeconds(delay);

            // Сдвигаем данные всех ячеек
            for (int i = 0; i < rouletteCells.Length - 1; i++)
            {
                rouletteCells[i].SetData(rouletteCells[i + 1].currentData);
            }

            // Последней ячейке даем новые случайные данные
            rouletteCells[rouletteCells.Length - 1].SetData(rouletteItems[Random.Range(0, rouletteItems.Length)]);
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
            awardText.text = "ВЫИГРЫШ: " + bestCell.currentData.typeOfAward;
            yield return new WaitForSeconds(1.3f);
            awardText.gameObject.SetActive(false);
            roulettePanel.SetActive(false);
        }
        
    }
}
