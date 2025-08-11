using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Represents a single mystery event.  Events can expose any number of
/// selectable options.  Each option contains a UnityEvent that can be wired up
/// in the inspector to perform custom logic (grant items, start battles, etc.).
/// </summary>
public class MysteryEvent : MonoBehaviour
{
    [Tooltip("The name/title of this event (shown at the top of the UI).")]
    public string eventName;

    [Tooltip("Short description that will be shown to the player."), TextArea]
    public string description;

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
            Debug.LogWarning("Invalid mystery event option index");
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
