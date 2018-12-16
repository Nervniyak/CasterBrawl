using System.Collections.Generic;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    [SerializeField] private PlayerUI playerUI;


    [SerializeField] private GameObject pauseMenu;

    [SerializeField] private GameObject primaryMenu;

    [SerializeField] private GameObject settingsMenu;

    [SerializeField] private GameObject scoreboard;

    private bool _settingsOpened;


    void Start()
    {
        PauseMenu.isOn = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            scoreboard.SetActive(true);
        }
        else if (Input.GetKeyUp(KeyCode.Tab))
        {
            scoreboard.SetActive(false);
        }
    }

    public void TogglePauseMenu()
    {
        pauseMenu.SetActive(!pauseMenu.activeSelf);

        Cursor.visible = pauseMenu.activeSelf;


        if (!playerUI.PlayerIsDead)
        {
            playerUI.gameObject.SetActive(!pauseMenu.activeSelf);
        }


        PauseMenu.isOn = pauseMenu.activeSelf;
        if (_settingsOpened)
        {
            ToggleSettingsMenu();
        }
    }

    public void ToggleSettingsMenu()
    {
        primaryMenu.SetActive(!primaryMenu.activeSelf);
        settingsMenu.SetActive(!settingsMenu.activeSelf);
        _settingsOpened = settingsMenu.activeSelf;
    }

}
