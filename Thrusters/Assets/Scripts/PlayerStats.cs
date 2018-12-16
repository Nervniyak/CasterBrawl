using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    public Text KD;


    void Start()
    {
        if (UserAccountManager.IsLoggedIn)
        {
            UserAccountManager.instance.GetData(OnRecievedData);
        }
    }

    void OnRecievedData(string data)
    {
        if (KD == null)
        {
            return;
        }

        KD.text = DataParser.DataToKills(data) + "/" + DataParser.DataToDeaths(data);

    }
}
