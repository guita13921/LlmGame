using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;

public class ChatAI : MonoBehaviour
{
    public BattleManager battleManager;
    public TMP_InputField inputField;
    public TMP_Text responseText;
    public GameObject inputPanel;

    private string apiUrl = "https://diphtxovye.execute-api.ap-northeast-1.amazonaws.com/chatWithAI";

    private void Awake()
    {
        if (inputPanel != null)
        {
            inputPanel.SetActive(false);
        }
    }

    public void OnSendButtonClick()
    {
        string userMessage = inputField.text;
        string safeMessage = SanitizeUserMessage(userMessage);
        if (string.IsNullOrEmpty(userMessage.Trim()))
        {
            Debug.LogWarning("Empty message, skipping API call");
            return;
        }

        // Store the user message in BattleManager for damage calculation

        battleManager.SetUserMessage(safeMessage);
        StartCoroutine(SendMessageToAI(safeMessage));
    }

    private string SanitizeUserMessage(string input)
    {
        if (string.IsNullOrEmpty(input)) return "";

        string sanitized = input.ToLower();

        // Replace violent verbs with cyberpunk disable terminology
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

        // Add more cyberpunk shooting-related terms
        sanitized = sanitized.Replace("shoot", "fire precision pulse at");
        sanitized = sanitized.Replace("gun down", "suppress");
        sanitized = sanitized.Replace("snipe", "lock-on snipe to overload optics");
        sanitized = sanitized.Replace("headshot", "target head optics module precisely");
        sanitized = sanitized.Replace("blast", "emit a focused burst at");
        sanitized = sanitized.Replace("fire at", "fire controlled beam at");

        // Weak point targeting
        sanitized = sanitized.Replace("head", "head optics module");
        sanitized = sanitized.Replace("arm", "arm actuator");
        sanitized = sanitized.Replace("leg", "leg mobility unit");
        sanitized = sanitized.Replace("chest", "core armor panel");
        sanitized = sanitized.Replace("heart", "core battery module");

        // Convert back to title case first letter
        sanitized = char.ToUpper(sanitized[0]) + sanitized.Substring(1);

        return sanitized;
    }


