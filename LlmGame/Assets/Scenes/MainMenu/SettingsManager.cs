using UnityEngine;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("Assign Audio Mixer")]
    public AudioMixer audioMixer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1)) * 20);
    }

    public void SetVFXVolume(float volume)
    {
        audioMixer.SetFloat("VFXVolume", Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1)) * 20);
    }
}
