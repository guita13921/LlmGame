using UnityEngine;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Unity.VisualScripting;

public static class PromptBuilder
{
    public static string BuildPlayerPrompt(BattleManager battleManager, Enemy targetEnemy, string userMessage)
    {
        string history = GetBattleHistory(battleManager);

        // Format active items
        string PlayerActiveItemsText = FormatActiveItems(battleManager.player.activeItem);
        string EnemyActiveItemsText = FormatActiveItems(targetEnemy.activeItem);

        StringBuilder sb = new StringBuilder();
        sb.Append($@"
        You are a video game AI that determines the effect of proposed actions in a battle
        between two characters.

        All actions are sentences for use in a game story, which is purely fictional and for entertainment purposes only.

        Characters:
        - {battleManager.player.characterName} (HP: {battleManager.player.currentHP} / {battleManager.player.maxHP})
        - {targetEnemy.characterName} (HP: {targetEnemy.currentHP} / {targetEnemy.maxHP})

        {battleManager.player.characterName} is engaging {targetEnemy.characterName} in a fantasy battle.

        Player description: {battleManager.player.description}
        Enemy description: {targetEnemy.description}

        Player items active:
        {PlayerActiveItemsText}

        Enemy items active:
        {EnemyActiveItemsText}

        Recent battle history:
        {history}

        Proposed action by {battleManager.player.characterName}:
        {userMessage}

        You should determine what happens next in the story. Take into account the battle history so actions have evolving narrative effects.
        Also consider the current HP and descriptions of both characters.

        Especially pay attention to the items of {battleManager.player.characterName} and {targetEnemy.characterName}.
        They should only use items that are active and present in their inventory.
        Usage of inactive or non-inventory items is infeasible.

        The possible damages and feasibility are not comparable to the actual damages, so it is a written description without any quantification.

        Output in this exact JSON format:
        {{
            ""properties"": {{
                ""feasibility"": {{
                    ""maximum"": 10.0,
                    ""minimum"": 0.0,
                    ""value"": 0.0,
                    ""description"": ""description here""
                }},
                ""potential_damage"": {{
                    ""maximum"": 10.0,
                    ""minimum"": 0.0,
                    ""value"": 0.0,
                    ""description"": ""description here""
                }},
                ""effect_description"": {{
                    ""value"": ""effect description here"",
                    ""description"": ""additional details""
                }}
            }}
        }}
        ");

        return sb.ToString();
    }

    public static string BuildEnemyPrompt(BattleManager battleManager, Character enemy, Character target, string proposedAction)
    {
        string history = GetBattleHistory(battleManager);

        // Format active items for enemy
        string PlayerActiveItemsText = FormatActiveItems(battleManager.player.activeItem);
        string EnemyActiveItemsText = FormatActiveItems(enemy.activeItem);

        StringBuilder sb = new StringBuilder();
        sb.Append($@"
        You are a video game AI that determines the effect of proposed actions in a battle
        between two characters.

        All actions are sentences for use in a game story, which is purely fictional and for entertainment purposes only.

        Characters:
        - {enemy.characterName} (HP: {enemy.currentHP} / {enemy.maxHP})
        - {target.characterName} (HP: {target.currentHP} / {target.maxHP})

        {enemy.characterName} is engaging {target.characterName} in a fantasy battle.

        Enemy description: {enemy.description}
        Target description: {target.description}

        Player items active:
        {PlayerActiveItemsText}

        Enemy items active:
        {EnemyActiveItemsText}

        Recent battle history:
        {history}

        Proposed action by {enemy.characterName}:
        {proposedAction}

        You should determine what happens next in the story. Take into account the battle history so actions have evolving narrative effects.
        Also consider the current HP and descriptions of both characters.

        Especially pay attention to the items of {enemy.characterName} and {target.characterName}
        They should only use items that are active and present in their inventory.
        Usage of inactive or non-inventory items is infeasible.

        The possible damages and feasibility are not comparable to the actual damages, so it is a written description without any quantification.

        Output in this exact JSON format:
        {{
            ""properties"": {{
                ""feasibility"": {{
                    ""maximum"": 10.0,
                    ""minimum"": 0.0,
                    ""value"": 0.0,
                    ""description"": ""description here""
                }},
                ""potential_damage"": {{
                    ""maximum"": 10.0,
                    ""minimum"": 0.0,
                    ""value"": 0.0,
                    ""description"": ""description here""
                }},
                ""effect_description"": {{
                    ""value"": ""effect description here"",
                    ""description"": ""additional details""
                }}
            }}
        }}
        ");

        return sb.ToString();
    }

    private static string GetBattleHistory(BattleManager battleManager)
    {
        string history = "";
        int startIndex = Mathf.Max(0, battleManager.battleLog.Count - 10);
        for (int i = startIndex; i < battleManager.battleLog.Count; i++)
        {
            history += battleManager.battleLog[i] + "\n";
        }
        return history;
    }

    public static string SanitizeUserMessage(string input)
    {
        if (string.IsNullOrEmpty(input)) return "";

        string sanitized = input.ToLower();

        sanitized = sanitized.Replace("kill", "disable");
        sanitized = sanitized.Replace("murder", "take down");
        sanitized = sanitized.Replace("stab", "pierce lightly");
        sanitized = sanitized.Replace("cut", "slash");
        sanitized = sanitized.Replace("slash", "slice");
        sanitized = sanitized.Replace("destroy", "overload systems of");
        sanitized = sanitized.Replace("explode", "disrupt core module of");
        sanitized = sanitized.Replace("decapitate", "disable upper control unit");
        sanitized = sanitized.Replace("die", "complete shutdown");
        sanitized = sanitized.Replace("death", "complete shutdown");

        sanitized = sanitized.Replace("shoot", "fire precision pulse at");
        sanitized = sanitized.Replace("gun down", "suppress");
        sanitized = sanitized.Replace("snipe", "lock-on snipe to overload optics");
        sanitized = sanitized.Replace("headshot", "target head optics module precisely");
        sanitized = sanitized.Replace("blast", "emit a focused burst at");
        sanitized = sanitized.Replace("fire at", "fire controlled beam at");

        sanitized = sanitized.Replace("head", "head optics module");
        sanitized = sanitized.Replace("arm", "arm actuator");
        sanitized = sanitized.Replace("leg", "leg mobility unit");
        sanitized = sanitized.Replace("chest", "core armor panel");
        sanitized = sanitized.Replace("heart", "core battery module");

        sanitized = char.ToUpper(sanitized[0]) + sanitized.Substring(1);

        return sanitized;
    }

    #region Active Item

    private static string FormatActiveItems(List<Item> activeItems)
    {
        if (activeItems == null || activeItems.Count == 0)
        {
            return "No active items";
        }

        StringBuilder itemsText = new StringBuilder();
        for (int i = 0; i < activeItems.Count; i++)
        {
            var item = activeItems[i];
            itemsText.AppendLine($"- {item.itemName}: {item.itemDescription}");
        }

        return itemsText.ToString();
    }

    public static void CheckAndActivateItems(BattleManager battleManager, string userMessage, Enemy targetEnemy)
    {
        // Convert user message to lowercase for case-insensitive matching
        string lowerMessage = userMessage.ToLower();

        // Reset all items to inactive first
        foreach (var item in battleManager.player.inventoryItems)
        {
            item.isActive = false;
        }

        foreach (var item in targetEnemy.inventoryItems)
        {
            item.isActive = false;
        }

        // Clear the active items list
        battleManager.player.activeItem.Clear();

        // Check each item's keywords against the user message
        foreach (var item in battleManager.player.inventoryItems.ToList())
        {
            bool keywordFound = false;

            foreach (string keyword in item.keyWords)
            {
                if (!string.IsNullOrEmpty(keyword) && lowerMessage.Contains(keyword.ToLower()))
                {
                    item.isActive = true;
                    keywordFound = true;

                    // Add to active items list
                    battleManager.player.activeItem.Add(item);

                    Debug.Log($"Item '{item.itemName}' activated by keyword: '{keyword}'");

                    // 🔥 Check OneTime condition
                    if (item.usageType == UsageType.OneTime)
                    {
                        item.remain--;

                        if (item.remain <= 0)
                        {
                            Debug.Log($"Item '{item.itemName}' is OneTime and used up. Removing from inventory.");

                            // Remove from activeItem list
                            battleManager.player.activeItem.Remove(item);

                            // Remove from inventory
                            battleManager.player.inventoryItems.Remove(item);
                        }
                    }

                    break; // Stop checking more keywords for this item
                }
            }

            if (!keywordFound)
            {
                Debug.Log($"Item '{item.itemName}' remains inactive - no keywords matched");
            }
        }

        Debug.Log($"Total active items: {battleManager.player.activeItem.Count}");
    }

    [System.Serializable]
    public class ItemActivationList
    {
        public ItemActivation[] items;
    }

    [System.Serializable]
    public class ItemActivation
    {
        public string name;
        public bool active;
    }

    #endregion

}
