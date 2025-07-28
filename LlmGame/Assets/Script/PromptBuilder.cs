using UnityEngine;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Unity.VisualScripting;

public static class PromptBuilder
{
    public static string BuildPlayerPrompt(BattleManager battleManager, Character targetEnemy, string userMessage)
    {
        string history = GetBattleHistory(battleManager);

        // Detect selected parts in enemy based on user's action
        if (battleManager.player.isUsingUltimateSkill == false) PromptBuilder.DetectSelectedBodyParts(userMessage, targetEnemy, battleManager);

        // Format active items
        string PlayerActiveItemsText = FormatActiveItems(battleManager.player.activeItem);
        string EnemyActiveItemsText = FormatActiveItems(targetEnemy.activeItem);

        StringBuilder sb = new StringBuilder();
        sb.Append($@"
            You are a video game AI that determines the effect of proposed actions in a battle
            between two characters.

            Characters:
            - {battleManager.player.characterName} (HP: {battleManager.player.currentHP} / {battleManager.player.maxHP})
            - {targetEnemy.characterName} (HP: {targetEnemy.currentHP} / {targetEnemy.maxHP})

            {battleManager.player.characterName} is engaging {targetEnemy.characterName} in a fantasy battle.

            Player description: {battleManager.player.characterType} - {battleManager.player.description}
            Enemy description: {targetEnemy.characterType} - {targetEnemy.description}

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

            If the Proposed action contains {{Skill}}, note that it is a character ability that can be performed.

            Especially pay attention to the items of {battleManager.player.characterName} and {targetEnemy.characterName}.
            - They should only use items that are active and present in their inventory.
            - Usage of inactive or non-inventory items is infeasible.

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

        // Detect selected parts in player based on enemy's action
        PromptBuilder.DetectSelectedBodyParts(proposedAction, target, battleManager);

        // Format active items for enemy
        string PlayerActiveItemsText = FormatActiveItems(battleManager.player.activeItem);
        string EnemyActiveItemsText = FormatActiveItems(enemy.activeItem);

        StringBuilder sb = new StringBuilder();
        sb.Append($@"
        You are a video game AI that determines the effect of proposed actions in a battle
        between two characters.

        Characters:
        - {enemy.characterName} (HP: {enemy.currentHP} / {enemy.maxHP})
        - {target.characterName} (HP: {target.currentHP} / {target.maxHP})

        {enemy.characterName} is engaging {target.characterName} in a fantasy battle.

        Player description: {target.characterType} - {target.description}
        Enemy description: {enemy.characterType} - {enemy.description}

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

        If the Proposed action contains {{Skill}}, note that it is a character ability that can be performed.

        Especially pay attention to the items of {enemy.characterName} and {target.characterName}
        - They should only use items that are active and present in their inventory.
        - Usage of inactive or non-inventory items is infeasible.

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

        sanitized = char.ToUpper(sanitized[0]) + sanitized.Substring(1);

        return sanitized;
    }

    #region Format Function

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
            itemsText.AppendLine($"- {item.itemName}: {item.itemType}: {item.itemDescription}");
        }

        return itemsText.ToString();
    }

    public static string FormatBodyParts(List<BodyPartData> parts)
    {
        if (parts == null || parts.Count == 0) return "No body part data.";

        StringBuilder sb = new StringBuilder();
        foreach (var part in parts)
        {
            sb.AppendLine($"- {part.type} ({part.composition}): {part.state}, HP: {part.health} / {part.maxHealth}, Vital: {part.isVital}");

            if (part.equippedArmor != null)
            {
                sb.AppendLine($"  Equipped Armor: {part.equippedArmor.armorName}");
                sb.AppendLine($"    Description: {part.equippedArmor.description}");
            }
        }
        return sb.ToString();
    }

    public static string FormatWeakPointsFromBodyParts(List<BodyPartData> parts)
    {
        if (parts == null || parts.Count == 0) return "No weak point data.";

        StringBuilder sb = new StringBuilder();
        bool foundExposed = false;

        foreach (var part in parts)
        {
            if (part.linkedWeakPoint != null && part.linkedWeakPoint.isExposed)
            {
                foundExposed = true;
                sb.AppendLine($"- {part.linkedWeakPoint.weakPointName} (Description: {part.linkedWeakPoint.weakPointDescription})");
            }
        }

        return foundExposed ? sb.ToString() : "No exposed weak points.";
    }

