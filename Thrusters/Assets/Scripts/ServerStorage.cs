using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ServerStorage : NetworkBehaviour
{
    //[SyncVar]
    public List<int> OccupiedColorIndexes = new List<int>();

    public static readonly string[] Colors =
    {
        "#ff0019", "#0055ff", "#d0ff00", "#7700ff", "#00ffae", "#ff6600", "#ff0099", "#00ffae", "#d000ff", "#ffd900", "#00ff00", "#00e1ff"
    };


    public static ServerStorage instance;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("More than one ServerStorage");
        }
        else
        {
            instance = this;
        }

    }
}
