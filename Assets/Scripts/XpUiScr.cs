using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class XpUiScr : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider xpSlider;
    [SerializeField] private TextMeshProUGUI currentXpText;
    [SerializeField] private TextMeshProUGUI currentLvlText;
    [SerializeField] private TextMeshProUGUI nextLvlText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private GameObject panel;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] levelUpSounds;
    
    [Header("Scripts")]
    [SerializeField] private XpHandler xpHandler;

    private bool isAnim;
    
    private readonly WaitForSeconds wait1_1f = new WaitForSeconds(1.1f);
    private readonly WaitForSeconds wait0_2f = new WaitForSeconds(0.2f);

    public bool GetIsAnim() => isAnim;

    void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        
        if(sceneName.Equals("BotsGame"))
        {
            SetOldProgress(xpHandler.GetOldXPProgress());
        }
        else if(sceneName.Equals("GameScene"))
        {
            SetProgress(xpHandler.GetXPProgress());
        }
        else
        {
            SetProgress(xpHandler.GetXPProgress());
        }
    }

    public void SetProgress(float progress)
    {
        xpSlider.value = progress;
        currentXpText.text = $"{xpHandler.GetXP()} / {xpHandler.GetXpToNextLevel()} XP";
        currentLvlText.text = xpHandler.GetLevel().ToString();
        nextLvlText.text = (xpHandler.GetLevel() + 1).ToString();
    }

    public void SetProgress(float progress, int currentXP)
    {
        xpSlider.value = progress;
        currentXpText.text = $"{currentXP} / {xpHandler.GetXpToNextLevel()} XP";
        currentLvlText.text = xpHandler.GetLevel().ToString();
        nextLvlText.text = (xpHandler.GetLevel() + 1).ToString();
    }

    public void SetOldProgress(float progress)
    {
        xpSlider.value = progress;
        if(currentXpText == null) 
        {
            Debug.Log("currentXpText = null"); 
            return;
        }
        currentXpText.text = $"{xpHandler.GetOldXP()} / {xpHandler.GetXpToNextLevel()} XP";
        currentLvlText.text = xpHandler.GetLevel().ToString();
        nextLvlText.text = (xpHandler.GetLevel() + 1).ToString();
    }

    public void LevelUpAnimStart()
    {
        if(panel == null) return;
        StartCoroutine(LevelUpAnim());
    }

    IEnumerator LevelUpAnim()
    {
        isAnim = true;
        yield return wait1_1f;
        
        int rand = UnityEngine.Random.Range(0, levelUpSounds.Length);
        audioSource.PlayOneShot(levelUpSounds[rand]);
        
        panel.transform.DOScale(1.5f, 0.4f).OnComplete(() => panel.transform.DOScale(1.0f, 0.2f));
        canvasGroup.DOFade(1.0f, 0.4f).OnComplete(() => canvasGroup.DOFade(0f, 0.2f));
        
        yield return wait0_2f;
        SetProgress(xpHandler.GetXPProgress());
        isAnim = false;
    }
}