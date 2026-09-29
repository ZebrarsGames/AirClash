using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimerScr : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject timerPanel;
    [SerializeField] private Button pauseBtn;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] goalSounds;
    [SerializeField] private AudioClip timerSound;

    [Header("Settings")]
    [SerializeField] private int startSeconds = 4;
    public UnityEngine.Events.UnityEvent OnTimerEnd;

    [HideInInspector] public bool TimerOn = false;
    private int TimeLeft;

    private static readonly string[] CachedNumbers = { "0", "1", "2", "3" };
    private static readonly string GoalText = "GOAL";
    private Coroutine timerCoroutine;

    public void Goal()
    {
        if(timerCoroutine != null) StopCoroutine(timerCoroutine);

        timerPanel.SetActive(true);
        timerText.SetText(GoalText);
        TimerOn = true;
        if(pauseBtn != null) pauseBtn.interactable = false;

        if(goalSounds != null && goalSounds.Length > 0 && audioSource != null)
        {
            int rand = Random.Range(0, goalSounds.Length);
            audioSource.PlayOneShot(goalSounds[rand]);
        }

        timerCoroutine = StartCoroutine(GoalSequence());
    }

    private IEnumerator GoalSequence()
    {
        yield return WaitForSecondsCache.Wait(1f);
        
        yield return TimerCountdownSequence();
    }

    public void TimerStart()
    {
        if(timerCoroutine != null) StopCoroutine(timerCoroutine);
        
        timerPanel.SetActive(true);
        TimerOn = true;
        if(pauseBtn != null) pauseBtn.interactable = false;

        timerCoroutine = StartCoroutine(TimerCountdownSequence());
    }

    private IEnumerator TimerCountdownSequence()
    {
        TimeLeft = startSeconds;

        while(TimeLeft > 0)
        {
            timerText.SetText((TimeLeft < CachedNumbers.Length) ? CachedNumbers[TimeLeft] : TimeLeft.ToString());
            
            if(audioSource != null && timerSound != null)
            {
                audioSource.PlayOneShot(timerSound);
                VibrationHandler.Vibrate(15, 10);
            }

            yield return WaitForSecondsCache.Wait(1f);
            TimeLeft--;
        }

        if(audioSource != null && timerSound != null)
        {
            audioSource.PlayOneShot(timerSound);
        }

        timerPanel.SetActive(false);
        TimerOn = false;
        OnTimerEnd.Invoke();
        if(pauseBtn != null) pauseBtn.interactable = true;
        timerCoroutine = null;
    }
}

public static class WaitForSecondsCache
{
    private static readonly System.Collections.Generic.Dictionary<float, WaitForSeconds> TimeDictionary = new();

    public static WaitForSeconds Wait(float seconds)
    {
        if(!TimeDictionary.TryGetValue(seconds, out var waitForSeconds))
        {
            waitForSeconds = new WaitForSeconds(seconds);
            TimeDictionary.Add(seconds, waitForSeconds);
        }
        return waitForSeconds;
    }
}