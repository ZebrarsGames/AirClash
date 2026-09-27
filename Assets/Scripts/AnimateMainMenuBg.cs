using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;

[RequireComponent(typeof(RawImage))]
[DisallowMultipleComponent]
public class AnimateMainMenuBg : MonoBehaviour
{
    private const string AnimBgKey = "isAnimBg";

    [Header("Settings")]
    [SerializeField] private Vector2 speed = new Vector2(-0.06f, -0.06f);

    private RawImage rawImage;
    private TweenerCore<Vector2, Vector2, VectorOptions> tween;

    private Rect initialUvRect;
    private bool isAnim;

    private Vector2 currentOffset;

    private void Awake()
    {
        rawImage = GetComponent<RawImage>();
        initialUvRect = rawImage.uvRect;
        isAnim = PlayerPrefs.GetInt(AnimBgKey, 1) == 1;
    }

    private void Start()
    {
        if(isAnim)
        {
            StartAnimation();
        }
    }

    private void StartAnimation()
    {
        if(tween != null && tween.IsActive()) return;
        if(speed == Vector2.zero) return;

        currentOffset = Vector2.zero;

        tween = DOTween.To(
            GetPositionOffset,
            SetPositionOffset,
            speed,
            1f
        )
        .SetLoops(-1, LoopType.Incremental)
        .SetEase(Ease.Linear)
        .SetLink(gameObject);
    }

    private Vector2 GetPositionOffset() => currentOffset;
    
    private void SetPositionOffset(Vector2 value)
    {
        currentOffset = value;

        Rect currentRect = initialUvRect;
        currentRect.x += currentOffset.x;
        currentRect.y += currentOffset.y;

        currentRect.x %= 1f;
        currentRect.y %= 1f;

        rawImage.uvRect = currentRect;
    }

    private void StopAnimation()
    {
        if(tween != null && tween.IsActive())
        {
            tween.Kill();
            tween = null;
        }
        
        if(rawImage != null)
        {
            rawImage.uvRect = initialUvRect;
        }
    }

    public void SetIsAnim(bool value)
    {
        if(isAnim == value) return;

        isAnim = value;
        PlayerPrefs.SetInt(AnimBgKey, value ? 1 : 0);

        if(isAnim) StartAnimation();
        else StopAnimation();
    }
}
