using UnityEngine;
using UnityEngine.Events;

public sealed class PinokScr : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private UnityEvent OnPanelActive;

    private System.Action OnPanelActiveRuntime;

    private void Awake()
    {
        if(OnPanelActive != null)
        {
            if(OnPanelActive.GetPersistentEventCount() > 0)
            {
                OnPanelActiveRuntime = OnPanelActive.Invoke;
            }
            
            OnPanelActive = null;
        }
    }

    private void OnEnable()
    {
        OnPanelActiveRuntime?.Invoke();
    }
}