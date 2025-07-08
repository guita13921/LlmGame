using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class BattleLogUI : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject logPanel;
    public ScrollRect scrollRect;
    public Transform logContent;
    public GameObject logEntryPrefab;
    public Button toggleButton;
    public TextMeshProUGUI toggleButtonText;

    [Header("Settings")]
    public int maxLogEntries = 50;
    public float fadeInDuration = 0.3f;
    public bool autoScroll = true;
    public bool showTurnNumbers = true;
    public bool showTimestamps = false;

    [Header("Colors")]
    public Color playerActionColor = new Color(0.2f, 0.8f, 0.2f);
    public Color enemyActionColor = new Color(0.8f, 0.2f, 0.2f);
    public Color systemMessageColor = new Color(0.7f, 0.7f, 0.7f);
    public Color damageColor = new Color(1f, 0.5f, 0f);
    public Color healColor = new Color(0.2f, 1f, 0.2f);
    public Color criticalColor = new Color(1f, 0.2f, 0.8f);

    private List<GameObject> logEntries = new List<GameObject>();
    private bool isLogVisible = true;
    private BattleManager battleManager;
    private int lastLogCount = 0;

    [System.Serializable]
    public class BattleLogEntry
    {
        public string message;
        public LogType type;
        public int turnNumber;
        public string timestamp;
        public Color textColor;
        public bool isImportant;

        public enum LogType
        {
            PlayerAction,
            EnemyAction,
            SystemMessage,
            Damage,
            Heal,
            Critical,
            TurnStart,
            BattleEnd
        }
    }

    private void Start()
    {
        battleManager = FindObjectOfType<BattleManager>();

        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(ToggleLog);
            UpdateToggleButton();
        }

        if (logPanel != null)
        {
            logPanel.SetActive(isLogVisible);
        }
    }

    private void Update()
    {
        // Check if new entries have been added to the battle log
        if (battleManager != null && battleManager.battleLog.Count > lastLogCount)
        {
            UpdateLogDisplay();
            lastLogCount = battleManager.battleLog.Count;
        }
    }

    public void AddLogEntry(string message, BattleLogEntry.LogType type = BattleLogEntry.LogType.SystemMessage, bool isImportant = false)
    {
        BattleLogEntry entry = new BattleLogEntry
        {
            message = message,
            type = type,
            turnNumber = battleManager != null ? battleManager.turnCount : 0,
            timestamp = System.DateTime.Now.ToString("HH:mm:ss"),
            textColor = GetColorForType(type),
            isImportant = isImportant
        };

        CreateLogEntryUI(entry);
    }

    private void UpdateLogDisplay()
    {
        if (battleManager == null || battleManager.battleLog.Count == 0) return;

        // Get new entries
        for (int i = lastLogCount; i < battleManager.battleLog.Count; i++)
        {
            string logMessage = battleManager.battleLog[i];
            BattleLogEntry.LogType type = DetermineLogType(logMessage);

            BattleLogEntry entry = new BattleLogEntry
            {
                message = logMessage,
                type = type,
                turnNumber = battleManager.turnCount,
                timestamp = System.DateTime.Now.ToString("HH:mm:ss"),
                textColor = GetColorForType(type),
                isImportant = type == BattleLogEntry.LogType.Critical || type == BattleLogEntry.LogType.BattleEnd
            };

            CreateLogEntryUI(entry);
        }
    }

    private BattleLogEntry.LogType DetermineLogType(string message)
    {
        message = message.ToLower();

        if (message.Contains("critical") || message.Contains("crit"))
            return BattleLogEntry.LogType.Critical;

        if (message.Contains("healed") || message.Contains("restored"))
            return BattleLogEntry.LogType.Heal;

        if (message.Contains("damage") || message.Contains("attacked"))
        {
            if (message.Contains(battleManager.player.characterName.ToLower()))
                return BattleLogEntry.LogType.PlayerAction;
            else
                return BattleLogEntry.LogType.EnemyAction;
        }

        if (message.Contains("turn") || message.Contains("==="))
            return BattleLogEntry.LogType.TurnStart;

        if (message.Contains("defeated") || message.Contains("victory") || message.Contains("battle"))
            return BattleLogEntry.LogType.BattleEnd;

        return BattleLogEntry.LogType.SystemMessage;
    }

    private Color GetColorForType(BattleLogEntry.LogType type)
    {
        switch (type)
        {
            case BattleLogEntry.LogType.PlayerAction:
                return playerActionColor;
            case BattleLogEntry.LogType.EnemyAction:
                return enemyActionColor;
            case BattleLogEntry.LogType.Damage:
                return damageColor;
            case BattleLogEntry.LogType.Heal:
                return healColor;
            case BattleLogEntry.LogType.Critical:
                return criticalColor;
            case BattleLogEntry.LogType.TurnStart:
                return Color.yellow;
            case BattleLogEntry.LogType.BattleEnd:
                return Color.cyan;
            default:
                return systemMessageColor;
        }
    }

    private void CreateLogEntryUI(BattleLogEntry entry)
    {
        if (logEntryPrefab == null || logContent == null) return;

        GameObject logEntryObj = Instantiate(logEntryPrefab, logContent);
        logEntries.Add(logEntryObj);

        // Get components
        TextMeshProUGUI messageText = logEntryObj.GetComponentInChildren<TextMeshProUGUI>();
        Image background = logEntryObj.GetComponent<Image>();

        if (messageText != null)
        {
            // Format the message
            string formattedMessage = FormatLogMessage(entry);
            messageText.text = formattedMessage;
            messageText.color = entry.textColor;

            // Make important messages bold
            if (entry.isImportant)
            {
                messageText.fontStyle = FontStyles.Bold;
            }
        }

        // Set background color based on type
        if (background != null)
        {
            Color bgColor = Color.black;
            bgColor.a = entry.isImportant ? 0.3f : 0.1f;
            background.color = bgColor;
        }

        // Animate entry
        StartCoroutine(AnimateLogEntry(logEntryObj));

        // Remove old entries if we have too many
        if (logEntries.Count > maxLogEntries)
        {
            GameObject oldEntry = logEntries[0];
            logEntries.RemoveAt(0);
            Destroy(oldEntry);
        }

        // Auto-scroll to bottom
        if (autoScroll)
        {
            StartCoroutine(ScrollToBottom());
        }
    }

    private string FormatLogMessage(BattleLogEntry entry)
    {
        string formatted = "";

        // Add turn number
        if (showTurnNumbers && entry.turnNumber > 0)
        {
            formatted += $"<color=#888888>[Turn {entry.turnNumber}]</color> ";
        }

        // Add timestamp
        if (showTimestamps)
        {
            formatted += $"<color=#666666>{entry.timestamp}</color> ";
        }

        // Add type icon
        string icon = GetIconForType(entry.type);
        if (!string.IsNullOrEmpty(icon))
        {
            formatted += $"{icon} ";
        }

        // Add the main message
        formatted += entry.message;

        return formatted;
    }

    private string GetIconForType(BattleLogEntry.LogType type)
    {
        switch (type)
        {
            case BattleLogEntry.LogType.PlayerAction:
                return "⚔️";
            case BattleLogEntry.LogType.EnemyAction:
                return "🗡️";
            case BattleLogEntry.LogType.Damage:
                return "💥";
            case BattleLogEntry.LogType.Heal:
                return "💚";
            case BattleLogEntry.LogType.Critical:
                return "💥";
            case BattleLogEntry.LogType.TurnStart:
                return "🎯";
            case BattleLogEntry.LogType.BattleEnd:
                return "🏆";
            default:
                return "📝";
        }
    }

    private IEnumerator AnimateLogEntry(GameObject logEntry)
    {
        CanvasGroup canvasGroup = logEntry.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = logEntry.AddComponent<CanvasGroup>();
        }

        // Start transparent
        canvasGroup.alpha = 0f;

        // Fade in
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    private IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

    public void ToggleLog()
    {
        isLogVisible = !isLogVisible;
        if (logPanel != null)
        {
            logPanel.SetActive(isLogVisible);
        }
        UpdateToggleButton();
    }

    private void UpdateToggleButton()
    {
        if (toggleButtonText != null)
        {
            toggleButtonText.text = isLogVisible ? "Hide Log" : "Show Log";
        }
    }

    public void ClearLog()
    {
        foreach (GameObject entry in logEntries)
        {
            if (entry != null)
            {
                Destroy(entry);
            }
        }
        logEntries.Clear();
        lastLogCount = 0;
    }

    public void SetAutoScroll(bool enabled)
    {
        autoScroll = enabled;
    }

    public void SetShowTurnNumbers(bool enabled)
    {
        showTurnNumbers = enabled;
    }

    public void SetShowTimestamps(bool enabled)
    {
        showTimestamps = enabled;
    }

    // Public methods for BattleManager to call
    public void LogPlayerAction(string action, string target, int damage)
    {
        string message = $"{battleManager.player.characterName} used '{action}' on {target} for {damage} damage!";
        AddLogEntry(message, BattleLogEntry.LogType.PlayerAction);
    }

    public void LogEnemyAction(string enemyName, string target, int damage)
    {
        string message = $"{enemyName} attacked {target} for {damage} damage!";
        AddLogEntry(message, BattleLogEntry.LogType.EnemyAction);
    }

    public void LogCriticalHit(string attacker, string target, int damage)
    {
        string message = $"💥 CRITICAL HIT! {attacker} deals {damage} damage to {target}!";
        AddLogEntry(message, BattleLogEntry.LogType.Critical, true);
    }

    public void LogTurnStart(string characterName)
    {
        string message = $"=== {characterName}'s Turn ===";
        AddLogEntry(message, BattleLogEntry.LogType.TurnStart);
    }

    public void LogBattleEnd(string result)
    {
        string message = $"🏆 {result}";
        AddLogEntry(message, BattleLogEntry.LogType.BattleEnd, true);
    }
}