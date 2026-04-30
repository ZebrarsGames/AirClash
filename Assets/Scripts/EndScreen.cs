using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndScreen : MonoBehaviour
{
    [SerializeField] private Text loseOrWinText;
    [SerializeField] private Slider xpProgressbar;
    [SerializeField] private Text moneyText;
    [SerializeField] private Text xpText;
    [SerializeField] private XpHandler xpHandler;
    [SerializeField] private GoalHandler goalHandler;
    [SerializeField] private MoneyHandler moneyHandler;
    [SerializeField] private CoinMover coinMover;
    public void StartEndScreen()
    {
        if(SceneManager.GetActiveScene().name.Equals("BotsGame"))
        {
            if(goalHandler.score1 >= goalHandler.howManyGoals) loseOrWinText.text = "Поражение!";
            else if(goalHandler.score2 >= goalHandler.howManyGoals) loseOrWinText.text = "Победа!";
            xpProgressbar.value = xpHandler.GetXPProgress();
            xpText.text = xpHandler.GetXP().ToString();
            moneyText.text = moneyHandler.GetMoney().ToString();
            coinMover.AddCoins(new Vector3(0, 0, 0), PlayerPrefs.GetInt("HowMoneyAdds"), moneyText.gameObject.transform);
        } else
        {
            if(goalHandler.score1 >= goalHandler.howManyGoals) loseOrWinText.text = "Игрок 1 выиграл!";
            else if(goalHandler.score2 >= goalHandler.howManyGoals) loseOrWinText.text = "Игрок 2 выиграл!";
        }
        
    }
}
