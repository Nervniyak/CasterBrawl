using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class killfeedItem : MonoBehaviour
{

    [SerializeField] private Text text;

    public void Setup(string player, string playerColor, string source, string sourceColor)
    {
        text.text = "<b><color=" + sourceColor + ">" + source + "</color></b> killed <color=" + playerColor + ">" + player + "</color>";
    }
}

