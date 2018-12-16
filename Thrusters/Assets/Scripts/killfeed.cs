using System.Collections.Generic;
using UnityEngine;

public class killfeed : MonoBehaviour
{

    [SerializeField] private GameObject killfeedItemPrefab;
    void Start()
    {
        GameManager.Instance.onPlayerKilledCallback += OnKill;
    }


    public void OnKill(string player, string playerColor, string source, string sourceColor)
    {
        GameObject go = Instantiate(killfeedItemPrefab, transform);
        go.GetComponent<killfeedItem>().Setup(player, playerColor, source, sourceColor);
        go.transform.SetAsFirstSibling();
        Destroy(go, 4f);
    }

}
