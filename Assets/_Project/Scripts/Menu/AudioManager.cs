using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioMixer mainAudioMixer;

    private const string MASTER_VOLUME_MIXER_NAME = "MasterVolume";
    private const string MUSIC_VOLUME_MIXER_NAME = "MusicVolume";
    private const string SFX_VOLUME_MIXER_NAME = "SFXVolume";

    private void Start()
    {
        LoadVolumeSettings(MASTER_VOLUME_MIXER_NAME);
        LoadVolumeSettings(MUSIC_VOLUME_MIXER_NAME);
        LoadVolumeSettings(SFX_VOLUME_MIXER_NAME);
    }

    public void LoadVolumeSettings(string volumeName)
    {
        if (PlayerPrefs.HasKey(volumeName))
            mainAudioMixer.SetFloat(volumeName, PlayerPrefs.GetFloat(volumeName));
    }
}