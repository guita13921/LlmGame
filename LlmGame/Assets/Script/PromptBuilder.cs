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
        string activeItemsJson = GetActiveItemsJson(battleManager, userMessage);


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

            Player items active (JSON):
            {activeItemsJson}

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

    public static void ActivateItemFromMessage(Character player, string userMessage)
    {
        if (string.IsNullOrEmpty(userMessage) || player.inventoryItems == null) return;

        // Clear previously active items
        player.activeItem.Clear();

        // Split user message into words (remove punctuation and lowercase)
        string[] words = userMessage.ToLower().Split(new char[] { ' ', ',', '.', '!', '?', ';', ':' }, System.StringSplitOptions.RemoveEmptyEntries);

        foreach (var item in player.inventoryItems)
        {
            if (item == null || item.keyWords == null) continue;

            foreach (var keyword in item.keyWords)
            {
                if (string.IsNullOrEmpty(keyword)) continue;

                if (words.Contains(keyword.ToLower()))
                {
                    if (!player.activeItem.Contains(item))
                    {
                        player.activeItem.Add(item);
                        Debug.Log($"Activated item: {item.itemName} (matched keyword: {keyword})");
                    }
                    break; // Stop checking more keywords for this item
                }
            }
        }
    }

    #region Active Item

    public static string GetActiveItemsJson(BattleManager battleManager, string userMessage)
    {
        var itemsStatus = new List<ItemStatus>();

        foreach (var item in battleManager.player.inventoryItems)
        {
            bool isActive = false;

            if (item.keyWords != null && item.keyWords.Count > 0)
            {
                foreach (var keyword in item.keyWords)
                {
                    if (userMessage.ToLower().Contains(keyword.ToLower()))
                    {
                        isActive = true;
                        break;
                    }
                }
            }

            // Update isActive on the item itself
            item.isActive = isActive;

            // Prepare for JSON
            itemsStatus.Add(new ItemStatus
            {
                name = item.itemName,
                description = item.itemDescription,
                active = isActive
            });
        }

        return JsonUtility.ToJson(new ItemStatusList { items = itemsStatus }, true);
    }

    [System.Serializable]
    public class ItemStatus
    {
        public string name;
        public string description;
        public bool active;
    }

    [System.Serializable]
    public class ItemStatusList
    {
        public List<ItemStatus> items;
    }


    public static string BuildItemKeywordJson(BattleManager battleManager)
    {
        var itemsKeyword = new List<ItemKeyword>();

        foreach (var item in battleManager.player.inventoryItems)
        {
            itemsKeyword.Add(new ItemKeyword
            {
                name = item.itemName,
                description = item.itemDescription,
                keyword = item.keyWords // ✔️ pass keywords here
            });
        }

        return JsonUtility.ToJson(new ItemKeywordList { items = itemsKeyword }, true);
    }


    public static string BuildItemActivationPrompt(string userMessage, string itemsKeywordJson)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append($@"
            You are a video game AI that determines which items from the player's inventory should be active based on the proposed action.

            Player action (proposed sentence or move):
            {userMessage}

            Available items and their usage descriptions (JSON):
            {itemsKeywordJson}

            Important instructions:
            - Only mark an item as 'active' if it is explicitly used in the proposed action to perform an attack or battle effect.
            - Do not guess or assume creative or indirect uses of items unless clearly stated in the action.
            - If an item is only mentioned without clearly using it for an attack or effect, it should remain inactive.
            - If the action is vague, purely performative, or not combat-related (e.g., 'Dance'), then mark all items as inactive.
            - You must use only the items listed in the JSON exactly as given. Never invent or create new items.
            - You should not activate an item based solely on inferred possibilities or indirect references.
            - The focus is on direct, intentional usage of items in combat.

            Output in this exact JSON format and do not include any explanation text, markdown, or code blocks. Only output the JSON object:
            {{
                ""items"": [
                    {{ ""name"": ""ItemName1"", ""active"": true }},
                    {{ ""name"": ""ItemName2"", ""active"": false }}
                ]
            }}
            ");

        return sb.ToString();
    }

    internal static string BuildPlayerPrompt(BattleManager battleManager, object targetEnemy, string userMessage)
    {
        throw new System.NotImplementedException();
    }

    [System.Serializable]
    public class ItemKeyword
    {
        public string name;
        public string description;
        public List<string> keyword; // Add this
    }

    [System.Serializable]
    public class ItemKeywordList
    {
        public List<ItemKeyword> items;
    }

    [System.Serializable]
    public class ItemActivation
    {
        public string name;
        public bool active;
    }

    [System.Serializable]
    public class ItemActivationList
    {
        public List<ItemActivation> items;
    }

    #endregion

}
