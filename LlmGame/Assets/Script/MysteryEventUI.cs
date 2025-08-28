using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Map;
using EasyTransition;

public class MysteryEventUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Top title text for the event name.")]
    public TMP_Text eventNameText;

    [Tooltip("Main body description of the current event.")]
    public TMP_Text descriptionText;

    [Tooltip("Where the outcome/result text appears after a click.")]
    public TMP_Text resultText;

    [Tooltip("Parent transform with a Vertical/Horizontal Layout Group to hold option buttons.")]
    public Transform optionsContainer;

    [Tooltip("Prefab with a Button + TMP_Text (or child text) to display an option.")]
    public Button optionButtonPrefab;

    [Header("Behavior")]
    [Tooltip("If > 0, only the first N options (in defined order) are shown.")]
    public int maxOptionsToShow = 0;

    [Tooltip("If true and no event is active in the system, pick one randomly from the pool below.")]
    public bool pickRandomFromPoolIfNone = true;

    [Tooltip("Event pool to use if no current event from system.")]
    public List<MysteryEvent> eventPool = new();

    [Tooltip("Delay (seconds) after selecting an option before switching scenes.")]
    public float postSelectDelay = 2f;

    [Tooltip("Scene to load after selection delay.")]
    public string nextSceneName = "MapGenerate";

    [Header("Transition")]
    [Tooltip("Settings used for transitioning to next scene.")]
    public TransitionSettings transition;

    [Tooltip("Delay before the transition starts.")]
    public float startDelay = 0f;

    private MysteryEvent mysteryEvent;
    private bool choiceLocked = false; // prevents double click and multiple selections
    private readonly List<Button> spawnedButtons = new();

    private void OnEnable()
    {
        MysteryEventSystem.OnEventSpawned += HandleEventSpawned;
        MysterySceneEventHandler.OnOutcome += HandleOutcome;   // shows actual result text from handler
        TryInitFromExistingOrPool();
    }

    private void OnDisable()
    {
        MysteryEventSystem.OnEventSpawned -= HandleEventSpawned;
        MysterySceneEventHandler.OnOutcome -= HandleOutcome;
    }

    public void LoadScene(string _sceneName)
    {
        TransitionManager.Instance().Transition(_sceneName, transition, startDelay);
    }

    private void TryInitFromExistingOrPool()
    {
        var system = FindObjectOfType<MysteryEventSystem>();
        if (system != null && system.Current != null)
        {
            HandleEventSpawned(system.Current);
            return;
        }

        if (pickRandomFromPoolIfNone && eventPool != null && eventPool.Count > 0)
        {
            var randomEvt = eventPool[Random.Range(0, eventPool.Count)];
            HandleEventSpawned(randomEvt);
        }
    }

    private void HandleEventSpawned(MysteryEvent evt)
    {
        mysteryEvent = evt;
        choiceLocked = false; // new event, unlock choices
        UpdateUI();
    }

    private void HandleOutcome(string rawOutcome)
    {
        if (resultText != null)
            resultText.text = rawOutcome ?? string.Empty;
    }

    public void UpdateUI()
    {
        if (mysteryEvent == null) return;

        if (eventNameText != null)
            eventNameText.text = mysteryEvent.eventName;

        if (descriptionText != null)
            descriptionText.text = mysteryEvent.description;

        if (resultText != null)
            resultText.text = string.Empty;

        RebuildOptionButtons();
    }

    private void RebuildOptionButtons()
    {
        if (optionsContainer == null || optionButtonPrefab == null || mysteryEvent == null)
            return;

        // Clear old children
        for (int i = optionsContainer.childCount - 1; i >= 0; i--)
            Destroy(optionsContainer.GetChild(i).gameObject);
        spawnedButtons.Clear();

        // Copy options preserving order
        var options = new List<MysteryEvent.Option>(mysteryEvent.options);

        // Limit: take the first N in defined order
        if (maxOptionsToShow > 0 && options.Count > maxOptionsToShow)
            options = options.Take(maxOptionsToShow).ToList();

        // Build buttons in the same order as options
        foreach (var opt in options)
        {
            var button = Instantiate(optionButtonPrefab, optionsContainer);
            spawnedButtons.Add(button);

            var label = GetButtonLabel(button);

            // Display: "Action — Result"
            string actionText = opt.optionText;
            string outcome = opt.descriptionText;
            string display = string.IsNullOrEmpty(outcome)
                ? actionText
                : $"{actionText} — {outcome}";
            if (label != null) label.text = display;

            button.onClick.RemoveAllListeners();

            // capture local for closure
            var capturedOption = opt;
            button.onClick.AddListener(() =>
            {
                if (choiceLocked) return;        // prevent double-clicks
                choiceLocked = true;

                // disable all buttons immediately
                SetButtonsInteractable(false);

                // run the option and then change scene after delay
                StartCoroutine(HandleSelectionCoroutine(capturedOption));
            });
        }

        // ensure initial interactable state
        SetButtonsInteractable(!choiceLocked);
    }

    private IEnumerator HandleSelectionCoroutine(MysteryEvent.Option option)
    {
        // Invoke the linked gameplay action
        option?.onSelected?.Invoke();

        // wait before scene change
        yield return new WaitForSeconds(postSelectDelay);

        // load next scene
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            var data = PlayerData.Instance;
            string sceneToLoad = nextSceneName;

            if (data != null && (data.nextNodeType == NodeType.MinorEnemy ||
                                 data.nextNodeType == NodeType.EliteEnemy ||
                                 data.nextNodeType == NodeType.Boss))
            {
                Debug.Log(data.nextNodeType);
                sceneToLoad = "BattleScene02";
            }

            var player = FindObjectOfType<Player>();
            PlayerData.Instance?.GainMPOnNodeExit(player);
            PlayerData.Instance?.SavePlayer(player);

            LoadScene(sceneToLoad);
        }
    }

    private void SetButtonsInteractable(bool interactable)
    {
        foreach (var btn in spawnedButtons)
        {
            if (btn != null) btn.interactable = interactable;
        }
    }

    // Attempts to find a TMP_Text on the Button or its children
    private TMP_Text GetButtonLabel(Button btn)
    {
        var text = btn.GetComponent<TMP_Text>();
        if (text != null) return text;
        return btn.GetComponentInChildren<TMP_Text>(true);
    }
}
