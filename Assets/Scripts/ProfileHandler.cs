using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class ProfileHandler : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] RawImage avatarImage;
    [SerializeField] Text nickText;
    [SerializeField] Text moneyText;
    [SerializeField] Text goalText;
    [SerializeField] Image currentSkinImage;

    [Header("Scripts")]
    [SerializeField] SaveManager saveManager;
    [SerializeField] MoneyHandler moneyHandler;
    private string avatarPath;

    void Start()
    {
        avatarPath = Path.Combine(Application.persistentDataPath, "avatar.png");
        moneyText.text = "Деньги: " + moneyHandler.GetMoney();
        nickText.text = saveManager.GetData().NickName;
        if (File.Exists(avatarPath))
        {
            byte[] bytes = File.ReadAllBytes(avatarPath);
            
            Texture2D savedTexture = new Texture2D(2, 2);
            savedTexture.LoadImage(bytes);

            avatarImage.texture = savedTexture;
            Debug.Log("Сохраненный аватар успешно загружен при старте.");
        }
    }

    public void SetProfileData(string nick, RawImage avatar)
    {
        avatarImage.texture = avatar.texture;
        nickText.text = nick;
        PlayerPrefs.SetString("Nick", nick);
        PlayerPrefs.Save();
        moneyText.text = "Деньги: " + moneyHandler.GetMoney();
        saveManager.SaveData();
    }
}
