using UnityEngine;
using DG.Tweening;
using System.Collections;

public class PauseHandler : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
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
            pauseMenu.SetActive(true);
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
        rect.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).SetUpdate(true).OnComplete(() =>
        {
            pausePanel.SetActive(false);
            pauseMenu.SetActive(false);
        });    
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
        PlayerPrefs.SetInt("HowMoneyAdds", 0);
        PlayerPrefs.Save();
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void RestartGame()
    {
        TogglePause();
        goalHandler.RestartGame();
    }
}
