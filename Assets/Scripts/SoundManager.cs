using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [Header("Sounds")]
    [SerializeField] private AudioClip whooshSound;
    [SerializeField] private AudioClip clickSound;

    [Header("Other")]
    [SerializeField] private AudioSource audioSource;

    public void PlayClickSound()
    {
        if(clickSound == null)
        {
            Debug.LogWarning("ClickSound равен null!");
            return;
        }
        if(audioSource == null)
        {
            Debug.LogWarning("AudioSource равен null!");
            return;
        }
        audioSource.PlayOneShot(clickSound);
        VibrationHandler.Vibrate(2, 2);
    }
    public void PlayWhooshSound()
    {
        if(whooshSound == null)
        {
            Debug.LogWarning("WhooshSound равен null!");
            return;
        }
        if(audioSource == null)
        {
            Debug.LogWarning("AudioSource равен null!");
            return;
        }
        audioSource.PlayOneShot(whooshSound);
    }
}