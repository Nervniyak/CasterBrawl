using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DatabaseControl;
using UnityEngine.SceneManagement;

public class UserAccountManager : MonoBehaviour
{

    public static UserAccountManager instance;

    //These store the username and password of the player when they have logged in
    public static string PlayerUsername { get; protected set; }
    private static string PlayerPassword = "";

    public static bool IsLoggedIn { get; protected set; }

    //public static string LoggedInData { get; protected set; }

    public string loggedInSceneName = "Lobby";
    public string loggedOutSceneName = "LoginMenu";

    public delegate void OnDataRecievedCallback(string data);

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this);
    }

    public void LogOut()
    {
        PlayerUsername = "";
        PlayerPassword = "";

        IsLoggedIn = false;

        Debug.Log("User logged out");
        SceneManager.LoadScene(loggedOutSceneName);
    }

    public void LogIn(string username, string password)
    {
        PlayerUsername = username;
        PlayerPassword = password;

        IsLoggedIn = true;

        Debug.Log("User logged in");
        SceneManager.LoadScene(loggedInSceneName);
    }


    public void SendData(string data)
    { //called when the 'Send Data' button on the data part is pressed
        if (IsLoggedIn)
        {
            //ready to send request
            StartCoroutine(sendSendDataRequest(PlayerUsername, PlayerPassword, data)); //calls function to send: send data request
        }
    }

    IEnumerator sendSendDataRequest(string username, string password, string data)
    {

        IEnumerator eee = DCF.SetUserData(username, password, data);
        while (eee.MoveNext())
        {
            yield return eee.Current;
        }
        //WWW returneddd = eee.Current as WWW;
        string response = eee.Current as string; // << The returned string from the request
        if (response == "ContainsUnsupportedSymbol")
        {
            //One of the parameters contained a - symbol
            Debug.Log("Data Upload Error. Could be a server error. To check try again, if problem still occurs, contact us.");
        }
        if (response == "Error")
        {
            //Error occurred. For more information of the error, DC.Login could
            //be used with the same username and password
            Debug.Log("Data Upload Error: Contains Unsupported Symbol '-'");
        }
    }

    public void GetData(OnDataRecievedCallback onDataRecieved)
    { //called when the 'Get Data' button on the data part is pressed

        if (IsLoggedIn)
        {
            //ready to send request    
            StartCoroutine(sendGetDataRequest(PlayerUsername, PlayerPassword, onDataRecieved)); //calls function to send get data request
        }
    }

    IEnumerator sendGetDataRequest(string username, string password, OnDataRecievedCallback onDataRecieved)
    {
        string data = "Error";

        IEnumerator eeee = DCF.GetUserData(username, password);
        while (eeee.MoveNext())
        {
            yield return eeee.Current;
        }
        //WWW returnedddd = eeee.Current as WWW;
        string response = eeee.Current as string; // << The returned string from the request
        if (response == "Error")
        {
            //Error occurred. For more information of the error, DC.Login could
            //be used with the same username and password
            Debug.Log("Data Upload Error. Could be a server error. To check try again, if problem still occurs, contact us.");
        }
        else
        {
            if (response == "ContainsUnsupportedSymbol")
            {
                //One of the parameters contained a - symbol
                Debug.Log("Get Data Error: Contains Unsupported Symbol '-'");
            }
            else
            {
                //Data received in returned.text variable
                string DataRecieved = response;
                data = DataRecieved;
            }
        }

        if (onDataRecieved != null)
        {
            onDataRecieved.Invoke(data);

        }
    }
}
