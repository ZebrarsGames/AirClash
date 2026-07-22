using UnityEngine;
using TMPro;

public class CloudUIScr : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public FirebaseManager firebaseManager;

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
}
