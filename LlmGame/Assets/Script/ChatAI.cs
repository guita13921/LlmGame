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

    IEnumerator SendMessageToAI(string userMessage)
    {
        Character targetEnemy = battleManager.selectedTarget;

        if (targetEnemy == null || !targetEnemy.IsAlive())
        {
            Debug.LogError("No valid enemy selected!");
            yield break;
        }

        string prompt = PromptBuilder.BuildPlayerPrompt(battleManager, targetEnemy, userMessage);
        string json = "{\"message\":\"" + EscapeJsonString(prompt) + "\"}";
        Debug.Log("<color=yellow>[SendMessageToAI] Initial JSON Prompt:</color>\n" + prompt);

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
        Debug.Log("<color=green>[SendMessageToAI] Raw Response:</color>\n" + resText);

        ResponseWrapper res = null;
        RootProperties baseRoot = null;

        try
        {
            res = JsonUtility.FromJson<ResponseWrapper>(resText);
            string jsonString = res.response.Replace("```json", "").Replace("```", "").Trim();

            // Unescape if double-encoded
            if (jsonString.StartsWith("{\\\""))
            {
                jsonString = jsonString.Trim('"').Replace("\\\"", "\"");
            }

            baseRoot = JsonUtility.FromJson<RootProperties>(jsonString);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error parsing base response: " + e.Message);
            responseText.text = "Error parsing response: " + e.Message;
            yield break;
        }

        float baseFeasibility = baseRoot.properties.feasibility?.value ?? 0f;
        string baseFeasibilityDesc = baseRoot.properties.feasibility?.description ?? "No description";

        float basePotential = baseRoot.properties.potential_damage?.value ?? 0f;
        string basePotentialDesc = baseRoot.properties.potential_damage?.description ?? "No description";

        string baseEffect = baseRoot.properties.effect_description?.value ?? "No effect";
        string baseEffectDesc = baseRoot.properties.effect_description?.description ?? "No description";

        // 🔁 BUILD REFINEMENT PROMPT
        string refinementPrompt = PromptBuilder.BuildRefinementPrompt(
            battleManager.player,
            targetEnemy,
            baseFeasibility,
            baseFeasibilityDesc,
            basePotential,
            basePotentialDesc,
            baseEffect,
            baseEffectDesc
        );

        string refineJson = "{\"message\":\"" + EscapeJsonString(refinementPrompt) + "\"}";
        Debug.Log("<color=cyan>[SendMessageToAI] Refinement Prompt:</color>\n" + refinementPrompt);

        var refineRequest = new UnityWebRequest(apiUrl, "POST");
        refineRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(refineJson));
        refineRequest.downloadHandler = new DownloadHandlerBuffer();
        refineRequest.SetRequestHeader("Content-Type", "application/json");

        yield return refineRequest.SendWebRequest();

        if (refineRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Refinement HTTP Error: {refineRequest.responseCode} - {refineRequest.error}");
            Debug.LogError("Response: " + refineRequest.downloadHandler.text);
            yield break;
        }

        string refineText = refineRequest.downloadHandler.text;
        Debug.Log("<color=lime>[SendMessageToAI] Refinement Response:</color>\n" + refineText);

        RootProperties finalRoot = null;

        try
        {
            var refineRes = JsonUtility.FromJson<ResponseWrapper>(refineText);
            string refineJsonString = refineRes.response.Replace("```json", "").Replace("```", "").Trim();

            if (refineJsonString.StartsWith("{\\\""))
            {
                refineJsonString = refineJsonString.Trim('"').Replace("\\\"", "\"");
            }

            finalRoot = JsonUtility.FromJson<RootProperties>(refineJsonString);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error parsing refined response: " + e.Message);
            yield break;
        }

        float finalFeasibility = finalRoot.properties.feasibility?.value ?? baseFeasibility;
        string finalFeasibilityDesc = finalRoot.properties.feasibility?.description ?? baseFeasibilityDesc;

        float finalPotential = finalRoot.properties.potential_damage?.value ?? basePotential;
        string finalPotentialDesc = finalRoot.properties.potential_damage?.description ?? basePotentialDesc;

        string finalEffect = finalRoot.properties.effect_description?.value ?? baseEffect;
        string finalEffectDesc = finalRoot.properties.effect_description?.description ?? baseEffectDesc;

        responseText.text = $"Feasibility: {finalFeasibility} ({finalFeasibilityDesc})\n" +
                            $"Potential: {finalPotential} ({finalPotentialDesc})\n" +
                            $"Effect: {finalEffect} ({finalEffectDesc})";

        Debug.Log("<color=white>[Final Player Result]</color>:\n" + responseText.text);

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
        string prompt = PromptBuilder.BuildEnemyPrompt(battleManager, enemy, target, proposedAction);
        string json = "{\"message\":\"" + EscapeJsonString(prompt) + "\"}";

        var request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            var resText = request.downloadHandler.text;
            Debug.Log("Enemy Raw Response (Initial): " + resText);

            ResponseWrapper res = null;
            RootProperties root = null;

            try
            {
                res = JsonUtility.FromJson<ResponseWrapper>(resText);
                string jsonString = res.response.Replace("```json", "").Replace("```", "").Trim();
                root = JsonUtility.FromJson<RootProperties>(jsonString);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error parsing first response: " + e.Message);
                yield break;
            }

            float baseFeasibility = root.properties.feasibility?.value ?? 0f;
            string baseFeasibilityDesc = root.properties.feasibility?.description ?? "No description";

            float basePotential = root.properties.potential_damage?.value ?? 0f;
            string basePotentialDesc = root.properties.potential_damage?.description ?? "No description";

            string baseEffect = root.properties.effect_description?.value ?? "No effect";
            string baseEffectDesc = root.properties.effect_description?.description ?? "No description";

            // 🔁 BUILD SECONDARY PROMPT
            string refinementPrompt = PromptBuilder.BuildRefinementPrompt(
                enemy,
                target,
                baseFeasibility,
                baseFeasibilityDesc,
                basePotential,
                basePotentialDesc,
                baseEffect,
                baseEffectDesc
            );


            string refineJson = "{\"message\":\"" + EscapeJsonString(refinementPrompt) + "\"}";
            var refineRequest = new UnityWebRequest(apiUrl, "POST");
            refineRequest.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(refineJson));
            refineRequest.downloadHandler = new DownloadHandlerBuffer();
            refineRequest.SetRequestHeader("Content-Type", "application/json");

            yield return refineRequest.SendWebRequest();

            if (refineRequest.result == UnityWebRequest.Result.Success)
            {
                var refineText = refineRequest.downloadHandler.text;

                // 📤 Log the raw AI response from refinement
                Debug.Log("<color=lime>[BuildRefinementPrompt] Raw AI Response:</color>\n" + refineText);

                ResponseWrapper refineRes = null;
                RootProperties finalRoot = null;

                try
                {
                    refineRes = JsonUtility.FromJson<ResponseWrapper>(refineText);
                    string refineJsonString = refineRes.response.Replace("```json", "").Replace("```", "").Trim();

                    // 👇 UNWRAP if double-encoded
                    if (refineJsonString.StartsWith("{\\\""))
                    {
                        // Double escaped — remove outer quotes and unescape
                        refineJsonString = refineJsonString.Trim('"');
                        refineJsonString = refineJsonString.Replace("\\\"", "\"");
                    }

                    // ✅ Log cleaned string for verification
                    Debug.Log("<color=lime>[Unwrapped AI JSON]</color>\n" + refineJsonString);

                    // Now parse
                    finalRoot = JsonUtility.FromJson<RootProperties>(refineJsonString);

                }
                catch (System.Exception e)
                {
                    Debug.LogError("Error parsing refined response: " + e.Message);
                    yield break;
                }

                float finalFeasibility = finalRoot.properties.feasibility?.value ?? baseFeasibility;
                string finalFeasibilityDesc = finalRoot.properties.feasibility?.description ?? baseFeasibilityDesc;

                float finalPotential = finalRoot.properties.potential_damage?.value ?? basePotential;
                string finalPotentialDesc = finalRoot.properties.potential_damage?.description ?? basePotentialDesc;

                string finalEffect = finalRoot.properties.effect_description?.value ?? baseEffect;
                string finalEffectDesc = finalRoot.properties.effect_description?.description ?? baseEffectDesc;

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
            else
            {
                Debug.LogError($"Refinement API Error: {refineRequest.responseCode} - {refineRequest.error}");
                Debug.LogError("Refinement Response Text: " + refineRequest.downloadHandler.text);
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