using UnityEngine;
using UnityEngine.UI;

public class XpUiScr : MonoBehaviour
{
    [SerializeField] private Slider xpSlider;
    [SerializeField] private XpHandler xpHandler;
    void Start()
    {
        SetProgress(xpHandler.GetXPProgress());
    }

    public void SetProgress(float progress)
    {
        xpSlider.value = progress;
    }
}

