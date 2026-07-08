using UnityEngine;
using UnityEngine.UI;

public class ProfileHandler : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] RawImage avatarImage;
    [SerializeField] Text nickText;
    [SerializeField] Text moneyText;
    [SerializeField] Text goalText;
    [SerializeField] Image currentSkinImage;

    public void SetProfileData(string nick, RawImage avatar)
    {
        avatarImage.texture = avatar.texture;
        nickText.text = nick;
    }
}
