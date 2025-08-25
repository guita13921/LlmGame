using TMPro;
using UnityEngine;

public class TurnIndicatorUI : MonoBehaviour
{
    public TMP_Text messageText;

    public void ShowTurn(Character character)
    {
        if (messageText != null && character != null)
        {
            messageText.text = $"{character.characterName}'s Turn";
            gameObject.SetActive(true);
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
