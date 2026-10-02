using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class EndScreen : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI loseOrWinText;
    [SerializeField] private TextMeshProUGUI earnedMoneyText;
    
    [Header("Scripts")]
    [SerializeField] private GoalHandler goalHandler;
    [SerializeField] private CoinMover coinMover;

    private WaitForSeconds waitTime = new WaitForSeconds(0.3f);

    public void StartEndScreen(int howManyXpEarned, int xpBeforeWin)
    {
        string sceneName = SceneManager.GetActiveScene().name;
        bool player1Wins = goalHandler.score1 >= goalHandler.howManyGoals;

        if(sceneName.Equals("BotsGame"))
        {
            loseOrWinText.SetText(player1Wins ? "Поражение!" : "Победа!");
            coinMover.AddXp(Vector3.zero, howManyXpEarned, xpBeforeWin);
            StartCoroutine(UpdateText());
        } 
        else if(sceneName.Equals("GameScene"))
        {
            loseOrWinText.SetText(player1Wins ? "Игрок 1 выиграл!" : "Игрок 2 выиграл!");
            earnedMoneyText.SetText("Заработанные деньги: 0");
        }
    }

    private IEnumerator UpdateText()
    {
        for(int i = 0; i < 3; i++)
        {
            earnedMoneyText.SetText("Заработанные деньги: Считаем.");
            yield return waitTime;
            earnedMoneyText.SetText("Заработанные деньги: Считаем..");
            yield return waitTime;
            earnedMoneyText.SetText("Заработанные деньги: Считаем...");
            yield return waitTime;
        }
        earnedMoneyText.SetText("Заработанные деньги: {0}", PlayerPrefs.GetInt("HowMoneyAdds"));
    }
}