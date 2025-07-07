using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;

public class ChatAI : MonoBehaviour
{
    public TMP_InputField inputField;
    public TMP_Text responseText;
    private string apiUrl = "https://diphtxovye.execute-api.ap-northeast-1.amazonaws.com/chatWithAI";

    public void OnSendButtonClick()
    {
        string userMessage = inputField.text;
        //StartCoroutine(SendMessageToAI(userMessage));
    }

    IEnumerator SendMessageToAI(string playerMessage, string context)
    {
        var payload = new
        {
            message = playerMessage,
            context = context // ถ้าจะส่ง context รวม story เพิ่ม
        };

        string json = JsonUtility.ToJson(payload);
        var request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Raw Response: " + request.downloadHandler.text);
            var root = JsonUtility.FromJson<NPCDecisionResponse>(request.downloadHandler.text);

            // ทำงานกับผลลัพธ์
            Debug.Log($"Outcome: {root.outcome}");
            Debug.Log($"NPC Reply: {root.npc_reply}");

            // อัปเดต UI หรือ state ได้ตรงนี้
        }
        else
        {
            Debug.LogError("Error: " + request.error);
        }
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
        [SerializeField] public Feasibility feasibility;
        [SerializeField] public PotentialDamage potential_damage;
        [SerializeField] public EffectDescription effect_description;
    }

    [System.Serializable]
    public class Feasibility
    {
        [SerializeField] public string description;
        [SerializeField] public float value;
    }

    [System.Serializable]
    public class PotentialDamage
    {
        [SerializeField] public string description;
        [SerializeField] public float value;
    }

    [System.Serializable]
    public class EffectDescription
    {
        [SerializeField] public string description;
        [SerializeField] public string value;
    }
}
