using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class XpUiScr : MonoBehaviour
{
    [SerializeField] private Slider xpSlider;
    [SerializeField] private XpHandler xpHandler;
    [SerializeField] private Text currentXpText;
    [SerializeField] private Text currentLvlText;
    [SerializeField] private Text nextLvlText;
    void Start()
    {
        if(SceneManager.GetActiveScene().name.Equals("BotsGame") || SceneManager.GetActiveScene().name.Equals("GameScene"))
        {
            SetOldProgress(xpHandler.GetOldXPProgress());
        } else
        {
            SetProgress(xpHandler.GetXPProgress());
        }
    }

    public void SetProgress(float progress)
    {
        xpSlider.value = progress;
        currentXpText.text = xpHandler.GetXP().ToString() + " / " + xpHandler.GetXpToNextLevel().ToString() + " XP";
        currentLvlText.text = xpHandler.GetLevel().ToString();
        nextLvlText.text = (xpHandler.GetLevel() + 1).ToString();
    }
    public void SetProgress(float progress, int currentXP)
    {
        xpSlider.value = progress;
        currentXpText.text = currentXP.ToString() + " / " + xpHandler.GetXpToNextLevel().ToString() + " XP";
        currentLvlText.text = xpHandler.GetLevel().ToString();
        nextLvlText.text = (xpHandler.GetLevel() + 1).ToString();
    }
    public void SetOldProgress(float progress)
    {
        xpSlider.value = progress;
        currentXpText.text = xpHandler.GetOldXP().ToString() + " / " + xpHandler.GetXpToNextLevel().ToString() + " XP";
        currentLvlText.text = xpHandler.GetLevel().ToString();
        nextLvlText.text = (xpHandler.GetLevel() + 1).ToString();
    }
}

