using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.IO;

[System.Serializable]
public class ApplyProfileEvent : UnityEvent<string, RawImage> { }
public class ProfileUIHandler : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private InputField nickInputField;
    [SerializeField] private RawImage displayImage; 
    [Header("Other")]
    [SerializeField] private ApplyProfileEvent applyProfileEvent;
    [SerializeField] private SaveManager saveManager;
    private string avatarPath;

    void Start()
    {
        avatarPath = Path.Combine(Application.persistentDataPath, "avatar.png");
        LoadSavedAvatar();
        nickInputField.text = saveManager.GetData().NickName;
    }

    public void OpenGallery()
    {
        if (NativeGallery.IsMediaPickerBusy()) return;

        NativeGallery.GetImageFromGallery((path) =>
        {
            Debug.Log("Путь к файлу: " + path);
            
            if (path != null)
            {
                Texture2D texture = NativeGallery.LoadImageAtPath(path, 1024, false);
                
                if (texture != null)
                {
                    displayImage.texture = texture;
                    SaveAvatarToFile(texture);
                }
            }
        }, "Выберите изображение", "image/*");
    }

    public void Apply()
    {
        string nick = nickInputField.text;
        applyProfileEvent.Invoke(nick, displayImage);
    }

    private void SaveAvatarToFile(Texture2D texture)
    {
        try
        {
            if (File.Exists(avatarPath))
            {
                File.Delete(avatarPath);
                Debug.Log("Старый аватар успешно удален.");
            }

            byte[] bytes = texture.EncodeToPNG();

            File.WriteAllBytes(avatarPath, bytes);
            Debug.Log("Новый аватар сохранен по пути: " + avatarPath);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Ошибка при сохранении аватара: " + e.Message);
        }
    }

    private void LoadSavedAvatar()
    {
        if (File.Exists(avatarPath))
        {
            byte[] bytes = File.ReadAllBytes(avatarPath);
            
            Texture2D savedTexture = new Texture2D(2, 2);
            savedTexture.LoadImage(bytes);

            displayImage.texture = savedTexture;
            Debug.Log("Сохраненный аватар успешно загружен при старте.");
        }
    }
}
