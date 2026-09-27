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
        CycleQuestArray(moneyQuests);
    }

    public void OnClickGoalBtn()
    {
        CycleQuestArray(goalsQuests);
    }

    public void OnClickXpBtn()
    {
        CycleQuestArray(xpQuests);
    }

    private void CycleQuestArray(GameObject[] quests)
    {
        for(int i = 0; i < quests.Length; i++)
        {
            if(quests[i].activeSelf)
            {
                quests[i].SetActive(false);
                
                int nextIndex = (i + 1) % quests.Length;
                
                quests[nextIndex].SetActive(true);
                
                Transform nextTransform = quests[nextIndex].transform; 
                nextTransform.DOScale(1.05f, 0.1f).OnComplete(() => nextTransform.DOScale(1f, 0.1f));
                
                break;
            }
        }
    }
}