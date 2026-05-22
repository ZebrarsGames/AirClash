using DG.Tweening;
using UnityEngine;

public class QuestsUIScr : MonoBehaviour
{
    [Header("Arrays")]
    [SerializeField] private GameObject[] moneyQuests;
    [SerializeField] private GameObject[] goalsQuests;
    [SerializeField] private GameObject[] xpQuests;

    public void OnClickMoneyBtn()
    {
        for (int i = 0; i < moneyQuests.Length; i++)
        {
            if (moneyQuests[i].activeSelf)
            {
                moneyQuests[i].SetActive(false);
                
                int nextIndex = (i + 1) % moneyQuests.Length;
                
                moneyQuests[nextIndex].SetActive(true);
                moneyQuests[nextIndex].GetComponent<RectTransform>().DOScale(1.05f, 0.1f).OnComplete(() => moneyQuests[nextIndex].GetComponent<RectTransform>().DOScale(1f, 0.1f));
                break;
            }
        }
    }

    public void OnClickGoalBtn()
    {
        for (int i = 0; i < goalsQuests.Length; i++)
        {
            if (goalsQuests[i].activeSelf)
            {
                goalsQuests[i].SetActive(false);
                
                int nextIndex = (i + 1) % goalsQuests.Length;
                
                goalsQuests[nextIndex].SetActive(true);
                goalsQuests[nextIndex].GetComponent<RectTransform>().DOScale(1.05f, 0.1f).OnComplete(() => goalsQuests[nextIndex].GetComponent<RectTransform>().DOScale(1f, 0.1f));
                break;
            }
        }
    }

    public void OnClickXpBtn()
    {
        for (int i = 0; i < xpQuests.Length; i++)
        {
            if (xpQuests[i].activeSelf)
            {
                xpQuests[i].SetActive(false);
                
                int nextIndex = (i + 1) % xpQuests.Length;
                
                xpQuests[nextIndex].SetActive(true);
                xpQuests[nextIndex].GetComponent<RectTransform>().DOScale(1.05f, 0.1f).OnComplete(() => xpQuests[nextIndex].GetComponent<RectTransform>().DOScale(1f, 0.1f));
                break;
            }
        }
    }
}