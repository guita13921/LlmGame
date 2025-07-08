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
        if (string.IsNullOrEmpty(userMessage.Trim()))
        {
            Debug.LogWarning("Empty message, skipping API call");
            return;
        }
        StartCoroutine(SendMessageToAI(userMessage));
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
        string prompt = $@"You are a video game AI that determines the effect of proposed actions in a battle.

                Characters:
                - {battleManager.player.characterName}
                - {targetEnemy.characterName}

                {battleManager.player.characterName} is attacking {targetEnemy.characterName}.

                Player description: {battleManager.player.description}
                Enemy description: {targetEnemy.description}
                Player items: {battleManager.player.itemDescription}

                Recent battle history:
                {history}

                Proposed action: {userMessage}

                Determine what happens next. Consider battle history, character HP, and available items. Repeated actions from battle history should have reduced damage.

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

                // Call PlayerAttack after successful response
                battleManager.PlayerAttack();
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