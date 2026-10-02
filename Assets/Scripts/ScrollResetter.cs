using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScrollResetter : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(ResetScroll());
    }

    private IEnumerator ResetScroll()
    {
        yield return new WaitForEndOfFrame();
        
        ScrollRect scrollRect = GetComponent<ScrollRect>();
        if(scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }
}