    public static void CheckAndActivateItems(BattleManager battleManager, string userMessage, Character targetEnemy)
    {
        string lowerMessage = userMessage.ToLower();

        // Reset all items to inactive
        foreach (var item in battleManager.player.inventoryItems)
        {
            item.isActive = false;
        }

        foreach (var item in targetEnemy.inventoryItems)
        {
            item.isActive = false;
        }

        // Clear active items list
        battleManager.player.activeItem.Clear();

        // Prepare items to check: equipped weapons + Sub_Weapons in inventory
        List<Item> itemsToCheck = new List<Item>();

        if (battleManager.player.leftHandWeapon != null)
            itemsToCheck.Add(battleManager.player.leftHandWeapon);

        if (battleManager.player.rightHandWeapon != null)
            itemsToCheck.Add(battleManager.player.rightHandWeapon);

        foreach (var item in battleManager.player.inventoryItems)
        {
            if (item is Weapon weapon && weapon.itemType == ItemType.Sub_Weapon)
            {
                itemsToCheck.Add(weapon);
            }
        }

        // 🔥 Activate all matching items
        foreach (var item in itemsToCheck.ToList())
        {
            bool keywordFound = false;

            foreach (string keyword in item.keyWords)
            {
                if (!string.IsNullOrEmpty(keyword) && lowerMessage.Contains(keyword.ToLower()))
                {
                    item.isActive = true;
                    keywordFound = true;

                    battleManager.player.activeItem.Add(item);
                    Debug.Log($"Item '{item.itemName}' activated by keyword: '{keyword}'");

                    // Handle OneTime items
                    if (item.usageType == UsageType.OneTime)
                    {
                        item.remain--;

                        if (item.remain <= 0)
                        {
                            Debug.Log($"Item '{item.itemName}' used up and removed (OneTime)");
                            battleManager.player.inventoryItems.Remove(item);
                        }
                    }

                    break; // Found a match, stop checking this item's keywords
                }
            }

            if (!keywordFound)
            {
                Debug.Log($"Item '{item.itemName}' remains inactive - no keywords matched");
            }
        }

        Debug.Log($"Total active items: {battleManager.player.activeItem.Count}");
    }

    public static void DetectSelectedBodyParts(string message, Character target, BattleManager battleManager)
    {
        battleManager.selectedParts.Clear();
        string lowerMessage = message.ToLower();

        foreach (var part in target.bodyParts)
        {
            if (part == null || part.keyword == null) continue;

            // Check each keyword defined in the ScriptableObject
            foreach (var keyword in part.keyword)
            {
                if (!string.IsNullOrEmpty(keyword) && lowerMessage.Contains(keyword.ToLower()))
                {
                    battleManager.selectedParts.Add(part);
                    Debug.Log($"🧠 [Selection] Body part matched: {part.type} via keyword '{keyword}'");
                    break; // Avoid adding the same part multiple times
                }
            }
        }

        // Fallback: select default body part (e.g., Torso)
        if (battleManager.selectedParts.Count == 0 && target.bodyParts != null && target.bodyParts.Count > 0)
        {
            var defaultPart = target.bodyParts.FirstOrDefault(p => p != null && p.type == BodyPartType.Torso);

            if (defaultPart != null)
            {
                battleManager.selectedParts.Add(defaultPart);
                Debug.Log($"🎯 [Fallback] No part matched — defaulted to: {defaultPart.type}");
            }
            else
            {
                // Still fallback to first valid part if Torso doesn't exist
                var fallbackPart = target.bodyParts.FirstOrDefault(p => p != null);
                if (fallbackPart != null)
                {
                    battleManager.selectedParts.Add(fallbackPart);
                    Debug.Log($"🎯 [Fallback] No part matched — defaulted to first available: {fallbackPart.type}");
                }
                else
                {
                    Debug.LogWarning("⚠️ [Fallback] No valid body parts found.");
                }
            }
        }

        Debug.Log($"✅ Total Selected Body Parts: {battleManager.selectedParts.Count}");
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


    #region Refine

    public static string BuildRefinementPrompt(
        BattleManager battleManager,
        Character attacker,
        Character target,
        float baseFeasibility,
        string baseFeasibilityDesc,
        float basePotentialDamage,
        string basePotentialDamageDesc,
        string baseEffectValue,
        string baseEffectDesc)
    {
        string attackerParts = FormatBodyParts(attacker.bodyParts);
        string attackerWeakPoints = FormatWeakPointsFromBodyParts(attacker.bodyParts);

        string targetParts = FormatBodyParts(target.bodyParts);
        string targetWeakPoints = FormatWeakPointsFromBodyParts(target.bodyParts);

        StringBuilder sb = new StringBuilder();
        sb.Append($@"You are a combat analysis AI that refines battle outcome **descriptions** based on detailed character anatomy and current conditions.

        CURRENT BATTLE STATE:
        Attacker: {attacker.characterName}
        {attackerParts}
        {attackerWeakPoints}

        Target: {target.characterName}
        {targetParts}
        {targetWeakPoints}

        INITIAL ASSESSMENT TO REFINE:
        - Feasibility: {baseFeasibility} → {baseFeasibilityDesc}
        - Potential Damage: {basePotentialDamage} → {basePotentialDamageDesc}
        - Effect: {baseEffectValue} → {baseEffectDesc}

        IMPORTANT: 
        - DO NOT CHANGE the 'value' fields of feasibility or potential_damage.
        - ONLY UPDATE the 'description' fields based on anatomy, injuries, and weak points.
        - The 'effect_description' field can have both value and description adjusted if necessary.

        Output in this exact JSON format:
        {{
            ""properties"": {{
                ""feasibility"": {{
                    ""maximum"": 10.0,
                    ""minimum"": 0.0,
                    ""value"": {baseFeasibility},
                    ""description"": ""updated feasibility description here""
                }},
                ""potential_damage"": {{
                    ""maximum"": 10.0,
                    ""minimum"": 0.0,
                    ""value"": {basePotentialDamage},
                    ""description"": ""updated potential damage description here""
                }},
                ""effect_description"": {{
                    ""value"": ""{baseEffectValue}"",
                    ""description"": ""updated effect description here""
                }}
            }}
        }}");

        string prompt = sb.ToString();

        // Debug log
        Debug.Log("<color=cyan>[PromptBuilder] Refinement Prompt:</color>\n" + prompt);

        return prompt;
    }


    #endregion


}
