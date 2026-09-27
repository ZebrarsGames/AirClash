using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ModificatorsHandler : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI moneyMultiplyText;
    [SerializeField] private Toggle[] modificatorsToggles;
    
    private ModidficatorToggleItem[] modificatorItems;

    private void Awake()
    {
        modificatorItems = new ModidficatorToggleItem[modificatorsToggles.Length];
        for(int i = 0; i < modificatorsToggles.Length; i++)
        {
            modificatorItems[i] = modificatorsToggles[i].GetComponent<ModidficatorToggleItem>();
        }
    }

    public void SetModificators()
    {
        List<string> modificatorsStrings = new List<string>();
        for(int i = 0; i < modificatorsToggles.Length; i++)
        {
            if(modificatorsToggles[i].isOn)
            {
                modificatorsStrings.Add(modificatorItems[i].ModificatorName);
            }
        }
        
        string result = string.Join(",", modificatorsStrings);
        PlayerPrefs.SetString("CurrentModificators", string.IsNullOrEmpty(result) ? "None" : result);
        PlayerPrefs.Save();
    }

    public void OnToggleClicked(Toggle clickedToggle)
    {
        if(clickedToggle.isOn)
        {
            var currentModificator = clickedToggle.GetComponent<ModidficatorToggleItem>();
            
            if(currentModificator.ModificatorOpposite != null)
            {
                currentModificator.ModificatorOpposite.SetIsOnWithoutNotify(false);
            }
        }
        CalculateTotalMoney();
    }

    private void CalculateTotalMoney()
    {
        float modificatorsMultiply = 1f;
        for(int i = 0; i < modificatorsToggles.Length; i++)
        {
            if(modificatorsToggles[i].isOn)
            {
                modificatorsMultiply += modificatorItems[i].ModificatorMultiplyMoney;
            }
        }

        moneyMultiplyText.SetText($"Деньги {modificatorsMultiply}x");
    }
}