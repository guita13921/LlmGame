using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using TMPro;

public class InputKeywordHighlighter : MonoBehaviour
{
    [Header("References")]
    public TMP_InputField inputField;
    public TMP_Text displayText;

    [SerializeField] public BattleManager battleManager;

    [Header("Highlight Colors")]
    public Color mainWeaponColor = Color.blue;
    public Color subWeaponColor = Color.yellow;

    // Map each keyword to its ItemType
    private Dictionary<string, ItemType> keywordToType = new Dictionary<string, ItemType>();

    void Start()
    {
        if (inputField == null || displayText == null)
        {
            Debug.LogError("Missing references on InputKeywordHighlighter.");
            return;
        }

        if (battleManager == null || battleManager.player == null || battleManager.player.inventoryItems == null)
        {
            Debug.LogError("Missing BattleManager or inventoryItems.");
            return;
        }

        // Map keywords to their ItemType
        foreach (var item in battleManager.player.inventoryItems)
        {
            if (item.itemType == ItemType.Other)
                continue; // Skip keywords from 'Other' items

            foreach (var keyword in item.keyWords)
            {
                string lowerKeyword = keyword.ToLower();
                if (!keywordToType.ContainsKey(lowerKeyword))
                {
                    keywordToType.Add(lowerKeyword, item.itemType);
                }
            }
        }

        inputField.onValueChanged.AddListener(OnTextChanged);
    }

    void OnDestroy()
    {
        if (inputField != null)
            inputField.onValueChanged.RemoveListener(OnTextChanged);
    }

    void OnTextChanged(string userInput)
    {
        string pattern = @"\b\w+\b";
        var matches = Regex.Matches(userInput, pattern);

        int lastIndex = 0;
        string formattedText = "";

        foreach (Match match in matches)
        {
            formattedText += userInput.Substring(lastIndex, match.Index - lastIndex);

            string word = match.Value;
            string lowerWord = word.ToLower();

            if (keywordToType.TryGetValue(lowerWord, out ItemType itemType))
            {
                string colorHex = "";

                switch (itemType)
                {
                    case ItemType.Main_Weapon:
                        colorHex = ColorUtility.ToHtmlStringRGB(mainWeaponColor);
                        break;
                    case ItemType.Sub_Weapon:
                        colorHex = ColorUtility.ToHtmlStringRGB(subWeaponColor);
                        break;
                    default:
                        colorHex = ""; // Do not highlight for 'Other'
                        break;
                }

                if (!string.IsNullOrEmpty(colorHex))
                {
                    formattedText += $"<color=#{colorHex}>{word}</color>";
                }
                else
                {
                    formattedText += word;
                }
            }
            else
            {
                formattedText += word;
            }

            lastIndex = match.Index + match.Length;
        }

        formattedText += userInput.Substring(lastIndex);
        displayText.text = formattedText;
    }
}