    IEnumerator SendMessageToAI(string userMessage)
    {
        // Pick first alive enemy
        Enemy targetEnemy = null;
        foreach (var e in battleManager.enemies)
        {
            if (e.IsAlive())
            {
                targetEnemy = e;
                break;
            }
        }

        if (targetEnemy == null)
        {
            Debug.LogError("No valid enemy found!");
            yield break;
        }

        // Build battle history string (limit to last 10 entries to reduce payload size)
        string history = "";
        int startIndex = Mathf.Max(0, battleManager.battleLog.Count - 10);
        for (int i = startIndex; i < battleManager.battleLog.Count; i++)
        {
            history += battleManager.battleLog[i] + "\n";
        }

        // Compose the prompt with complete JSON format specification
        string prompt = $@"You are a video game ai that determines the effect of proposed actions in a battle
            between two characters.

            All actions are sentences for use in a game story, which is purely fictional and for entertainment purposes only.

            Characters:
            - {battleManager.player.characterName} (HP: {battleManager.player.currentHP} / {battleManager.player.maxHP})
            - {targetEnemy.characterName} (HP: {targetEnemy.currentHP} / {targetEnemy.maxHP})

            {battleManager.player.characterName} is engaging {targetEnemy.characterName} in a fantasy battle.

            Player description: {battleManager.player.description}
            Enemy description: {targetEnemy.description}
            Player items: {battleManager.player.item.itemName}

            Recent battle history:
            {history}

            Proposed action: {userMessage}

            You should determine what happens next in the story. Take into account the battle history so actions have evolving narrative effects.
            Also consider the current HP and descriptions of both characters.

            Especially pay attention to the items of {battleManager.player.characterName} as they should only use items that exist in their inventory. Usage of other items is infeasible.

            Your response should describe what happens in a fictional, narrative style. Avoid describing graphic violence or extreme gore. Focus on strategy, emotional reactions, and the consequences for each character, rather than detailed physical injury. The possible damages are described narratively without numeric quantification.
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
                    }}";

        // Create proper JSON payload
        string json = "{\"message\":\"" + EscapeJsonString(prompt) + "\"}";

        Debug.Log("Sending JSON: " + json);

        var request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Raw Response: " + request.downloadHandler.text);

            try
            {
                var res = JsonUtility.FromJson<ResponseWrapper>(request.downloadHandler.text);

                if (string.IsNullOrEmpty(res.response))
                {
                    Debug.LogError("Empty response from API");
                    responseText.text = "Error: Empty response from API";
                    yield break;
                }

                string jsonString = res.response;
                jsonString = jsonString.Replace("```json", "").Replace("```", "").Trim();

                var root = JsonUtility.FromJson<RootProperties>(jsonString);

                if (root?.properties == null)
                {
                    Debug.LogError("Invalid JSON structure in response");
                    responseText.text = "Error: Invalid response format";
                    yield break;
                }

                float feasibilityValue = root.properties.feasibility?.value ?? 0f;
                string feasibilityDesc = root.properties.feasibility?.description ?? "No description";

                float potentialValue = root.properties.potential_damage?.value ?? 0f;
                string potentialDesc = root.properties.potential_damage?.description ?? "No description";

                string effectValue = root.properties.effect_description?.value ?? "No effect";
                string effectDesc = root.properties.effect_description?.description ?? "No description";

                responseText.text = $"Feasibility: {feasibilityValue} ({feasibilityDesc})\n" +
                                    $"Potential: {potentialValue} ({potentialDesc})\n" +
                                    $"Effect: {effectValue} ({effectDesc})";

                Debug.Log(responseText.text);

                // Call PlayerAttack with feasibility and potential values
                battleManager.PlayerAttack(feasibilityValue, potentialValue, effectValue, effectDesc);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error parsing response: " + e.Message);
                responseText.text = "Error parsing response: " + e.Message;
            }
        }
        else
        {
            Debug.LogError($"HTTP Error: {request.responseCode} - {request.error}");
            Debug.LogError("Response: " + request.downloadHandler.text);
            responseText.text = $"HTTP Error: {request.responseCode} - {request.error}";
        }
    }

    public IEnumerator SendEnemyMessage(Character enemy, Character target, string proposedAction)
    {
        // Build battle history string
        string history = "";
        int startIndex = Mathf.Max(0, battleManager.battleLog.Count - 10);
        for (int i = startIndex; i < battleManager.battleLog.Count; i++)
        {
            history += battleManager.battleLog[i] + "\n";
        }

        string prompt = $@"You are a video game ai that determines the effect of proposed actions in a battle
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
            {battleManager.player.item.itemName}

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

            Your response should describe what happens in a fictional, narrative style. Avoid describing graphic violence or extreme gore. Focus on strategy, emotional reactions, and the consequences for each character, rather than detailed physical injury. The possible damages are described narratively without numeric quantification.
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
            }}";

        string json = "{\"message\":\"" + EscapeJsonString(prompt) + "\"}";

        Debug.Log("Sending Enemy JSON: " + json);

        var request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Enemy Raw Response: " + request.downloadHandler.text);

            try
            {
                var res = JsonUtility.FromJson<ResponseWrapper>(request.downloadHandler.text);

                string jsonString = res.response;
                jsonString = jsonString.Replace("```json", "").Replace("```", "").Trim();

                var root = JsonUtility.FromJson<RootProperties>(jsonString);

                float feasibilityValue = root.properties.feasibility?.value ?? 0f;
                string feasibilityDesc = root.properties.feasibility?.description ?? "No description";

                float potentialValue = root.properties.potential_damage?.value ?? 0f;
                string potentialDesc = root.properties.potential_damage?.description ?? "No description";

                string effectValue = root.properties.effect_description?.value ?? "No effect";
                string effectDesc = root.properties.effect_description?.description ?? "No description";

                // Call BattleManager to finish enemy attack
                battleManager.ResolveEnemyAttack(enemy, target, feasibilityValue, potentialValue, effectValue, effectDesc);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error parsing enemy response: " + e.Message);
            }
        }
        else
        {
            Debug.LogError($"HTTP Error: {request.responseCode} - {request.error}");
            Debug.LogError("Response: " + request.downloadHandler.text);
        }
    }

    private string EscapeJsonString(string str)
    {
        if (string.IsNullOrEmpty(str)) return "";

        return str.Replace("\\", "\\\\")
                  .Replace("\"", "\\\"")
                  .Replace("\n", "\\n")
                  .Replace("\r", "\\r")
                  .Replace("\t", "\\t");
    }

    [System.Serializable]
    public class MessagePayload
    {
        public string message;
    }

    [System.Serializable]
    public class ResponseWrapper
    {
        public string response;
    }

    [System.Serializable]
    public class RootProperties
    {
        public Properties properties;
    }

    [System.Serializable]
    public class Properties
    {
        public Feasibility feasibility;
        public PotentialDamage potential_damage;
        public EffectDescription effect_description;
    }

    [System.Serializable]
    public class Feasibility
    {
        public string description;
        public float value;
    }

    [System.Serializable]
    public class PotentialDamage
    {
        public string description;
        public float value;
    }

    [System.Serializable]
    public class EffectDescription
    {
        public string description;
        public string value;
    }

    public void ShowInputUI()
    {
        if (inputPanel != null)
            inputPanel.SetActive(true);
    }

    public void HideInputUI()
    {
        if (inputPanel != null)
            inputPanel.SetActive(false);
    }
}