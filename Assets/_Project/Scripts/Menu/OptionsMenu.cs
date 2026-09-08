using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    #region Graphics Fields
    [Header("Graphics Options")]
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Toggle vsyncToggle;
    [SerializeField] private List<ResolutionItem> resolutionItemsList;
    [SerializeField] private List<string> resolutionNamesList;
    [SerializeField] private TextMeshProUGUI resolutionText;
    [SerializeField] private TMP_Dropdown resolutionDropdownList;

    [SerializeField] private int selectedResolution;
    #endregion

    #region Audio Fields
    [Header("Audio Options")]
    [SerializeField] private AudioMixer mainAudioMixer;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TextMeshProUGUI masterVolumeText;
    [SerializeField] private TextMeshProUGUI musicVolumeText;
    [SerializeField] private TextMeshProUGUI sfxVolumeText;

    // The string names created as exposed parameters on the Main Audio Mixer in Unity
    private const string MASTER_VOLUME_MIXER_NAME = "MasterVolume";
    private const string MUSIC_VOLUME_MIXER_NAME = "MusicVolume";
    private const string SFX_VOLUME_MIXER_NAME = "SFXVolume";

    /// <summary>
    /// This is needed because Unity's default Mixer min and max values are -80 and 20, respectively, which are not intuitive for players.
    /// </summary>
    private const int VOLUME_OFFSET_VALUE = 80;
    #endregion

    private void Start()
    {
        SetupGraphicsOptions();
        UpdateResolutionText();

        LoadMasterVolume();
        LoadMusicVolume();
        LoadSFXVolume();

        resolutionDropdownList.onValueChanged.AddListener(OnDropdownValueChanged);
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            gameObject.SetActive(false);
    }

    #region Graphics Functions
    public void SetupGraphicsOptions()
    {
        vsyncToggle.isOn = QualitySettings.vSyncCount != 0;

        Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, fullscreenToggle.isOn);

        int count = Screen.resolutions.Length - 1;
        for (int i = 0; i < Screen.resolutions.Length; i++)
        {
            resolutionItemsList.Add(new ResolutionItem
            {
                horizontal = Screen.resolutions[i + count].width,
                vertical = Screen.resolutions[i + count].height
            });
            count -= 2;
            if (resolutionItemsList[i].horizontal == Screen.currentResolution.width && resolutionItemsList[i].vertical == Screen.currentResolution.height)
                selectedResolution = i;
        }

        SetResolutionDropdown();
    }

    public void ResolutionLeft()
    {
        selectedResolution = resolutionDropdownList.value;
        selectedResolution--;
        if (selectedResolution < 0)
            selectedResolution = resolutionItemsList.Count - 1;

        UpdateResolutionText();
        ApplyGraphics();
    }

    public void ResolutionRight()
    {
        selectedResolution = resolutionDropdownList.value;
        selectedResolution++;
        if (selectedResolution > resolutionItemsList.Count - 1)
            selectedResolution = 0;

        UpdateResolutionText();
        ApplyGraphics();
    }

    public void UpdateResolutionText()
    {
        resolutionText.text = $"{resolutionItemsList[selectedResolution].horizontal} x {resolutionItemsList[selectedResolution].vertical}";
        resolutionDropdownList.value = selectedResolution;
    }

    public void SetResolutionDropdown()
    {
        for (int i = 0; i < resolutionItemsList.Count; i++)
            resolutionNamesList.Add($"{resolutionItemsList[i].horizontal} x {resolutionItemsList[i].vertical}");

        resolutionDropdownList.AddOptions(resolutionNamesList);
    }

    public void OnDropdownValueChanged(int index)
    {
        selectedResolution = index;
        ApplyGraphics();
    }

    public void SetResolutionAndFullscreenMode()
    {
        Screen.SetResolution(resolutionItemsList[selectedResolution].horizontal, resolutionItemsList[selectedResolution].vertical, fullscreenToggle.isOn);
    }

    public void SetVSyncMode()
    {
        if (vsyncToggle.isOn)
            QualitySettings.vSyncCount = 1;
        else
            QualitySettings.vSyncCount = 0;
    }

    public void ApplyGraphics()
    {
        SetVSyncMode();
        SetResolutionAndFullscreenMode();
    }
    #endregion

    #region Volume Functions
    public void SetAllVolumeTexts()
    {
        SetMasterVolume();
        SetMusicVolume();
        SetSFXVolume();
    }

    public void SetVolumeText(TextMeshProUGUI volumeText, Slider volumeSlider, string volumeName)
    {
        mainAudioMixer.SetFloat(volumeName, volumeSlider.value);
        volumeText.text = $"{volumeSlider.value + VOLUME_OFFSET_VALUE}";
    }

    public void SetMasterVolume()
    {
        SetVolumeText(masterVolumeText, masterSlider, MASTER_VOLUME_MIXER_NAME);
        PlayerPrefs.SetFloat(MASTER_VOLUME_MIXER_NAME, masterSlider.value);
    }

    public void SetMusicVolume()
    {
        SetVolumeText(musicVolumeText, musicSlider, MUSIC_VOLUME_MIXER_NAME);
        PlayerPrefs.SetFloat(MUSIC_VOLUME_MIXER_NAME, musicSlider.value);
    }

    public void SetSFXVolume()
    {
        SetVolumeText(sfxVolumeText, sfxSlider, SFX_VOLUME_MIXER_NAME);
        PlayerPrefs.SetFloat(SFX_VOLUME_MIXER_NAME, sfxSlider.value);
    }

    public void LoadVolumeSettings(TextMeshProUGUI volumeText, Slider volumeSlider, string volumeName)
    {
        if (PlayerPrefs.HasKey(volumeName))
        {
            mainAudioMixer.SetFloat(volumeName, PlayerPrefs.GetFloat(volumeName));
            volumeSlider.value = PlayerPrefs.GetFloat(volumeName);
            volumeText.text = $"{volumeSlider.value + VOLUME_OFFSET_VALUE}";
        }
        else
            PlayerPrefs.SetFloat(volumeName, volumeSlider.value);
    }

    public void LoadMasterVolume()
    {
        LoadVolumeSettings(masterVolumeText, masterSlider, MASTER_VOLUME_MIXER_NAME);
    }

    public void LoadMusicVolume()
    {
        LoadVolumeSettings(musicVolumeText, musicSlider, MUSIC_VOLUME_MIXER_NAME);
    }

    public void LoadSFXVolume()
    {
        LoadVolumeSettings(sfxVolumeText, sfxSlider, SFX_VOLUME_MIXER_NAME);
    }
    #endregion
}

[System.Serializable]
public class ResolutionItem
{
    public int horizontal;
    public int vertical;
}