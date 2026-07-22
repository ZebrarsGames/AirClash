using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CloudUIScr : MonoBehaviour
{
    [Header("Input Fields")]
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;

    [Header("Texts")]
    [SerializeField] private Text statusText;

    [Header("Buttons")]
    [SerializeField] private Button[] buttons;

    [Header("Scripts")]
    [SerializeField] private FirebaseManager firebaseManager;

    public void OnClickLoginOrRegister()
    {
        firebaseManager.AccountAuth(usernameInput.text, passwordInput.text);
    }

    public void OnClickSave()
    {
        firebaseManager.SaveProgress(usernameInput.text, passwordInput.text);
    }

    public void OnClickLoad()
    {
        firebaseManager.LoadProgress(usernameInput.text, passwordInput.text);
    }

    public void SetStatusText(string status)
    {
        statusText.text = "Статус: " + status;
    }

    public void SetActiveBtns(bool isActive)
    {
        for(int i = 0; i < buttons.Length; i++)
        {
            buttons[i].interactable = !isActive;
        }
    }
}
