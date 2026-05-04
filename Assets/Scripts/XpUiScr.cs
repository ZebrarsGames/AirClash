using System.Collections;
using DG.Tweening;
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
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private GameObject panel;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip levelUpSound;
    void Start()
    {
        if(SceneManager.GetActiveScene().name.Equals("BotsGame"))
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

    public void LevelUpAnimStart()
    {
        StartCoroutine(LevelUpAnim());
    }

    IEnumerator LevelUpAnim()
    {
        yield return new WaitForSeconds(1.1f);
        audioSource.PlayOneShot(levelUpSound);
        panel.transform.DOScale(1.5f, 0.4f).OnComplete(() => panel.transform.DOScale(1.0f, 0.2f));
        canvasGroup.DOFade(1.0f, 0.4f).OnComplete(() => canvasGroup.DOFade(0f, 0.2f));
        SetProgress(xpHandler.GetXPProgress());
    }
}

