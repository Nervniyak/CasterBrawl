using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class NotificationsGui : MonoBehaviour
{
    [SerializeField] private GameObject remainingTextPrefab;

    void Start()
    {
        GameManager.Instance.onMatchStartCallback += OnMatchStart;
        GameManager.Instance.onPlayerDeathCallback += OnPlayerDeath;
        GameManager.Instance.onPlayerWonCallback += OnPlayerWon;
        GameManager.Instance.onGameReadyCallback += OnGameReady;
    }

    public void OnMatchStart()
    {
        var go = Instantiate(remainingTextPrefab, transform);
        var text = go.GetComponent<Text>();
        text.text = "FIGHT!";
        var textRenderer = go.GetComponent<CanvasRenderer>();
        StartCoroutine(MoveAndDestroy(go, textRenderer));
    }

    public void OnPlayerDeath(int remaining)
    {
        var go = Instantiate(remainingTextPrefab, transform);
        var text = go.GetComponent<Text>();
        text.text = remaining + " REMAINING!";
        var textRenderer = go.GetComponent<CanvasRenderer>();
        StartCoroutine(MoveAndDestroy(go, textRenderer));
    }

    public void OnPlayerWon(string username, string playerColor)
    {
        var go = Instantiate(remainingTextPrefab, transform);
        var text = go.GetComponent<Text>();
        text.text = "<color=" + playerColor + ">"  + username + "</color> WON!";
        var textRenderer = go.GetComponent<CanvasRenderer>();
        StartCoroutine(MoveAndDestroy(go, textRenderer));
    }

    public void OnGameReady()
    {
        var go = Instantiate(remainingTextPrefab, transform);
        var text = go.GetComponent<Text>();
        text.text = "Press R to start.";
        var textRenderer = go.GetComponent<CanvasRenderer>();
        StartCoroutine(MoveAndDestroy(go, textRenderer));
    }

    public IEnumerator MoveAndDestroy(GameObject go, CanvasRenderer renderer)
    {
        var destinationY = go.transform.position.y + 100;
        yield return new WaitForSeconds(1f);
        for (int i = 0; i < 25; i++)
        {
            yield return new WaitForSeconds(0.01f);
            go.transform.position = Vector2.Lerp(go.transform.position, new Vector2(go.transform.position.x, destinationY), 0.15f);
            renderer.SetAlpha(renderer.GetAlpha() - 1f / 10);
            //text.color = new Color(text.color.r, text.color.g, text.color.b, text.color.a - 1f / 10);
        }
        yield return new WaitForSeconds(1.5f);
        Destroy(go);
    }
}
