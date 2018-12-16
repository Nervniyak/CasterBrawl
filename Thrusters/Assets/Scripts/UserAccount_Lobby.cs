using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserAccount_Lobby : MonoBehaviour
{
    public Text usernameText;

    void Start()
    {
        if (UserAccountManager.IsLoggedIn)
        {
            usernameText.text = "Logged In As: " + UserAccountManager.PlayerUsername;
        }


    }


    public void LogOut()
    {
        if (UserAccountManager.IsLoggedIn)
        {
            UserAccountManager.instance.LogOut();
        }
    }

    public void Quit()
    {
        Application.Quit();
    }
}
