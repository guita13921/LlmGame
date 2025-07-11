using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using System.Linq;

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
        string safeMessage = PromptBuilder.SanitizeUserMessage(userMessage);

        // Get target enemy and send final prompt
        Enemy targetEnemy = battleManager.enemies.FirstOrDefault(e => e.IsAlive());

        if (targetEnemy == null)
        {
            Debug.LogError("No valid enemy found!");
            return;
        }

        if (string.IsNullOrEmpty(safeMessage.Trim()))
        {
            Debug.LogWarning("Empty message, skipping API call");
            return;
        }

        // Save user message in BattleManager first
        battleManager.SetUserMessage(safeMessage);

        // Check for item keyword matches and activate items
        PromptBuilder.CheckAndActivateItems(battleManager, safeMessage, targetEnemy);


        string finalPrompt = PromptBuilder.BuildPlayerPrompt(battleManager, targetEnemy, safeMessage);
        StartCoroutine(SendMessageToAI(finalPrompt));
    }


    IEnumerator SendMessageToAI(string userMessage)
    {
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

        string prompt = PromptBuilder.BuildPlayerPrompt(battleManager, targetEnemy, userMessage);

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

                battleManager.combatHandler.PlayerAttack(feasibilityValue, potentialValue, effectValue, effectDesc);
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
        string prompt = PromptBuilder.BuildEnemyPrompt(battleManager, enemy, target, proposedAction);

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

                battleManager.combatHandler.ResolveEnemyAttack(enemy, target, feasibilityValue, potentialValue, effectValue, effectDesc);
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

    # region Class

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
    #endregion
}
