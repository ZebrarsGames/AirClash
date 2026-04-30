using UnityEngine;
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
        SetProgress(xpHandler.GetXPProgress());
        currentXpText.text = xpHandler.GetXP().ToString() + " / " + xpHandler.GetXpToNextLevel().ToString() + " XP";
        currentLvlText.text = xpHandler.GetLevel().ToString();
        nextLvlText.text = (xpHandler.GetLevel() + 1).ToString();
    }

    public void SetProgress(float progress)
    {
        xpSlider.value = progress;
    }
}

