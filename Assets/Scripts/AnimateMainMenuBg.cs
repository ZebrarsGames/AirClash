using UnityEngine;
using UnityEngine.UI;

public class AnimateMainMenuBg : MonoBehaviour
{
    [Header("Floats")]
    public Vector2 speed = new Vector2(0.1f, 0.1f);
    
    private RawImage rawImage;
    private bool isAnim = true;

    void Awake()
    {
        if(PlayerPrefs.GetInt("isAnimBg", 1) == 1) SetIsAnim(true);
        else SetIsAnim(false);
        rawImage = GetComponent<RawImage>();
    }

    void Update()
    {
        if(isAnim)
        {
           Rect currentUV = rawImage.uvRect;

            currentUV.x += speed.x * Time.deltaTime;
            currentUV.y += speed.y * Time.deltaTime;

            rawImage.uvRect = currentUV; 
        }
    }

    public void SetIsAnim(bool value)
    {
        isAnim = value;
    }
}
