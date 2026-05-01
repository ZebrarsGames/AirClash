using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndScreen : MonoBehaviour
{
    [SerializeField] private Text loseOrWinText;
    [SerializeField] private Text earnedMoneyText;
    [SerializeField] private GoalHandler goalHandler;
    public void StartEndScreen()
    {
        if(SceneManager.GetActiveScene().name.Equals("BotsGame"))
        {
            if(goalHandler.score1 >= goalHandler.howManyGoals) loseOrWinText.text = "Поражение!";
            else if(goalHandler.score2 >= goalHandler.howManyGoals) loseOrWinText.text = "Победа!";
            earnedMoneyText.text = "Заработанные деньги: " + goalHandler.howMoneyAdd;
        } else
        {
            if(goalHandler.score1 >= goalHandler.howManyGoals) loseOrWinText.text = "Игрок 1 выиграл!";
            else if(goalHandler.score2 >= goalHandler.howManyGoals) loseOrWinText.text = "Игрок 2 выиграл!";
        }
        
    }
}
