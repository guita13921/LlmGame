using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Names")]
    public string mapGenerateSceneName = "MapGenerate"; // Assign this in Inspector

    [Header("UI Panels")]
    public GameObject settingsPanel;

    [Header("Audio Sliders")]
    public Slider musicSlider;
    public Slider vfxSlider;

    [Header("Display Toggles")]
    public Toggle showFeasibilityDescToggle;
    public Toggle showPotentialDescToggle;

    [Header("Player Config")]
    public DefaultPlayerConfig defaultConfig;

    void Start()
    {
        // Optional auto-start: comment out in production
        // OnStartButton();

        if (showFeasibilityDescToggle != null)
            showFeasibilityDescToggle.isOn = SettingsManager.Instance.showFeasibilityDesc;
        if (showPotentialDescToggle != null)
            showPotentialDescToggle.isOn = SettingsManager.Instance.showPotentialDesc;
    }

    public void OnStartButton()
    {
        if (PlayerData.Instance == null)
        {
            Debug.LogError("PlayerData singleton not found in the boot scene.");
            return;
        }

        PlayerData.Instance.NewGame(defaultConfig);
        SceneManager.LoadScene(mapGenerateSceneName);
    }

    public void OnOptionsButton()
    {
        settingsPanel.SetActive(true);

        if (showFeasibilityDescToggle != null)
            showFeasibilityDescToggle.isOn = SettingsManager.Instance.showFeasibilityDesc;
        if (showPotentialDescToggle != null)
            showPotentialDescToggle.isOn = SettingsManager.Instance.showPotentialDesc;
    }

    public void OnExitButton()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void OnCloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    public void OnMusicVolumeChanged(float value)
    {
        SettingsManager.Instance.SetMusicVolume(value);
    }

    public void OnVFXVolumeChanged(float value)
    {
        SettingsManager.Instance.SetVFXVolume(value);
    }

    public void OnShowFeasibilityDescChanged(bool value)
    {
        SettingsManager.Instance.SetShowFeasibilityDesc(value);
    }

    public void OnShowPotentialDescChanged(bool value)
    {
        SettingsManager.Instance.SetShowPotentialDesc(value);
    }
}
