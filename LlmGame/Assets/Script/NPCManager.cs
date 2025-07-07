using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;

public class NPCManager : MonoBehaviour
{
    public DialogueUI dialogueUI;
    public StoryState currentState = StoryState.Introduction;

    private string apiUrl = " https://g1i8b86ls3.execute-api.ap-northeast-1.amazonaws.com/prod/chatWithNPC";


    public void SendPlayerMessage(string playerMessage)
    {
        string prompt = $@"
            You are Anna, a scientist hiding from zombies. You must guide the player to reach the safe house.

            No matter what the player says, always keep the conversation focused on survival and moving toward the safe house.
            Speak naturally and avoid revealing unnecessary personal details.

            Current Story State: {currentState}
            Player message: ""{playerMessage}""
            ";

        StartCoroutine(SendToAI(prompt));
    }

    IEnumerator SendToAI(string prompt)
    {
        dialogueUI.SetNPCText("Thinking...");

        var jsonData = JsonUtility.ToJson(new MessageData { message = prompt });

        var request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error: " + request.error);
            dialogueUI.SetNPCText("Error talking to AI...");
        }
        else
        {
            var jsonResponse = request.downloadHandler.text;
            var wrapped = JsonUtility.FromJson<AIResponseWrapper>("{\"response\":" + jsonResponse + "}");
            dialogueUI.SetNPCText(wrapped.response);
        }
    }

    [System.Serializable]
    public class MessageData
    {
        public string message;
    }

    [System.Serializable]
    public class AIResponseWrapper
    {
        public string response;
    }
}

public enum StoryState
{
    Introduction,
    Explore,
    Danger,
    Escape,
    SafeHouse,
    Ending
}
