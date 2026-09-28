using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using TMPro;

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
    [SerializeField] private GameObject choiceRoulettePanel;
    [SerializeField] private Transform centerMarker;
    [SerializeField] private Button stopRouletteBtn;
    [SerializeField] private TextMeshProUGUI awardText;

    [Header("Economy")]
    public int rouletteCost;
    [SerializeField] private MoneyHandler moneyHandler;
    [SerializeField] private TextMeshProUGUI moneyText;

    [Header("Sounds Effects")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip rouletteSound;
    [SerializeField] private AudioClip cancelSound;
    [SerializeField] private AudioClip endSound;
    
    [Header("Scripts")]
    [SerializeField] private AchievementsHandler achievementsHandler;
    [SerializeField] private XpHandler xpHandler;
    [SerializeField] private QuestsHandler questsHandler;
    [SerializeField] private DailyQuestHandler dailyQuestHandler;

    private CanvasGroup roulettePanelGroup;
    private CanvasGroup choiceRoulettePanelGroup;
    
    private Color commonColor;
    private Color epicColor;
    private Color legendaryColor;

    private readonly WaitForSeconds wait1_5f = new WaitForSeconds(1.5f);
    private readonly WaitForSeconds wait1_3f = new WaitForSeconds(1.3f);
    private readonly WaitForSeconds wait1_0f = new WaitForSeconds(1f);
    private readonly WaitForSeconds wait0_7f = new WaitForSeconds(0.7f);
    private readonly WaitForSeconds wait0_3f = new WaitForSeconds(0.3f);

    private void Awake()
    {
        roulettePanelGroup = roulettePanel.GetComponent<CanvasGroup>();
        choiceRoulettePanelGroup = choiceRoulettePanel.GetComponent<CanvasGroup>();

        ColorUtility.TryParseHtmlString("#56A1CC", out commonColor); commonColor.a = 0.39f;
        ColorUtility.TryParseHtmlString("#ff00d4", out epicColor); epicColor.a = 0.39f;
        ColorUtility.TryParseHtmlString("#FF0000", out legendaryColor); legendaryColor.a = 0.39f;
    }

    public void StartRoulette(string typeOfRoulette)
    {
        RouletteItemData[] itemsToUse = null;
        Color targetColor = default;

        switch(typeOfRoulette)
        {
            case "Common":
                rouletteCost = 25;
                itemsToUse = rouletteItems;
                targetColor = commonColor;
                break;
            case "Epic":
                rouletteCost = 50;
                itemsToUse = rareRouletteItems;
                targetColor = epicColor;
                break;
            case "Legendary":
                rouletteCost = 100;
                itemsToUse = veryRareRouletteItems;
                targetColor = legendaryColor;
                break;
            default:
                Debug.Log($"Неправильный typeOfRoulette! ({typeOfRoulette})");
                return;
        }

        if(moneyHandler.GetMoney() >= rouletteCost)
        {
            roulettePanel.SetActive(true);
            achievementsHandler.UpdateProgress("ludoman", 1);
            
            for(int i = 0; i < rouletteCells.Length; i++)
            {
                int randomIndex = Random.Range(0, itemsToUse.Length);
                rouletteCells[i].SetData(itemsToUse[randomIndex]);
                rouletteCells[i].cellBg.color = targetColor;
            }

            stopRouletteBtn.interactable = false;
            awardText.gameObject.SetActive(false);
            roulettePanelGroup.alpha = 0;
            roulettePanelGroup.DOFade(1f, 1f);
            
            moneyHandler.RemoveMoney(rouletteCost);
            moneyText.SetText($"{moneyHandler.GetMoney()} <sprite=0>");

            StartCoroutine(SpinRouletteRoutine(typeOfRoulette));
        }
        else
        {
            audioSource.PlayOneShot(cancelSound);
        }
    }

    IEnumerator SpinRouletteRoutine(string typeOfRoulette)
    {
        yield return wait1_5f;
        
        if(typeOfRoulette.Equals("Common") || typeOfRoulette.Equals("Epic") || typeOfRoulette.Equals("Legendary"))
        {
            dailyQuestHandler.UpdateQuestProgress("open_roulette", 1);
            if(typeOfRoulette.Equals("Legendary"))
            {
                dailyQuestHandler.UpdateQuestProgress("open_legendary_roulette", 1);
            }
        }
        
        StartCoroutine(MoveRouletteItems(typeOfRoulette));
    }

    IEnumerator MoveRouletteItems(string typeOfRoulette)
    {
        int totalSteps = Random.Range(40, 100);
        int randKoof = 0;
        
        for(int step = 0; step < totalSteps; step++)
        {
            if(this == null || !gameObject.activeInHierarchy) yield break;
            
            audioSource.PlayOneShot(rouletteSound);
            float delay = Mathf.Lerp(0.05f, 0.2f, (float)step / totalSteps);
            yield return new WaitForSeconds(delay);

            for(int i = 0; i < rouletteCells.Length - 1; i++)
            {
                rouletteCells[i].SetData(rouletteCells[i + 1].currentData);
                rouletteCells[i].cellBg.color = rouletteCells[i + 1].cellBg.color;
            }

            switch(typeOfRoulette)
            {
                case "Common": randKoof = Random.Range(0, 17); break;
                case "Epic": randKoof = Random.Range(14, 24); break;
                case "Legendary": randKoof = Random.Range(23, 28); break;
            }

            RouletteCell lastCell = rouletteCells[rouletteCells.Length - 1];

            if(randKoof <= 15)
            {
                lastCell.SetData(rouletteItems[Random.Range(0, rouletteItems.Length)]);
                lastCell.cellBg.color = commonColor;
            }
            else if(randKoof <= 23)
            {
                lastCell.SetData(rareRouletteItems[Random.Range(0, rareRouletteItems.Length)]);
                lastCell.cellBg.color = epicColor;
            }
            else
            {
                lastCell.SetData(veryRareRouletteItems[Random.Range(0, veryRareRouletteItems.Length)]);
                lastCell.cellBg.color = legendaryColor;
            }

            foreach(var cell in rouletteCells)
            {
                cell.rectTransform.DOComplete(); 
                cell.rectTransform.DOPunchScale(new Vector3(0.05f, 0.05f, 0.05f), 0.05f, 1, 0.5f);
                VibrationHandler.Vibrate(15, 35);
            }
        }
        
        yield return wait0_7f;
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

        foreach(var cell in rouletteCells)
        {
            float dist = Vector3.Distance(cell.transform.position, centerMarker.position);
            if(dist < minDistance)
            {
                minDistance = dist;
                bestCell = cell;
            }
        }

        if(bestCell != null && bestCell.currentData != null)
        {
            awardText.gameObject.SetActive(true);
            var data = bestCell.currentData;

            switch(data.typeOfAward)
            {
                case "Money":
                    if(data.award == 67) achievementsHandler.UpdateProgress("six_seven", 1);
                    awardText.text = $"ВЫИГРЫШ: {data.award} монет";
                    moneyHandler.AddMoney(data.award);
                    UpdateQuests(data.award);
                    moneyText.SetText($"{moneyHandler.GetMoney()} <sprite=0>");
                    break;
                    
                case "Skin":
                    foreach(var i in skins)
                    {
                        if(i.skinName == data.skinAward)
                        {
                            if(i.isBuy)
                            {
                                awardText.text = $"ВЫИГРЫШ: {i.skinPrice} монет (скин уже получен)";
                                moneyHandler.AddMoney(i.skinPrice);
                                UpdateQuests(i.skinPrice);
                                moneyText.SetText($"{moneyHandler.GetMoney()} <sprite=0>");
                            }
                            else
                            {
                                achievementsHandler.UpdateProgress("large_wardrobe", 1);
                                awardText.text = $"ВЫИГРЫШ: {i.guiSkinName}";
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
                    xpHandler.AddXp(data.award);
                    awardText.text = $"ВЫИГРЫШ: {data.award} XP";
                    UpdateXpQuests(data.award);
                    break; 
                    
                default:
                    awardText.text = "Неправильный typeOFAward!";
                    break; 
            }
            
            yield return wait1_3f;
            roulettePanelGroup.alpha = 1f;
            roulettePanelGroup.DOFade(0f, 1f);
            
            yield return wait1_0f;
            awardText.gameObject.SetActive(false);
            roulettePanel.SetActive(false);
        }
    }

    public void CloseChoiceRoulettePanel()
    {
        StartCoroutine(CloseChoiceRoulettePanelAnim());
    }

    IEnumerator CloseChoiceRoulettePanelAnim()
    {
        choiceRoulettePanelGroup.alpha = 1;
        choiceRoulettePanelGroup.DOFade(0.0f, 0.2f);
        yield return wait0_3f;
        choiceRoulettePanel.SetActive(false);
    }

    public void OpenChoiceRoulettePanel()
    {
        choiceRoulettePanel.SetActive(true);
        choiceRoulettePanelGroup.alpha = 0;
        choiceRoulettePanelGroup.DOFade(1.0f, 0.2f);
    }

    private void UpdateQuests(int amount)
    {
        questsHandler.UpdateQuestProgress("money10", amount);
        questsHandler.UpdateQuestProgress("money50", amount);
        questsHandler.UpdateQuestProgress("money100", amount);
        questsHandler.UpdateQuestProgress("money200", amount);
        questsHandler.UpdateQuestProgress("money300", amount);
        questsHandler.UpdateQuestProgress("money500", amount);
        dailyQuestHandler.UpdateQuestProgress("money50", amount);
        dailyQuestHandler.UpdateQuestProgress("money70", amount);
        dailyQuestHandler.UpdateQuestProgress("money100", amount);
    }

    private void UpdateXpQuests(int amount)
    {
        questsHandler.UpdateQuestProgress("xp100", amount);
        questsHandler.UpdateQuestProgress("xp200", amount);
        questsHandler.UpdateQuestProgress("xp400", amount);
        questsHandler.UpdateQuestProgress("xp500", amount);
        questsHandler.UpdateQuestProgress("xp700", amount);
        questsHandler.UpdateQuestProgress("xp1000", amount);
        dailyQuestHandler.UpdateQuestProgress("xp50", amount);
    }
}