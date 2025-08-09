using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public DefaultPlayerConfig defaultConfig;
    public string gameplaySceneName;

    void Start()
    {
        OnNewGameButton();
    }

    public void OnNewGameButton()
    {
        if (PlayerData.Instance == null)
        {
            Debug.LogError("PlayerData singleton not found in the boot scene.");
            return;
        }

        PlayerData.Instance.NewGame(defaultConfig);
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void OnContinueButton()
    {
        // If you have disk persistence, load it here, then:
        // PlayerData.Instance.LoadFromDisk();
        // For now, just proceed if PlayerData was already initialized in the session.
        if (!PlayerData.Instance.Initialized)
        {
            Debug.LogWarning("No save found in memory. Starting a New Game instead.");
            PlayerData.Instance.NewGame(defaultConfig);
        }

        SceneManager.LoadScene(gameplaySceneName);
    }
}
