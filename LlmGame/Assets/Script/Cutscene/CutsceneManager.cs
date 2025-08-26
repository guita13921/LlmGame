using UnityEngine;
using UnityEngine.Playables;

public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager Instance { get; private set; }

    [Header("Cutscenes")]
    [SerializeField] private PlayableDirector startGameCutscene;
    [SerializeField] private PlayableDirector endGameCutscene;

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

    public void PlayStartGameCutscene()
    {
        if (startGameCutscene != null)
        {
            startGameCutscene.Play();
        }
    }

    public void PlayEndGameCutscene()
    {
        if (endGameCutscene != null)
        {
            endGameCutscene.Play();
        }
    }
}
