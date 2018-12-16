using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class MatchManager : NetworkBehaviour
{
    [SyncVar]
    public float RespawnTime = -1f;
    [SyncVar]
    public float ShrinkDelay = 10f;


    //[SyncVar(hook = "PlayerDied")]
    public int PlayersRemaining = -1;

    public static MatchManager Instance;

    [SerializeField] private LevelBuilder _builder;

    private Player[] _playersInMatch;
    private List<Player> _playersInMatchAlive;

    //private MatchSettings _matchSettings;
    private bool _isRestarting;
    private bool _isInLobby;

    public bool IsReadyToStart;
    private bool _hasStarted;

    private bool _matchInProgress;


    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one MatchManager");
        }
        else
        {
            Instance = this;
        }

    }

    private void Start()
    {
        _isInLobby = _builder.CurrentRestartIteration == 1;
        if (isServer)
        {
            //_matchSettings = GameManager.Instance.matchSettings;
            StartCoroutine(WaitForPlayers());
            GameManager.Instance.onPlayerLeaveCallback += Test;
        }
    }

    public void Update()
    {
        if (IsReadyToStart && Input.GetKey(KeyCode.R) && !_hasStarted)
        {
            _hasStarted = true;
        }
    }

    private void Test(string playerId)
    {
        Debug.LogError(playerId + " DISCONNECTED");
        Debug.LogError("PLAYERS ALIVE BEFORE: " + _playersInMatchAlive.Count);
        //Debug.LogError("DISCO");
        foreach (var player in _playersInMatchAlive)
        {
            Debug.LogError("Alive player player.transform.name: " + player.transform.name + " player.username " + player.username + " player.netId " + player.netId);
        }
        _playersInMatchAlive = _playersInMatchAlive.Where(p => p.transform.name != playerId).ToList();
        Debug.LogError("PLAYERS ALIVE AFTER: " + _playersInMatchAlive.Count);
        foreach (var player in _playersInMatchAlive)
        {
            Debug.LogError("Alive player player.transform.name: " + player.transform.name + " player.username " + player.username + " player.netId " + player.netId);
        }
        //foreach (var player in _playersInMatchAlive)
        //{
        //    Debug.LogError(player.transform.name);
        //}
    }

    private void StartMatch()
    {
        _playersInMatch = GameManager.GetAllPlayers();

        _playersInMatchAlive = _playersInMatch.ToList();
        PlayersRemaining = _playersInMatchAlive.Count;
        _matchInProgress = true;
        Debug.Log(PlayersRemaining);


        _builder.GenerateRandomLevel();

    }


    private IEnumerator WaitForPlayers()
    {
        int counter = 0;
        while (true)
        {
            Debug.Log("Waiting");
            _playersInMatch = GameManager.GetAllPlayers();

            if (_hasStarted)
            {
                break;
            }

            yield return new WaitForSeconds(1f);

            if (_playersInMatch.Length > 1)
            {
                IsReadyToStart = true;
                if (counter % 8 == 1)
                {
                    GameManager.Instance.onGameReadyCallback.Invoke();
                }
            }
            else
            {
                IsReadyToStart = false;
            }
            counter++;
        }

        //_hasStarted = false;

        Debug.Log("WaitForPlayers after join");

        if (_isInLobby)
        {
            Debug.Log("_isInLobby");
            yield return new WaitForSeconds(4f);
            _isInLobby = false;
            _isRestarting = true;
            StartCoroutine(MatchEnded());


            //_builder.CmdDestroyCurrentLevel();
            //yield return new WaitForSeconds(0.5f);
        }

        if (!_isRestarting)
        {
            Debug.Log("StartMatch");
            StartMatch();
            yield return new WaitForSeconds(3f);
            RpcMatchStarted();
        }

    }

    public void PlayerDied(Player diedPlayer)
    {
        if (!_matchInProgress)
        {
            return;
        }
        if (diedPlayer != null && _playersInMatchAlive != null)
        {
            _playersInMatchAlive.Remove(diedPlayer);
        }

        PlayersRemaining = _playersInMatchAlive.Count;
        if (_isRestarting)
        {
            return;
        }
        Debug.Log(PlayersRemaining);
        if (PlayersRemaining <= 1)
        {
            _isRestarting = true;
            StartCoroutine(MatchEnded());
            if (_playersInMatchAlive.Count == 1)
            {
                RpcPlayerWon(_playersInMatchAlive[0].transform.name, _playersInMatchAlive[0].username, _playersInMatchAlive[0].playerColor); // TODO: COMMENT ME TO DEMO
            }
            else
            {
                RpcPlayerDied(100);
            }
        }
        else
        {
            RpcPlayerDied(PlayersRemaining);
        }
    }


    [ClientRpc]
    public void RpcMatchStarted()
    {
        GameManager.Instance.onMatchStartCallback.Invoke();
    }

    [ClientRpc]
    public void RpcPlayerDied(int playersRemaining)
    {
        PlayersRemaining = playersRemaining;
        GameManager.Instance.onPlayerDeathCallback.Invoke(playersRemaining);
    }

    [ClientRpc]
    public void RpcPlayerWon(string playerID, string username, string playerColor)
    {
        GameManager.GetPlayer(playerID).wins++;
        GameManager.Instance.onPlayerWonCallback.Invoke(username, playerColor);
    }

    private IEnumerator MatchEnded()
    {
        yield return new WaitForSeconds(2.5f);

        foreach (var player in _playersInMatch)
        {
            player.RpcTakeDamage(9999, null, "Match End");
        }
        _builder.CmdDestroyCurrentLevel();
        _matchInProgress = false;
        yield return new WaitForSeconds(0.75f);
        RespawnTime = 3.5f; // TODO: COMMENT ME TO DEMO
        yield return new WaitForSeconds(0.75f);
        foreach (var player in _playersInMatch)
        {
            Debug.Log(player.username + " isDead = " + player.isDead);
            if (player.isDead)
            {
                player.RpcRespawn();
                //StartCoroutine(player.Respawn());
            }
        }
        yield return new WaitForSeconds(1f);
        RespawnTime = -1;
        //_builder.CmdDestroyCurrentLevel();
        _isRestarting = false;
        StartCoroutine(WaitForPlayers());

        //yield return new WaitForSeconds(50f);

    }
}
