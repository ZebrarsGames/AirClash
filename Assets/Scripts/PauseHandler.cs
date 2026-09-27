using UnityEngine;
using DG.Tweening;

public class PauseHandler : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GoalHandler goalHandler;
    
    private bool isPaused = false;
    private RectTransform pausePanelRect;

    private void Awake()
    {
        pausePanelRect = pausePanel.GetComponent<RectTransform>();
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        AnimatePauseMenu();
    }

    private void AnimatePauseMenu()
    {
        if(isPaused) ShowPauseMenu();
        else HidePauseMenu();
    }

    private void HidePauseMenu()
    {
        Time.timeScale = 1.0f;
        pausePanelRect.DOScale(Vector3.zero, 0.3f)
            .SetEase(Ease.InBack)
            .SetUpdate(true)
            .OnComplete(() => pausePanel.SetActive(false));    
    }

    private void ShowPauseMenu()
    {
        Time.timeScale = 0f;
        pausePanelRect.localScale = Vector3.zero;
        pausePanel.SetActive(true);
        pausePanelRect.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
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