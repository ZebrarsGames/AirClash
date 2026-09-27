using UnityEngine;

public class MoneyHandler : MonoBehaviour
{
    [SerializeField] private SaveManager saveManager;
    private int money;
    private int totalMoney;

    private void Awake()
    {
        var data = saveManager.GetData();
        money = data.Money;
        totalMoney = data.TotalMoney;
    }

    public void AddMoney(int amount)
    {
        money += amount;
        totalMoney += amount;
        Save();
    }

    public void SetMoney(int amount)
    {
        money = amount;
        totalMoney += amount;
        Save();
    }

    public void SetTotalMoney(int amount)
    {
        totalMoney = amount;
        Save();
    }

    public void RemoveMoney(int amount)
    {
        money = Mathf.Max(0, money - amount);
        Save();
    }

    public int GetMoney() => money;
    public int GetTotalMoney() => totalMoney;

    private void Save()
    {
        saveManager.SaveData();
    }
}