using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Text;

public class ChatAI : MonoBehaviour
{
    public BattleManager battleManager;
    public TMP_InputField inputField;
    public TMP_Text responseText;
    public GameObject inputPanel;

    public float baseFeasibility = 0f;
    public float basePotential = 0f;
    public string baseFeasibilityDesc = "";
    public string basePotentialDesc = "";
    public string baseEffect = "";
    public string baseEffectDesc = "";

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

        Character targetEnemy = battleManager.selectedTarget;
        Character player = battleManager.player; // You need this reference

        if (targetEnemy == null || !targetEnemy.IsAlive())
        {
            Debug.LogError("No valid enemy selected!");
            return;
        }

        if (string.IsNullOrEmpty(safeMessage.Trim()))
        {
            Debug.LogWarning("Empty message, skipping API call");
            return;
        }

        int messageCost = userMessage.Length;

        if (messageCost > player.focus)
        {
            Debug.LogWarning($"Message too complex! Cost ({messageCost}) exceeds your Focus ({player.focus}).");
            return;
        }

        battleManager.SetUserMessage(safeMessage);

        PromptBuilder.CheckAndActivateItems(battleManager, userMessage, targetEnemy);

        List<DamageType> enemyDamageTypes = new List<DamageType>();

        foreach (var weapon in targetEnemy.activeItem.OfType<Weapon>())
        {
            enemyDamageTypes.AddRange(weapon.damageType);
        }

