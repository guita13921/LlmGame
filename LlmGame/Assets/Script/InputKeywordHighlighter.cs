using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using TMPro;

public class InputKeywordHighlighter : MonoBehaviour
{
    [Header("References")]
    public TMP_InputField inputField;
    public TMP_Text displayText; // Text object that shows highlighted text.

    [SerializeField] public BattleManager battleManager;

    [Header("Color")]
    public Color highlightColor = Color.green;

    private HashSet<string> allKeywords = new HashSet<string>();

    void Start()
    {
        if (inputField == null || displayText == null)
        {
            Debug.LogError("Missing references on InputKeywordHighlighter.");
            return;
        }

        // Collect all keywords from inventory
        foreach (var item in battleManager.player.inventoryItems)
        {
            foreach (var keyword in item.keyWords)
            {
                allKeywords.Add(keyword.ToLower()); // Lowercase for case-insensitive match
            }
        }

        // Subscribe to input event
        inputField.onValueChanged.AddListener(OnTextChanged);
    }

    void OnDestroy()
    {
        inputField.onValueChanged.RemoveListener(OnTextChanged);
    }

    void OnTextChanged(string userInput)
    {
        // If you want to escape < and >, use the EscapeRichText method, otherwise just use userInput directly.
        string escapedInput = userInput;

        // Regex pattern to match words (ignoring punctuation and whitespace)
        string pattern = @"\b\w+\b";
        var matches = Regex.Matches(escapedInput, pattern);

        int lastIndex = 0;
        string formattedText = "";

        foreach (Match match in matches)
        {
            formattedText += escapedInput.Substring(lastIndex, match.Index - lastIndex);

            string word = match.Value;

            if (allKeywords.Contains(word.ToLower()))
            {
                string colorHex = ColorUtility.ToHtmlStringRGB(highlightColor);
                formattedText += $"<color=#{colorHex}>{word}</color>";
            }
            else
            {
                formattedText += word;
            }

            lastIndex = match.Index + match.Length;
        }

        // Add remaining text after last word
        formattedText += escapedInput.Substring(lastIndex);

        // Update display text
        displayText.text = formattedText;
    }

}
