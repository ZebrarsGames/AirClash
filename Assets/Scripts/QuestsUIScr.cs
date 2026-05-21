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
                break;
            }
        }
    }
}