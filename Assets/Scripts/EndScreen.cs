using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndScreen : MonoBehaviour
{
    [SerializeField] private Text loseOrWinText;
    [SerializeField] private Text earnedMoneyText;
    [SerializeField] private GoalHandler goalHandler;
    [SerializeField] private CoinMover coinMover;
    public void StartEndScreen(int howManyXpEarned, int xpBeforeWin)
    {
        if(SceneManager.GetActiveScene().name.Equals("BotsGame"))
        {
            if(goalHandler.score1 >= goalHandler.howManyGoals) loseOrWinText.text = "Поражение!";
            else if(goalHandler.score2 >= goalHandler.howManyGoals) loseOrWinText.text = "Победа!";
            coinMover.AddXp(Vector3.zero, howManyXpEarned, xpBeforeWin);
            StartCoroutine(UpdateText());
        } else
        {
            if(goalHandler.score1 >= goalHandler.howManyGoals) loseOrWinText.text = "Игрок 1 выиграл!";
            else if(goalHandler.score2 >= goalHandler.howManyGoals) loseOrWinText.text = "Игрок 2 выиграл!";
        }
        
    }

    IEnumerator UpdateText()
    {
        for(int i = 0; i <= 3; i++)
        {
            earnedMoneyText.text = "Заработанные деньги: Считаем.";
            yield return new WaitForSeconds(0.3f);
            earnedMoneyText.text = "Заработанные деньги: Считаем..";
            yield return new WaitForSeconds(0.3f);
            earnedMoneyText.text = "Заработанные деньги: Считаем...";
            yield return new WaitForSeconds(0.3f);
        }
        earnedMoneyText.text = "Заработанные деньги: " + PlayerPrefs.GetInt("HowMoneyAdds").ToString();
    }
}
