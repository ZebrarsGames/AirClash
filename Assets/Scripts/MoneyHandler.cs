using UnityEngine;

public class MoneyHandler : MonoBehaviour
{
    [SerializeField] private SaveManager saveManager;
    private int money;
    private int totalMoney;

    void Awake()
    {
        money = saveManager.GetData().Money;
        totalMoney = saveManager.GetData().TotalMoney;
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
        money -= amount;
        money = Mathf.Max(0, money);
        Save();
    }

    public int GetMoney()
    {
        return money;
    }

    public int GetTotalMoney()
    {
        return totalMoney;
    }

    private void Save()
    {
        saveManager.SaveData();
    }
}