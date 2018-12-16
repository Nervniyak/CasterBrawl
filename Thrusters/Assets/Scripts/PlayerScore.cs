using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerScore : MonoBehaviour
{
    private int lastKills = 0;
    private int lastDeaths = 0;

    private Player player;

    void Start()
    {
        player = GetComponent<Player>();
        StartCoroutine(SyncScoreLoop());
    }

    void OnDestroy()
    {
        if (player != null)
        {
            SyncNow();
        }
    }

    IEnumerator SyncScoreLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(5);
            if (player.kills == 0 && player.deaths == 0)
            {
                yield return null;
            }

            SyncNow();
        }
    }

    void SyncNow()
    {
        if (UserAccountManager.IsLoggedIn)
        {
            UserAccountManager.instance.GetData(OnDataReceived);
        }
    }

    void OnDataReceived(string data)
    {
        if (player.kills <= lastKills && player.deaths <= lastDeaths)
        {
            return;
        }

        int killsSinceLast = player.kills - lastKills;
        int deathsSinceLast = player.deaths - lastDeaths;


        int syncedKills = DataParser.DataToKills(data);
        int syncedDeaths = DataParser.DataToDeaths(data);

        int newKills = killsSinceLast + syncedKills;
        int newDeaths = deathsSinceLast + syncedDeaths;

        string newData = DataParser.ValuesToData(newKills, newDeaths);

        Debug.Log("Syncing: " + newData);
        UserAccountManager.instance.SendData(newData);

        lastKills = player.kills;
        lastDeaths = player.deaths;

    }

}
