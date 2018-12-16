using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance;

    //public MatchSettings matchSettings;

    [SerializeField] private GameObject SceneCamera;

    public delegate void OnPlayerKilledCallback(string player, string playerColor, string source, string sourceColor);

    public OnPlayerKilledCallback onPlayerKilledCallback;

    public delegate void OnMatchStartCallback();

    public OnMatchStartCallback onMatchStartCallback;

    public delegate void OnPlayerDeathCallback(int remaining);

    public OnPlayerDeathCallback onPlayerDeathCallback;

    public delegate void OnPlayerWonCallback(string username, string playerColor);

    public OnPlayerWonCallback onPlayerWonCallback;

    public delegate void OnGameReadyCallback();

    public OnGameReadyCallback onGameReadyCallback;

    public delegate void OnPlayerLeaveCallback(string playerId);

    public OnPlayerLeaveCallback onPlayerLeaveCallback;

    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one GameManager");
        }
        else
        {
            Instance = this;
        }

    }

    public void SetSceneCameraActive(bool isActive)
    {
        if (SceneCamera == null)
        {
            return;
        }
        SceneCamera.SetActive(isActive);
    }

    #region Player tracking
    private const string PLAYER_ID_PREFIX = "Player ";

    private static Dictionary<string, Player> players = new Dictionary<string, Player>();

    public static void RegisterPlayer(string _netID, Player _player)
    {
        string _playerID = PLAYER_ID_PREFIX + _netID;
        if (!players.ContainsKey(_playerID))
        {
            players.Add(_playerID, _player);
            _player.transform.name = _playerID;
        }


    }

    public static void UnRegisterPlayer(string _playerID)
    {
        players.Remove(_playerID);
        Instance.onPlayerLeaveCallback.Invoke(_playerID);
        Debug.LogError("UnRegisterPlayer" + _playerID);
    }

    public static Player GetPlayer(string _playerID)
    {
        if (string.IsNullOrEmpty(_playerID) || !players.ContainsKey(_playerID))
        {
            return null;
        }
        return players[_playerID];
        //return null;
    }

    public static Player[] GetAllPlayers()
    {
        return players.Values.ToArray();
        //return null;
    }


    #endregion
}
