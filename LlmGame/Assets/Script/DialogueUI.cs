using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    public TextMeshProUGUI npcNameText;
    public TextMeshProUGUI npcMessageText;
    public TMP_InputField playerInputField;
    public Button sendButton;
    public NPCManager npcManager;

    void Start()
    {
        sendButton.onClick.AddListener(OnSendClicked);
    }

    void OnSendClicked()
    {
        string playerMessage = playerInputField.text;
        if (!string.IsNullOrEmpty(playerMessage))
        {
            npcManager.SendPlayerMessage(playerMessage);
            playerInputField.text = "";
        }
    }

    public void SetNPCText(string message)
    {
        npcMessageText.text = message;
    }
}
