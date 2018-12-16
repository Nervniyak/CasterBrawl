using UnityEngine;

public class Scoreboard : MonoBehaviour
{
    [SerializeField] private GameObject _playerScoreboardItem;
    [SerializeField] private Transform _playerScoreboardList;

    private void OnEnable()
    {
        var players = GameManager.GetAllPlayers();

        foreach (var player in players)
        {
            var itemGo = Instantiate(_playerScoreboardItem, _playerScoreboardList);
            var item = itemGo.GetComponent<ScoreboardItem>();
            if (item != null)
            {
                item.Setup(player.username, player.playerColor, player.wins, player.kills, player.deaths);
            }
        }
    }

    private void OnDisable()
    {
        foreach (Transform child in _playerScoreboardList)
        {
            Destroy(child.gameObject);
        }
    }
}
