using UnityEngine;

public class MoneyHandler : MonoBehaviour
{
    [SerializeField] private SaveManager saveManager;
    private int money;

    void Awake()
    {
        money = saveManager.GetData().Money;
    }

    public void AddMoney(int amount)
    {
        money += amount;
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

    private void Save()
    {
        saveManager.SaveData();
    }
}