using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SettingChanger : MonoBehaviour
{
    public Toggle FullscreenToggle;
    public Dropdown ResolutionDropdown;
    public Dropdown QualityDropdown;
    public Dropdown AntialiasingDropdown;
    public Dropdown VSyncDropdown;
    public Slider AudioVolumeSlider;
    public Slider MouseSensitivitySlider;
    public Button ApplyButton;

    private Resolution[] _resolutions;
    private GameSettinngs _gameSettinngs;

    void OnEnable()
    {
        _gameSettinngs = SettingManager.GameSettinngs ?? new GameSettinngs();

        if (FullscreenToggle && ResolutionDropdown && QualityDropdown && AntialiasingDropdown && VSyncDropdown && AudioVolumeSlider && MouseSensitivitySlider && ApplyButton)
        {
            FullscreenToggle.onValueChanged.AddListener(delegate { OnFullscreenToggle(); });
            ResolutionDropdown.onValueChanged.AddListener(delegate { OnResolutionChange(); });
            QualityDropdown.onValueChanged.AddListener(delegate { OnQualityChange(); });
            AntialiasingDropdown.onValueChanged.AddListener(delegate { OnAntialiasingChange(); });
            VSyncDropdown.onValueChanged.AddListener(delegate { OnVsyncChange(); });
            AudioVolumeSlider.onValueChanged.AddListener(delegate { OnAudioVolumeChange(); });
            MouseSensitivitySlider.onValueChanged.AddListener(delegate { OnSensitivitChange(); });
            ApplyButton.onClick.AddListener(OnApplyButtonClick);
        }

        _resolutions = SettingManager.Resolutions;
        foreach (var resolution in _resolutions)
        {
            ResolutionDropdown.options.Add(new Dropdown.OptionData(resolution.ToString()));
        }

        LoadInputs();
    }

    public void OnFullscreenToggle()
    {
        _gameSettinngs.fullScreen = FullscreenToggle.isOn;
    }

    public void OnResolutionChange()
    {
        _gameSettinngs.resolutionIndex = ResolutionDropdown.value;
    }

    public void OnQualityChange()
    {
        _gameSettinngs.quality = QualityDropdown.value;
    }

    public void OnAntialiasingChange()
    {
        _gameSettinngs.antialiasing = AntialiasingDropdown.value;
    }

    public void OnVsyncChange()
    {
        _gameSettinngs.vSync = VSyncDropdown.value;
    }

    public void OnAudioVolumeChange()
    {
        _gameSettinngs.audioVolume = AudioVolumeSlider.value;
    }

    public void OnSensitivitChange()
    {
        _gameSettinngs.mouseSensitivity = MouseSensitivitySlider.value;
    }

    public void OnApplyButtonClick()
    {
        SettingManager.Aplysettings(_gameSettinngs);
        SettingManager.SaveSettings(_gameSettinngs);
    }

    //public void Aplysettings()
    //{
    //    Screen.SetResolution(Resolutions[ResolutionDropdown.value].width, Resolutions[ResolutionDropdown.value].height, Screen.fullScreen);
    //    Screen.fullScreen = _gameSettinngs.fullScreen;
    //    QualitySettings.masterTextureLimit = _gameSettinngs.quality;
    //    QualitySettings.antiAliasing = (int)Mathf.Pow(2, _gameSettinngs.antialiasing);
    //    QualitySettings.vSyncCount = _gameSettinngs.vSync;

    //    GameSettinngs = _gameSettinngs;
    //}

    //public void SaveSettings()
    //{
    //    var jsonData = JsonUtility.ToJson(_gameSettinngs, true);
    //    File.WriteAllText(Application.persistentDataPath + "/gamesettings.json", jsonData);
    //}

    public void LoadInputs()
    {
        AudioVolumeSlider.value = _gameSettinngs.audioVolume;
        MouseSensitivitySlider.value = _gameSettinngs.mouseSensitivity;
        AntialiasingDropdown.value = _gameSettinngs.antialiasing;
        VSyncDropdown.value = _gameSettinngs.vSync;
        QualityDropdown.value = _gameSettinngs.quality;
        ResolutionDropdown.value = _gameSettinngs.resolutionIndex;
        FullscreenToggle.isOn = _gameSettinngs.fullScreen;

        ResolutionDropdown.RefreshShownValue();
    }
}

