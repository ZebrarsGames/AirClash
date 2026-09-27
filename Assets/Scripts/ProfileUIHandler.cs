using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.IO;
using DG.Tweening;
using TMPro;

[System.Serializable]
public class ApplyProfileEvent : UnityEvent<RawImage> { }

public class ProfileUIHandler : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private RawImage displayImage; 
    [SerializeField] private GameObject editProfilePanel;
    [Header("Other")]
    [SerializeField] private ApplyProfileEvent applyProfileEvent;
    
    private string avatarPath;
    private RectTransform panelRect;
    private Texture2D dynamicTexture;

    void Awake()
    {
        panelRect = editProfilePanel.GetComponent<RectTransform>();
        avatarPath = Path.Combine(Application.persistentDataPath, "avatar.png");
    }

    void Start()
    {
        LoadSavedAvatar();
        panelRect.DOKill();
        panelRect.localPosition = Vector2.zero;
    }

    void OnDestroy()
    {
        panelRect.DOKill();
    }

    public void OpenGallery()
    {
        if(NativeGallery.IsMediaPickerBusy()) return;

        NativeGallery.GetImageFromGallery((path) =>
        {
            Debug.Log("Путь к файлу: " + path);
            
            if(path != null)
            {
                Texture2D texture = NativeGallery.LoadImageAtPath(path, 1024, false);
                if(texture != null)
                {
                    ClearDisplayTexture();
                    dynamicTexture = texture;
                    displayImage.texture = texture;
                    SaveAvatarToFile(texture);
                }
            }
        }, "Выберите изображение", "image/*");
    }

    public void MoveUpPanel()
    {
        panelRect.DOLocalMoveY(100.0f, 0.3f).SetEase(Ease.OutSine);
    }

    public void MoveDownPanel()
    {
        panelRect.DOLocalMoveY(0, 0.3f).SetEase(Ease.OutSine);
    }

    public void Apply()
    {
        applyProfileEvent.Invoke(displayImage);
    }

    private void SaveAvatarToFile(Texture2D texture)
    {
        try
        {
            byte[] bytes = texture.EncodeToPNG();
            File.WriteAllBytes(avatarPath, bytes);
            Debug.Log("Новый аватар сохранен по пути: " + avatarPath);
        }
        catch(System.Exception e)
        {
            Debug.LogError("Ошибка при сохранении аватара: " + e.Message);
        }
    }

    private void LoadSavedAvatar()
    {
        if(File.Exists(avatarPath))
        {
            try
            {
                byte[] bytes = File.ReadAllBytes(avatarPath);
                
                Texture2D savedTexture = new Texture2D(2, 2);
                savedTexture.LoadImage(bytes);

                ClearDisplayTexture();
                dynamicTexture = savedTexture;
                displayImage.texture = savedTexture;
                Debug.Log("Сохраненный аватар успешно загружен при старте.");
            }
            catch(System.Exception e)
            {
                Debug.LogError("Ошибка при чтении файла аватары: " + e.Message);
            }
        }
    }

    private void ClearDisplayTexture()
    {
        if(dynamicTexture != null)
        {
            Destroy(dynamicTexture);
            dynamicTexture = null;
        }
        displayImage.texture = null;
    }
}