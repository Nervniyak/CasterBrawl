using UnityEngine;
using UnityEngine.UI;

public class ScoreboardItem : MonoBehaviour
{
    [SerializeField] private Text usernameText;
    [SerializeField] private Text winsText;
    [SerializeField] private Text killsText;
    [SerializeField] private Text deathsText;

    public void Setup(string username, string color, int wins, int kills, int deaths)
    {
        usernameText.text = "<color=" + color + ">" + username + "</color>";
        winsText.text =" Wins: " + wins;
        killsText.text = "Kill: " + kills;
        deathsText.text = "Deaths: " + deaths;
    }
}
