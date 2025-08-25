using UnityEngine;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("Assign Audio Mixer")]
    public AudioMixer audioMixer;

    private const string ShowFeasibilityDescKey = "ShowFeasibilityDesc";
    private const string ShowPotentialDescKey = "ShowPotentialDesc";

    [Header("Display Settings")]
    public bool showFeasibilityDesc = true;
    public bool showPotentialDesc = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        showFeasibilityDesc = PlayerPrefs.GetInt(ShowFeasibilityDescKey, 1) == 1;
        showPotentialDesc = PlayerPrefs.GetInt(ShowPotentialDescKey, 1) == 1;
    }

    public void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1)) * 20);
    }

    public void SetVFXVolume(float volume)
    {
        audioMixer.SetFloat("VFXVolume", Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1)) * 20);
    }

    public void SetShowFeasibilityDesc(bool value)
    {
        showFeasibilityDesc = value;
        PlayerPrefs.SetInt(ShowFeasibilityDescKey, value ? 1 : 0);
    }

    public void SetShowPotentialDesc(bool value)
    {
        showPotentialDesc = value;
        PlayerPrefs.SetInt(ShowPotentialDescKey, value ? 1 : 0);
    }
}
