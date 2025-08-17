using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Rest event definition. Works like MysteryEvent (name/description/options),
/// but also carries a RestEventType so systems can theme backgrounds.
/// </summary>
public enum RestEventType { Camp, Bar, Sofa }

public class RestEvent : MonoBehaviour
{
    [Header("Identity")]
    [Tooltip("What kind of rest event this is (used for background/theming).")]
    public RestEventType eventType = RestEventType.Camp;

    [Tooltip("The name/title of this event (shown at the top of the UI).")]
    public string eventName;

    [Tooltip("Short description that will be shown to the player."), TextArea]
    public string description;

    [Header("Optional Art")]
    [Tooltip("If set, this sprite can be used as a per-event artwork/icon in the UI.")]
    public Sprite icon;

    [Tooltip("Optional: override background specifically for this event (takes precedence over type-based background).")]
    public Sprite backgroundOverride;

    [Header("Options")]
    [Tooltip("List of options the player can choose from for this event.")]
    public List<Option> options = new();

    /// <summary>
    /// Invoke the outcome for an option at the given index.
    /// UI elements should call this when the player makes a selection.
    /// </summary>
    public void ChooseOption(int index)
    {
        if (index < 0 || index >= options.Count)
        {
            Debug.LogWarning("[RestEvent] Invalid option index.", this);
            return;
        }

        options[index]?.onSelected?.Invoke();
    }

    [Serializable]
    public class Option
    {
        [Tooltip("Text shown on the button for this option.")]
        public string optionText;

        [TextArea]
        [Tooltip("Additional descriptive text about the option's outcome.")]
        public string descriptionText;

        [Tooltip("The outcome that will occur when the option is chosen.")]
        public UnityEvent onSelected;
    }
}
