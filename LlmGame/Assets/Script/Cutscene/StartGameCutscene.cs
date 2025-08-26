using UnityEngine;

public class StartGameCutscene : MonoBehaviour
{
    private void Start()
    {
        CutsceneManager.Instance?.PlayStartGameCutscene();
    }
}
