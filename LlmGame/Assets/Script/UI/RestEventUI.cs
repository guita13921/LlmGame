using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Map;                  // For NodeType / EnemyDifficulty
using EasyTransition;       // For TransitionManager / TransitionSettings
using Random = UnityEngine.Random;

public class RestEventUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Top title text for the event name.")]
    public TMP_Text eventNameText;

    [Tooltip("Main body description of the current event.")]
    public TMP_Text descriptionText;

    [Tooltip("Where the outcome/result text appears after a click (from RestSceneEventHandler.OnOutcome).")]
    public TMP_Text resultText;

    [Tooltip("Parent transform with a Layout Group to hold option entries.")]
    public Transform optionsContainer;

    [Tooltip("Prefab for an option. Can be a full Button OR a GameObject with only TMP_Text.")]
    public GameObject optionPrefab;

    [Header("Behavior")]
    [Tooltip("If > 0, only the first N options (in defined order) are shown.")]
    public int maxOptionsToShow = 0;

    [Tooltip("If true and no current event from system, pick one randomly from the pool below.")]
    public bool pickRandomFromPoolIfNone = true;

    [Tooltip("Event pool to use if no current event from system is found.")]
    public List<RestEvent> eventPool = new();

    [Tooltip("Delay (seconds) after selecting an option before switching scenes.")]
    public float postSelectDelay = 2f;

    [Tooltip("Scene to load after selection delay (fallback if not going to battle).")]
    public string nextSceneName = "MapGenerate";

    [Header("Transition")]
    [Tooltip("Settings used for transitioning to next scene.")]
    public TransitionSettings transition;

    [Tooltip("Delay before the transition starts.")]
    public float startDelay = 0f;

    private RestEvent restEvent;
    private bool choiceLocked = false; // prevents multiple selections
    private readonly List<Button> spawnedButtons = new();

    private void OnEnable()
    {
        RestEventSystem.OnEventSpawned += HandleEventSpawned;
        RestSceneEventHandler.OnOutcome += HandleOutcome;
        TryInitFromExistingOrPool();
    }

    private void OnDisable()
    {
        RestEventSystem.OnEventSpawned -= HandleEventSpawned;
        RestSceneEventHandler.OnOutcome -= HandleOutcome;
    }

    public void LoadScene(string _sceneName)
    {
        TransitionManager.Instance().Transition(_sceneName, transition, startDelay);
    }

    private void TryInitFromExistingOrPool()
    {
        var system = FindObjectOfType<RestEventSystem>();
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

    private void HandleEventSpawned(RestEvent evt)
    {
        restEvent = evt;
        choiceLocked = false; // unlock for new event
        UpdateUI();
    }

    private void HandleOutcome(string rawOutcome)
    {
        if (resultText != null)
            resultText.text = rawOutcome ?? string.Empty;
    }

    public void UpdateUI()
    {
        if (restEvent == null) return;

        if (eventNameText != null)
            eventNameText.text = restEvent.eventName;

        if (descriptionText != null)
            descriptionText.text = restEvent.description;

        if (resultText != null)
            resultText.text = string.Empty;

        RebuildOptionButtons();
    }

    private void RebuildOptionButtons()
    {
        if (optionsContainer == null || optionPrefab == null || restEvent == null)
            return;

        // Clear old children
        for (int i = optionsContainer.childCount - 1; i >= 0; i--)
            Destroy(optionsContainer.GetChild(i).gameObject);
        spawnedButtons.Clear();

        // Copy options preserving order
        var options = new List<RestEvent.Option>(restEvent.options);

        // Limit to first N if set
        if (maxOptionsToShow > 0 && options.Count > maxOptionsToShow)
            options = options.Take(maxOptionsToShow).ToList();

        // Build entries in the same order
        foreach (var opt in options)
        {
            // Instantiate prefab (can be text-only or a full button)
            var go = Instantiate(optionPrefab, optionsContainer);

            // Ensure it's actually clickable
            var button = EnsureButton(go, out var targetGraphic);

            // Ensure we have a label to write to
            var label = EnsureLabel(go);

            // Display: "Action — Result"
            string actionText = opt.optionText;
            string outcomeText = opt.descriptionText;
            string display = string.IsNullOrEmpty(outcomeText)
                ? actionText
                : $"{actionText} — {outcomeText}";
            if (label != null) label.text = display;

            // wire up onclick
            button.onClick.RemoveAllListeners();

            // get original index from the event list (not the sliced one)
            int originalIndex = restEvent.options.IndexOf(opt);

            button.onClick.AddListener(() =>
            {
                if (choiceLocked) return;
                choiceLocked = true;

                SetButtonsInteractable(false);
                StartCoroutine(HandleSelectionCoroutine(originalIndex));
            });

            spawnedButtons.Add(button);
        }

        // initial interactable state
        SetButtonsInteractable(!choiceLocked);

        // Make sure we have an EventSystem in the scene
        EnsureEventSystem();
    }

    private IEnumerator HandleSelectionCoroutine(int optionIndex)
    {
        // Invoke the linked gameplay action
        restEvent?.ChooseOption(optionIndex);

        // wait before scene change
        yield return new WaitForSeconds(postSelectDelay);

        // NEW: if the action failed (e.g., not enough credits), do NOT change scene
        if (!RestSceneEventHandler.AllowAdvanceAfterLastAction)
        {
            // Re-enable choices so the player can pick another option
            choiceLocked = false;
            SetButtonsInteractable(true);
            yield break;
        }

        // Proceed with scene change as before
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            var data = PlayerData.Instance;
            string sceneToLoad = nextSceneName;

            if (data != null && (data.nextNodeType == NodeType.MinorEnemy ||
                                 data.nextNodeType == NodeType.EliteEnemy ||
                                 data.nextNodeType == NodeType.Boss))
            {
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

    /// <summary>
    /// Ensures a Button and a raycastable Graphic exist on the GameObject.
    /// If missing, adds a Button and a transparent Image so clicks work.
    /// </summary>
    private Button EnsureButton(GameObject go, out Graphic targetGraphic)
    {
        var button = go.GetComponent<Button>();
        targetGraphic = null;

        // Prefer existing Image as the targetGraphic
        var image = go.GetComponent<Image>();
        if (image != null)
        {
            image.raycastTarget = true;
            targetGraphic = image;
        }
        else
        {
            // TMP_Text can be raycastable, but Button still expects a Graphic target
            var tmp = go.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.raycastTarget = true;
                targetGraphic = tmp;
            }
        }

        if (button == null)
            button = go.AddComponent<Button>();

        // If still no Graphic, add a transparent Image to catch clicks
        if (targetGraphic == null)
        {
            image = go.AddComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0f); // invisible but raycastable
            image.raycastTarget = true;
            targetGraphic = image;
        }

        if (button.targetGraphic == null)
            button.targetGraphic = targetGraphic;

        return button;
    }

    /// <summary>
    /// Ensures there is a TMP_Text on this GO or a child; creates one if missing.
    /// </summary>
    private TMP_Text EnsureLabel(GameObject go)
    {
        var label = go.GetComponent<TMP_Text>();
        if (label == null) label = go.GetComponentInChildren<TMP_Text>(true);

        if (label == null)
        {
            // Create child TMP_Text if none exists
            var textGO = new GameObject("Label", typeof(RectTransform));
            textGO.transform.SetParent(go.transform, false);

            var rt = (RectTransform)textGO.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.enableWordWrapping = true;
            tmp.raycastTarget = false; // parent catches clicks
            label = tmp;
        }

        return label;
    }

    /// <summary>
    /// Ensures an EventSystem exists (needed for UI clicks).
    /// </summary>
    private void EnsureEventSystem()
    {
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var es = new GameObject("EventSystem",
                typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.EventSystems.StandaloneInputModule));
            DontDestroyOnLoad(es);
        }
    }
}
