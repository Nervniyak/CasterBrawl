using System;
using System.IO;
using System.Linq;
using UnityEngine;

public class SettingManager : MonoBehaviour
{
    public static SettingManager Instance;

    public static GameSettinngs GameSettinngs;


    public static Resolution[] Resolutions;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this);
        Resolutions = Screen.resolutions.Distinct().Where(r => r.width >= 800 && r.height >= 600).ToArray();
        LoadSettings();
    }

    public static void Aplysettings(GameSettinngs gameSettinngs)
    {
        try
        {
            Screen.SetResolution(Resolutions[gameSettinngs.resolutionIndex].width, Resolutions[gameSettinngs.resolutionIndex].height, Screen.fullScreen);
        }
        catch (Exception)
        {
            gameSettinngs.resolutionIndex = 0;
            Screen.SetResolution(Resolutions[gameSettinngs.resolutionIndex].width, Resolutions[gameSettinngs.resolutionIndex].height, Screen.fullScreen);
        }
        Screen.fullScreen = gameSettinngs.fullScreen;
        QualitySettings.masterTextureLimit = gameSettinngs.quality;
        QualitySettings.antiAliasing = (int)Mathf.Pow(2, gameSettinngs.antialiasing);
        QualitySettings.vSyncCount = gameSettinngs.vSync;

        GameSettinngs = gameSettinngs;
    }

    public static void SaveSettings(GameSettinngs gameSettinngs)
    {
        var jsonData = JsonUtility.ToJson(gameSettinngs, true);
        File.WriteAllText(Application.persistentDataPath + "/gamesettings.json", jsonData);
    }

    public static void LoadSettings()
    {
        GameSettinngs gameSettinngs;
        if (File.Exists(Application.persistentDataPath + "/gamesettings.json"))
        {
            gameSettinngs = JsonUtility.FromJson<GameSettinngs>(File.ReadAllText(Application.persistentDataPath + "/gamesettings.json"));
        }
        else
        {
            gameSettinngs = new GameSettinngs
            {
                mouseSensitivity = 0.5f,
                audioVolume = 1f
            };
            SaveSettings(gameSettinngs);
        }

        Aplysettings(gameSettinngs);
    }
}

