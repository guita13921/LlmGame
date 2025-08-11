using System.Text;
using TMPro;
using UnityEngine;

/// <summary>
/// Displays the current mystery event's description and option names,
/// automatically hooking into the event spawned by MysteryEventSystem.
/// </summary>
public class MysteryEventUI : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text descriptionText;
    public TMP_Text optionsText;

    private MysteryEvent mysteryEvent;

    private void OnEnable()
    {
        MysteryEventSystem.OnEventSpawned += HandleEventSpawned;
        TryInitFromExisting();
    }

    private void OnDisable()
    {
        MysteryEventSystem.OnEventSpawned -= HandleEventSpawned;
    }

    private void TryInitFromExisting()
    {
        // If the system already spawned an event before we subscribed, grab it.
        var system = FindObjectOfType<MysteryEventSystem>();
        if (system != null && system.Current != null)
        {
            HandleEventSpawned(system.Current);
        }
    }

    private void HandleEventSpawned(MysteryEvent evt)
    {
        mysteryEvent = evt;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (mysteryEvent == null) return;

        if (descriptionText != null)
            descriptionText.text = mysteryEvent.description;

        if (optionsText != null)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < mysteryEvent.options.Count; i++)
            {
                var option = mysteryEvent.options[i];
                sb.AppendLine($"{i + 1}. {option.optionText}");
            }
            optionsText.text = sb.ToString();
        }
    }
}
