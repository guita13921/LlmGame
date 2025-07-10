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
        string activeItemsText = FormatActiveItems(battleManager.player.activeItem);

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
        {activeItemsText}

        Recent battle history:
        {history}

        Proposed action by {battleManager.player.characterName}:
        {userMessage}

        You should determine what happens next in the story. Take into account the battle history so actions have evolving narrative effects.
        Also consider the current HP and descriptions of both characters.

        Especially pay attention to the items of {battleManager.player.characterName}.
        They should only use items that are active (marked true in the JSON) and present in their inventory.
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

        StringBuilder sb = new StringBuilder();
        sb.Append($@"You are a video game ai that determines the effect of proposed actions in a battle
                between two characters.

                All actions are sentences for use in a game story, which is purely fictional and for entertainment purposes only.

                The characters are:
                - {battleManager.player.characterName} (HP: {battleManager.player.currentHP} / {battleManager.player.maxHP})
                - {enemy.characterName} (HP: {enemy.currentHP} / {enemy.maxHP})

                {enemy.characterName} is attacking {battleManager.player.characterName}.

                {battleManager.player.characterName} description:
                {battleManager.player.description}

                {enemy.characterName} description:
                {enemy.description}

                {battleManager.player.characterName} items in inventory:
                {battleManager.player.inventoryItems}

                Battle history:
                {history}

                Proposed action of {enemy.characterName}:
                {proposedAction}

                You should determine what happens next. Take into account the battle history as
                actions should have different effects depending on the history.

                All actions are sentences for use in the game, which is a fiction. If it's too violent, you can edit the sentence to make it less violent for appropriateness 
                but you must output in JSON format.

                Also take into account the current HP and description of both characters.

                Also pay attention to the items of {battleManager.player.characterName} as the damage of {enemy.characterName}
                can be influenced by the items of {battleManager.player.characterName}.

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
                }}");

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

    public static void CheckAndActivateItems(BattleManager battleManager, string userMessage)
    {
        // Convert user message to lowercase for case-insensitive matching
        string lowerMessage = userMessage.ToLower();

        // Reset all items to inactive first
        foreach (var item in battleManager.player.inventoryItems)
        {
            item.isActive = false;
        }

        // Clear the active items list
        battleManager.player.activeItem.Clear();

        // Check each item's keywords against the user message
        foreach (var item in battleManager.player.inventoryItems)
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
                    break; // Found a matching keyword, no need to check others for this item
                }
            }

            if (!keywordFound)
            {
                Debug.Log($"Item '{item.itemName}' remains inactive - no keywords matched");
            }
        }

        Debug.Log($"Total active items: {battleManager.player.activeItem.Count}");
    }

    public static void UpdateItemActiveStatus(BattleManager battleManager, string jsonString)
    {
        // Clear the active items list first
        battleManager.player.activeItem.Clear();

        var itemList = JsonUtility.FromJson<PromptBuilder.ItemActivationList>(jsonString);

        foreach (var activation in itemList.items)
        {
            var item = battleManager.player.inventoryItems.FirstOrDefault(i => i.itemName == activation.name);
            if (item != null)
            {
                item.isActive = activation.active;
                Debug.Log($"Item '{item.itemName}' active status set to: {activation.active}");

                // Add to active items list if it's active
                if (activation.active)
                {
                    battleManager.player.activeItem.Add(item);
                }
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
