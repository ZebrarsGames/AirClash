using UnityEngine;
using UnityEngine.Events;

public class PinokScr : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private UnityEvent profilePanelActive;

    void OnEnable()
    {
        profilePanelActive.Invoke();
    }
}
