using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using System.Collections;

public class JoinGame : MonoBehaviour
{
    private List<GameObject> roomList = new List<GameObject>();

    [SerializeField] private Text status;

    [SerializeField] private GameObject roomListItemPrefab;

    [SerializeField] private Transform RoomListParent;

    private NetworkManager networkManager;

    private bool isJoining = false;


    void Start()
    {
        networkManager = NetworkManager.singleton;
        if (networkManager.matchMaker == null)
        {
            networkManager.StartMatchMaker();
        }

        StartCoroutine(AutoRefresh());

    }

    IEnumerator AutoRefresh()
    {
        while (true)
        {
            if (!isJoining)
            {
                RefreshRoomList();
            }

            yield return new WaitForSeconds(7);
        }
    }

    public void RefreshRoomList()
    {
        ClearRoomList();

        if (networkManager.matchMaker == null)
        {
            networkManager.StartMatchMaker();
        }
        networkManager.matchMaker.ListMatches(0, 20, "", false, 0, 0, OnMatchList);
        status.text = "Loading...";


    }

    public void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matches)
    {
        status.text = "";

        if (!success || matches == null)
        {
            status.text = "Couldn't get room list";
            return;
        }



        foreach (var match in matches)
        {
            GameObject _roomListItemGO = Instantiate(roomListItemPrefab);
            _roomListItemGO.transform.SetParent(RoomListParent);

            RoomListItem _roomListItem = _roomListItemGO.GetComponent<RoomListItem>();
            if (_roomListItem != null)
            {
                _roomListItem.Setup(match, JoinRoom);
            }

            roomList.Add(_roomListItemGO);
        }

        if (roomList.Count == 0)
        {
            status.text = "No rooms found.";
        }
    }

    void ClearRoomList()
    {
        for (int i = 0; i < roomList.Count; i++)
        {
            Destroy(roomList[i]);
        }

        roomList.Clear();
    }

    public void JoinRoom(MatchInfoSnapshot _match)
    {
        Debug.Log("Joining " + _match.name);
        networkManager.matchMaker.JoinMatch(_match.networkId, "", "", "", 0, 0, networkManager.OnMatchJoined);
        StartCoroutine(WaitForJoin());

    }

    private IEnumerator WaitForJoin()
    {
        isJoining = true;
        ClearRoomList();


        var countdown = 16;
        while (countdown > 0)
        {
            status.text = "Joining game... (" + countdown + ")";
            yield return new WaitForSeconds(1);
            countdown--;
        }

        isJoining = false;
        status.text = "Failed to connect.";
        yield return new WaitForSeconds(1);

        var matchInfo = networkManager.matchInfo;
        if (matchInfo != null)
        {
            networkManager.matchMaker.DropConnection(matchInfo.networkId, matchInfo.nodeId, 0, networkManager.OnDropConnection);
            networkManager.StopHost();
        }


        RefreshRoomList();
    }

}
