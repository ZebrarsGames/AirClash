using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[System.Serializable]
public class ApplyProfileEvent : UnityEvent<string, RawImage> { }
public class ProfileUIHandler : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private InputField nickInputField;
    [SerializeField] private RawImage displayImage; 
    [Header("Other")]
    [SerializeField] private ApplyProfileEvent applyProfileEvent;
    public void OpenGallery()
    {
        if (NativeGallery.IsMediaPickerBusy()) return;

        NativeGallery.GetImageFromGallery((path) =>
        {
            Debug.Log("Путь к файлу: " + path);
            
            if (path != null)
            {
                Texture2D texture = NativeGallery.LoadImageAtPath(path, 1024);
                
                if (texture != null)
                {
                    displayImage.texture = texture;
                }
            }
        }, "Выберите изображение", "image/*");
    }

    public void Apply()
    {
        string nick = nickInputField.text;
        applyProfileEvent.Invoke(nick, displayImage);
    }
}
