using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Player))]
[RequireComponent(typeof(PlayerController))]
public class PlayerSetup : NetworkBehaviour
{

    [SerializeField] private Behaviour[] componentsToDisable;

    [SerializeField] private string remoteLayerName = "RemotePlayer";

    [SerializeField] private GameObject playerGraphics;

    [SerializeField] private string dontDrawLayerNane = "DontDraw";

    //[SerializeField] private GameObject playerUIPrefab;
    //[SerializeField] private GameObject gameUIPrefab;
    [SerializeField] private GameObject guiPrefab;

    [HideInInspector]
    public GameObject PlayerUiInstance;
    [HideInInspector]
    public GameObject GameUiInstance;
    [HideInInspector]
    public GameObject guiInstance;


    void Start()
    {
        if (!isLocalPlayer)
        {
            DisableComponents();
            AssignRemoteLayer();         
        }
        else
        {
            SetLayerRecursively(playerGraphics, LayerMask.NameToLayer(dontDrawLayerNane));

            guiInstance = Instantiate(guiPrefab);
            guiInstance.name = guiPrefab.name;

            var playerUi = guiInstance.GetComponentInChildren<PlayerUI>();
            PlayerUiInstance = playerUi.gameObject;

            var gameUi = guiInstance.GetComponentInChildren<GameUI>();
            GameUiInstance = gameUi.gameObject;

            playerUi.SetPlayer(GetComponent<Player>());

            var thisPlayer = GetComponent<Player>();
            thisPlayer.SetupPlayer();

            string username;
            if (UserAccountManager.IsLoggedIn)
            {
                username = UserAccountManager.PlayerUsername;
            }
            else
            {
                username = transform.name;
            }
            CmdSetUsername(transform.name, username);

            var players = GameManager.GetAllPlayers();

            foreach (var player in players)
            {
                if (player == thisPlayer)
                {
                    continue;
                }
                RecolorPlayer(player, player.playerColor);
            }

            CmdSetColor(transform.name);
        }
    }

    [Command]
    void CmdSetUsername(string playerID, string username)
    {
        var player = GameManager.GetPlayer(playerID);
        if (player != null)
        {
            Debug.Log(username + " has joined.");
            player.username = username;
        }
    }

    [Command]
    void CmdSetColor(string playerID)
    {
        var occupiedColorIndexes = ServerStorage.instance.OccupiedColorIndexes;     
        var player = GameManager.GetPlayer(playerID);
        if (player != null)
        {
            var playerColor = "#333";
            var occupiedIndex = -1;
            for (var i = 0; i < ServerStorage.Colors.Length; i++)
            {
                if (!occupiedColorIndexes.Contains(i))
                {
                    occupiedIndex = i;

                    playerColor = ServerStorage.Colors[i];
                    break;
                }
            }
            occupiedColorIndexes.Add(occupiedIndex);      

            player.playerColor = playerColor;
            Debug.Log(player.username + " has " + player.playerColor + ".");
            RpcRecolorPlayerOnAllClients(playerID, playerColor);
        }
    }

    [ClientRpc]
    private void RpcRecolorPlayerOnAllClients(string playerID, string color)
    {
        var player = GameManager.GetPlayer(playerID);
        RecolorPlayer(player, color);
    }

    void RecolorPlayer(Player player, string stringColor)
    {
        Color color;
        ColorUtility.TryParseHtmlString(stringColor, out color);
        var armor = Player.FindDeepChild(player.transform, "Armor");
        armor.gameObject.GetComponent<Renderer>().material.color = color;
    }

    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        string _netID = GetComponent<NetworkIdentity>().netId.ToString();
        Player _player = GetComponent<Player>();

        GameManager.RegisterPlayer(_netID, _player);
    }

    //public override void O


    void DisableComponents()
    {
        for (int i = 0; i < componentsToDisable.Length; i++)
        {
            componentsToDisable[i].enabled = false;
        }

    }

    void AssignRemoteLayer()
    {
        gameObject.layer = LayerMask.NameToLayer(remoteLayerName);
    }

    void OnDisable()
    {
        Debug.LogError("OnDisable");
        Destroy(guiInstance);

        if (isLocalPlayer)
        {
            GameManager.Instance.SetSceneCameraActive(true);
        }
        GameManager.UnRegisterPlayer(transform.name);


    }
}
