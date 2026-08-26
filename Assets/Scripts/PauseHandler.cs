using UnityEngine;
using DG.Tweening;
using System.Collections;

public class PauseHandler : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    private bool isPaused = false;
    [SerializeField] private GoalHandler goalHandler;

    public void TogglePause()
    {
        isPaused = !isPaused;
        AnimatePauseMenu();
    }
    private void AnimatePauseMenu()
    {
        if(isPaused)
        {  
            ShowPauseMenu();
        } else
        {
            HidePauseMenu();
        }
    }
    private void HidePauseMenu()
    {
        Time.timeScale = 1.0f;
        var rect = pausePanel.GetComponent<RectTransform>();
        rect.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).SetUpdate(true).OnComplete(() => pausePanel.SetActive(false));    
    }
    private void ShowPauseMenu()
    {
        Time.timeScale = 0f;
        var rect = pausePanel.GetComponent<RectTransform>();
        rect.localScale = Vector3.zero;
        pausePanel.SetActive(true);
        rect.DOScale(new Vector3(1.0f, 1.0f, 1.0f), 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
    }
    public void MainMenu()
    {
        Time.timeScale = 1f;
        PlayerPrefs.SetInt("HowMoneyAdds", 0);
        PlayerPrefs.Save();
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void RestartGame()
    {
        TogglePause();
        goalHandler.RestartGame();
    }
}