        string finalPrompt = PromptBuilder.BuildPlayerPrompt(battleManager, targetEnemy, safeMessage);
        StartCoroutine(SendMessageToAI(finalPrompt));
    }


    public IEnumerator SendMessageToAI(string userMessage)
    {
        Character targetEnemy = battleManager.selectedTarget;

        if (targetEnemy == null || !targetEnemy.IsAlive())
        {
            Debug.LogError("No valid enemy selected!");
            yield break;
        }

        string json = "{\"message\":\"" + EscapeJsonString(userMessage) + "\"}";
        Debug.Log("<color=yellow>[SendMessageToAI] Initial JSON Prompt:</color>\n" + userMessage);

        int maxAttempts = 10;
        int attempts = 0;
        bool validResponseReceived = false;
        RootProperties baseRoot = null;

        while (attempts < maxAttempts && !validResponseReceived)
        {
            attempts++;

            var request = new UnityWebRequest(apiUrl, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"HTTP Error: {request.responseCode} - {request.error}");
                Debug.LogError("Response: " + request.downloadHandler.text);
                responseText.text = $"HTTP Error: {request.responseCode} - {request.error}";
                yield break;
            }

            string resText = request.downloadHandler.text;
            Debug.Log($"<color=green>[SendMessageToAI - Attempt {attempts}] Raw Response:</color>\n{resText}");

            try
            {
                var res = JsonUtility.FromJson<ResponseWrapper>(resText);
                string jsonString = res.response.Replace("```json", "").Replace("```", "").Trim();

                if (jsonString.StartsWith("{\\\""))
                {
                    jsonString = jsonString.Trim('"').Replace("\\\"", "\"");
                }

                baseRoot = JsonUtility.FromJson<RootProperties>(jsonString);

                float currentFeasibility = baseRoot.properties.feasibility?.value ?? 0f;
                float currentPotential = baseRoot.properties.potential_damage?.value ?? 0f;

                if (Mathf.Approximately(currentFeasibility, 3f) && Mathf.Approximately(currentPotential, 4f))
                {
                    Debug.LogWarning($"[SendMessageToAI] Default values detected (feasibility: {currentFeasibility}, potential: {currentPotential}) — retrying...");
                    continue;
                }

                baseFeasibility = currentFeasibility;
                basePotential = currentPotential;
                baseFeasibilityDesc = baseRoot.properties.feasibility?.description ?? "No description";
                basePotentialDesc = baseRoot.properties.potential_damage?.description ?? "No description";
                baseEffect = baseRoot.properties.effect_description?.value ?? "No effect";
                baseEffectDesc = baseRoot.properties.effect_description?.description ?? "No description";

                validResponseReceived = true;

            }
            catch (System.Exception e)
            {
                Debug.LogError("Error parsing response: " + e.Message);
                responseText.text = "Error parsing response: " + e.Message;
                yield break;
            }
        }

        if (!validResponseReceived)
        {
            Debug.LogError("[SendMessageToAI] Failed to receive valid response after multiple attempts.");
            responseText.text = "AI did not respond with a valid action. Try again.";
            yield break;
        }

        // === COMBAT APPLICATION ===
        if (battleManager.player.isUsingConsumeTurnItem)
        {
            Debug.Log("Using consume-turn item — skipping direct attack.");
        }
        else
        {
            battleManager.player.selectedAction = battleManager.player.availableActions[0];

            battleManager.StartCoroutine(
                battleManager.combatHandler.PlayerAttack(
                    battleManager.player.selectedAction,
                    baseFeasibility,
                    basePotential,
                    baseEffect,
                    baseEffectDesc,
                    targetEnemy
                )
            );
        }

        // === FINAL OUTPUT (After Damage Calculation) ===
        responseText.text = $"Feasibility: {baseFeasibility} ({baseFeasibilityDesc})\n" +
                            $"Potential: {basePotential} ({basePotentialDesc})\n" +
                            $"Effect: {baseEffect}";

        Debug.Log("<color=white>[Final AI Result]</color>:\n" + responseText.text);

    }

    public IEnumerator SendEnemyMessage(Character enemy, Character target, string proposedAction)
    {
        // === STEP 1: Initial AI Prompt (Enemy Intent) ===
        string prompt = PromptBuilder.BuildEnemyPrompt(battleManager, enemy, target, proposedAction);
        string json = "{\"message\":\"" + EscapeJsonString(prompt) + "\"}";
        Debug.Log("<color=yellow>[SendEnemyMessage] Initial Prompt:</color>\n" + prompt);

        int maxAttempts = 10;
        int attempts = 0;
        bool validResponseReceived = false;
        RootProperties root = null;

        while (attempts < maxAttempts && !validResponseReceived)
        {
            attempts++;

            var request = new UnityWebRequest(apiUrl, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[SendEnemyMessage] HTTP Error: {request.responseCode} - {request.error}");
                Debug.LogError("Response: " + request.downloadHandler.text);
                yield break;
            }

            string resText = request.downloadHandler.text;
            Debug.Log($"<color=cyan>[SendEnemyMessage - Attempt {attempts}] Raw Response:</color>\n{resText}");

            try
            {
                var res = JsonUtility.FromJson<ResponseWrapper>(resText);
                string jsonString = res.response.Replace("```json", "").Replace("```", "").Trim();

                if (jsonString.StartsWith("{\\\""))
                {
                    jsonString = jsonString.Trim('"').Replace("\\\"", "\"");
                }

                root = JsonUtility.FromJson<RootProperties>(jsonString);

                baseFeasibility = root.properties.feasibility?.value ?? 0f;
                baseFeasibilityDesc = root.properties.feasibility?.description ?? "No description";

                basePotential = root.properties.potential_damage?.value ?? 0f;
                basePotentialDesc = root.properties.potential_damage?.description ?? "No description";

                baseEffect = root.properties.effect_description?.value ?? "No effect";
                baseEffectDesc = root.properties.effect_description?.description ?? "No description";

                if (Mathf.Approximately(baseFeasibility, 3f) && Mathf.Approximately(basePotential, 4f))
                {
                    Debug.LogWarning("[SendEnemyMessage] Default values detected, retrying...");
                    continue;
                }

                validResponseReceived = true;

            }
            catch (System.Exception e)
            {
                Debug.LogError("Error parsing enemy AI response: " + e.Message);
                yield break;
            }
        }

        if (!validResponseReceived)
        {
            Debug.LogError("[SendEnemyMessage] Failed to get valid response after max attempts.");
            yield break;
        }

        // === STEP 2: Apply Enemy Attack Using Initial Response ===
        battleManager.StartCoroutine(
            battleManager.combatHandler.ResolveEnemyAttack(
                enemy,
                target,
                enemy.selectedAction,
                baseFeasibility,
                basePotential,
                baseEffect,
                baseEffectDesc
            )
        );

        // === FINAL OUTPUT (After Damage Calculation) ===
        responseText.text = $"Feasibility: {baseFeasibility} ({baseFeasibilityDesc})\n" +
                            $"Potential: {basePotential} ({basePotentialDesc})\n" +
                            $"Effect: {baseEffect}";

        Debug.Log("<color=white>[Final AI Result]</color>:\n" + responseText.text);

    }


    public string EscapeJsonString(string str)
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