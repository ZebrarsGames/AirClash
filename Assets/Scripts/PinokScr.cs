using UnityEngine;
using UnityEngine.Events;

public class PinokScr : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private UnityEvent profilePanelActive;

    private void OnEnable()
    {
        if(profilePanelActive != null)
        {
            profilePanelActive.Invoke();
        }
    }
}