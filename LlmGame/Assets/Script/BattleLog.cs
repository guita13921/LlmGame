using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleLog : MonoBehaviour
{
    public TMP_Text responseText; // Assign in inspector
    public ScrollRect scrollRect; // Assign in inspector

    public void Log(string message)
    {
        responseText.text += message + "\n";
        Canvas.ForceUpdateCanvases(); // Force layout rebuild
        //ScrollToTop();
    }

    private void ScrollToBottom()
    {
        // Move the scrollbar to bottom
        scrollRect.verticalNormalizedPosition = 0f;
    }

    private void ScrollToTop()
    {
        // Move the scrollbar to top
        scrollRect.verticalNormalizedPosition = 1f;
    }
}
