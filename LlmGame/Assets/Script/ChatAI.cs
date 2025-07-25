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

        battleManager.SetUserMessage(safeMessage);

        PromptBuilder.CheckAndActivateItems(battleManager, safeMessage, targetEnemy);

        List<DamageType> enemyDamageTypes = new List<DamageType>();

        foreach (var weapon in targetEnemy.activeItem.OfType<Weapon>())
        {
            enemyDamageTypes.AddRange(weapon.damageType);
        }

        battleManager.CheckAndActivateDefensiveItems(battleManager.player, targetEnemy);

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

        float finalFeasibility = 0f;
        float finalPotential = 0f;
        string finalFeasibilityDesc = "";
        string finalPotentialDesc = "";
        string finalEffect = "";
        string finalEffectDesc = "";

        // === FIRST STAGE: INITIAL AI RESPONSE (via BuildPlayerPrompt) ===
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

                finalFeasibility = currentFeasibility;
                finalPotential = currentPotential;
                finalFeasibilityDesc = baseRoot.properties.feasibility?.description ?? "No description";
                finalPotentialDesc = baseRoot.properties.potential_damage?.description ?? "No description";
                finalEffect = baseRoot.properties.effect_description?.value ?? "No effect";
                finalEffectDesc = baseRoot.properties.effect_description?.description ?? "No description";

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

        // === SECOND STAGE: REFINEMENT PROMPT (anatomy-aware prompt) ===

        string refinementPrompt = PromptBuilder.BuildRefinementPrompt(
            battleManager,
            battleManager.player,
            targetEnemy,
            finalFeasibility,
            finalFeasibilityDesc,
            finalPotential,
            finalPotentialDesc,
            finalEffect,
            finalEffectDesc
        );

        string refinementJson = "{\"message\":\"" + EscapeJsonString(refinementPrompt) + "\"}";

        var refinementRequest = new UnityWebRequest(apiUrl, "POST");
        byte[] refinementRaw = Encoding.UTF8.GetBytes(refinementJson);
        refinementRequest.uploadHandler = new UploadHandlerRaw(refinementRaw);
        refinementRequest.downloadHandler = new DownloadHandlerBuffer();
        refinementRequest.SetRequestHeader("Content-Type", "application/json");

        yield return refinementRequest.SendWebRequest();

        if (refinementRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[Refinement] HTTP Error: {refinementRequest.responseCode} - {refinementRequest.error}");
            yield break;
        }

        string refinementResponseText = refinementRequest.downloadHandler.text;
        Debug.Log("<color=orange>[Refinement Response]</color>\n" + refinementResponseText);

        try
        {
            var refinementRes = JsonUtility.FromJson<ResponseWrapper>(refinementResponseText);
            string cleanedRefinementJson = refinementRes.response.Replace("```json", "").Replace("```", "").Trim();

            if (cleanedRefinementJson.StartsWith("{\\\""))
            {
                cleanedRefinementJson = cleanedRefinementJson.Trim('"').Replace("\\\"", "\"");
            }

            RootProperties refinedRoot = JsonUtility.FromJson<RootProperties>(cleanedRefinementJson);

            finalFeasibility = refinedRoot.properties.feasibility?.value ?? finalFeasibility;
            finalFeasibilityDesc = refinedRoot.properties.feasibility?.description ?? finalFeasibilityDesc;

            finalPotential = refinedRoot.properties.potential_damage?.value ?? finalPotential;
            finalPotentialDesc = refinedRoot.properties.potential_damage?.description ?? finalPotentialDesc;

            finalEffect = refinedRoot.properties.effect_description?.value ?? finalEffect;
            finalEffectDesc = refinedRoot.properties.effect_description?.description ?? finalEffectDesc;
        }
        catch (System.Exception e)
        {
            Debug.LogError("[Refinement] Error parsing refinement response: " + e.Message);
            yield break;
        }

        // === FINAL OUTPUT ===

        responseText.text = $"Feasibility: {finalFeasibility} ({finalFeasibilityDesc})\n" +
                            $"Potential: {finalPotential} ({finalPotentialDesc})\n" +
                            $"Effect: {finalEffect} ({finalEffectDesc})";

        Debug.Log("<color=white>[Final Refined Result]</color>:\n" + responseText.text);

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
                    finalFeasibility,
                    finalPotential,
                    finalEffect,
                    finalEffectDesc,
                    targetEnemy
                )
            );
        }
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

        float baseFeasibility = 0f;
        float basePotential = 0f;
        string baseFeasibilityDesc = "";
        string basePotentialDesc = "";
        string baseEffect = "";
        string baseEffectDesc = "";

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

        // === STEP 2: REFINEMENT PROMPT ===
        string refinementPrompt = PromptBuilder.BuildRefinementPrompt(
            battleManager,
            enemy,
            target,
            baseFeasibility,
            baseFeasibilityDesc,
            basePotential,
            basePotentialDesc,
            baseEffect,
            baseEffectDesc
        );

        string refinementJson = "{\"message\":\"" + EscapeJsonString(refinementPrompt) + "\"}";

        var refinementRequest = new UnityWebRequest(apiUrl, "POST");
        byte[] refinementRaw = Encoding.UTF8.GetBytes(refinementJson);
        refinementRequest.uploadHandler = new UploadHandlerRaw(refinementRaw);
        refinementRequest.downloadHandler = new DownloadHandlerBuffer();
        refinementRequest.SetRequestHeader("Content-Type", "application/json");

        yield return refinementRequest.SendWebRequest();

        if (refinementRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[SendEnemyMessage - Refinement] HTTP Error: {refinementRequest.responseCode} - {refinementRequest.error}");
            yield break;
        }

        string refinementResponseText = refinementRequest.downloadHandler.text;
        Debug.Log("<color=orange>[SendEnemyMessage - Refinement Response]</color>\n" + refinementResponseText);

        try
        {
            var refinementRes = JsonUtility.FromJson<ResponseWrapper>(refinementResponseText);
            string cleanedJson = refinementRes.response.Replace("```json", "").Replace("```", "").Trim();

            if (cleanedJson.StartsWith("{\\\""))
            {
                cleanedJson = cleanedJson.Trim('"').Replace("\\\"", "\"");
            }

            var refinedRoot = JsonUtility.FromJson<RootProperties>(cleanedJson);

            // Preserve values, but use refined descriptions
            float finalFeasibility = baseFeasibility;
            string finalFeasibilityDesc = refinedRoot.properties.feasibility?.description ?? baseFeasibilityDesc;

            float finalPotential = basePotential;
            string finalPotentialDesc = refinedRoot.properties.potential_damage?.description ?? basePotentialDesc;

            string finalEffect = baseEffect;
            string finalEffectDesc = refinedRoot.properties.effect_description?.description ?? baseEffectDesc;

            // === STEP 3: Resolve Enemy Attack ===
            battleManager.StartCoroutine(
                battleManager.combatHandler.ResolveEnemyAttack(
                    enemy,
                    target,
                    enemy.selectedAction,
                    finalFeasibility,
                    finalPotential,
                    finalEffect,
                    finalEffectDesc
                )
            );
        }
        catch (System.Exception e)
        {
            Debug.LogError("[SendEnemyMessage - Refinement] Error parsing refined response: " + e.Message);
            yield break;
        }
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