using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Linq;

public class BattleManager : MonoBehaviour
{
    [SerializeField] public Player player;
    [SerializeField] public List<Enemy> enemies = new List<Enemy>();
    [SerializeField] public List<Character> allCharacters = new List<Character>();
    [SerializeField] public List<string> battleLog = new List<string>();
    [SerializeField] public ChatAI chatAI;

    [SerializeField] public int turnCount = 1;
    public float gaugeThreshold = 1000f;
    public bool battleActive = true;

    [SerializeField] public bool isActionPhase = false;
    [SerializeField] public Character currentActingCharacter = null;

    // Store the last user message for damage calculation
    private string lastUserMessage = "";

    private void Start()
    {
        player.turnGauge = 0f;
        allCharacters.Add(player);

        foreach (var e in enemies)
        {
            e.turnGauge = 0f;
            allCharacters.Add(e);
        }
    }

    private void Update()
    {
        if (!battleActive) return;

        if (isActionPhase)
            return;

        foreach (var character in allCharacters)
        {
            if (!character.IsAlive()) continue;

            character.turnGauge += character.speed * Time.deltaTime * 10;

            if (character.turnGauge >= gaugeThreshold)
            {
                currentActingCharacter = character;
                isActionPhase = true;
                character.turnGauge = 0f;

                // Show or hide UI depending on who is acting
                if (character is Player)
                {
                    chatAI.ShowInputUI();
                }
                else
                {
                    chatAI.HideInputUI();
                }

                StartCoroutine(DoAction(character));
                break;
            }
        }
    }

    private Character GetRandomOpponent(Character self)
    {
        if (self is Player)
        {
            List<Enemy> aliveEnemies = enemies.FindAll(e => e.IsAlive());
            if (aliveEnemies.Count > 0)
            {
                return aliveEnemies[Random.Range(0, aliveEnemies.Count)];
            }
        }
        else if (self is Enemy)
        {
            if (player.IsAlive())
            {
                return player;
            }
        }

        return null;
    }

    private bool CheckBattleEnd()
    {
        if (!player.IsAlive())
        {
            Debug.Log("Player Defeated!");
            return true;
        }

        bool anyEnemyAlive = false;
        foreach (var e in enemies)
        {
            if (e.IsAlive())
            {
                anyEnemyAlive = true;
                break;
            }
        }

        if (!anyEnemyAlive)
        {
            Debug.Log("All Enemies Defeated!");
        }

        return !anyEnemyAlive;
    }

    public string GetBattleLog()
    {
        StringBuilder sb = new StringBuilder();
        foreach (var entry in battleLog)
        {
            sb.AppendLine(entry);
        }
        return sb.ToString();
    }

    private IEnumerator DoAction(Character character)
    {
        Debug.Log($"=== {character.characterName}'s Turn ===");

        if (character is Player)
        {
            Debug.Log("Player's turn: waiting for player to choose action.");
            chatAI.ShowInputUI();
            yield break;
        }
        else
        {
            Character target = GetRandomOpponent(character);
            if (target != null)
            {
                target.TakeDamage(character.attack);
                //Enemy Log
                battleLog.Add($"{character.characterName} attacked {target.characterName} for {character.attack} damage. {target.characterName} HP: {target.currentHP}");
            }

            yield return new WaitForSeconds(2.0f);

            if (CheckBattleEnd())
            {
                battleActive = false;
                Debug.Log("Battle Finished!");
            }

            turnCount++;
            isActionPhase = false;
            currentActingCharacter = null;

            chatAI.HideInputUI();
        }
    }

    // Method to store the user message for damage calculation
    public void SetUserMessage(string message)
    {
        lastUserMessage = message;
    }

    private List<string> GetPastMessagesFromActor(Character actor)
    {
        List<string> messages = new List<string>();

        foreach (string logEntry in battleLog)
        {
            // Check if this log entry was from this character
            if (logEntry.Contains(actor.characterName))
            {
                // Try extract message inside quotes (your new log format uses "..." for message)
                int start = logEntry.IndexOf("\"");
                int end = logEntry.LastIndexOf("\"");

                if (start != -1 && end != -1 && end > start)
                {
                    string extracted = logEntry.Substring(start + 1, end - start - 1);
                    messages.Add(extracted);
                }
            }
        }

        return messages;
    }


    private float CalculateCreativityBonus(string userMessage)
    {
        if (string.IsNullOrEmpty(userMessage))
            return 0f;

        // Combine past messages from this actor
        List<string> pastMessages = GetPastMessagesFromActor(currentActingCharacter);

        // Add current message
        pastMessages.Add(userMessage);

        // Combine into one big text
        string combinedText = string.Join(" ", pastMessages).ToLower();

        // Split words
        string[] words = combinedText
            .Split(new char[] { ' ', '\t', '\n', '\r', '.', ',', '!', '?', ';', ':', '"', '\'', '(', ')', '[', ']', '{', '}' },
                   System.StringSplitOptions.RemoveEmptyEntries);

        // Count unique words
        HashSet<string> uniqueWords = new HashSet<string>(words);
        int nUniqueWords = uniqueWords.Count;

        // Count words used at least 2 times
        Dictionary<string, int> wordCount = new Dictionary<string, int>();
        foreach (string word in words)
        {
            if (wordCount.ContainsKey(word))
                wordCount[word]++;
            else
                wordCount[word] = 1;
        }

        int nWordsUsedAtLeast2Times = wordCount.Count(kvp => kvp.Value >= 2);

        // Calculate bonuses
        float uniqueWordBonus = Mathf.Min(1f, nUniqueWords * 0.05f);
        float repetitionPenalty = Mathf.Min(0.3f, nWordsUsedAtLeast2Times * 0.02f);

        float creativityBonus = uniqueWordBonus - repetitionPenalty;

        Debug.Log($"Creativity Calculation: UniqueWords={nUniqueWords}, RepeatedWords={nWordsUsedAtLeast2Times}");
        Debug.Log($"UniqueWordBonus={uniqueWordBonus}, RepetitionPenalty={repetitionPenalty}, CreativityBonus={creativityBonus}");

        return creativityBonus;
    }

    private float CalculateDamage(float feasibility, float potential, float baseDamage, string userMessage)
    {
        const float constant = 2f;

        float llmDamageModifier = ((feasibility / 10) * (potential / 10)) * constant;
        float llmScaledBaseDamage = baseDamage * llmDamageModifier;
        float creativityBonus = CalculateCreativityBonus(userMessage);
        float totalDamage = llmScaledBaseDamage * (1 + creativityBonus);

        Debug.Log($"Damage Calculation: Feasibility={feasibility}, Potential={potential}, BaseDamage={baseDamage}");
        Debug.Log($"LLMDamageModifier={llmDamageModifier}, LLMScaledBaseDamage={llmScaledBaseDamage}");
        Debug.Log($"CreativityBonus={creativityBonus}, TotalDamage={totalDamage}");

        return totalDamage;
    }


    public void PlayerAttack(float feasibility, float potential, string effectValue, string effectDesc)
    {
        if (!(currentActingCharacter is Player))
        {
            Debug.LogWarning("Not player's turn!");
            return;
        }

        Character target = GetRandomOpponent(currentActingCharacter);

        if (target != null)
        {
            float baseDamage = currentActingCharacter.attack;
            float calculatedDamage = CalculateDamage(feasibility, potential, baseDamage, lastUserMessage);

            int finalDamage = Mathf.RoundToInt(calculatedDamage);

            target.TakeDamage(finalDamage);

            // Build new formatted log
            string log = $"Turn {turnCount}: {currentActingCharacter.characterName} (HP: {currentActingCharacter.currentHP}) " +
                         $"used \"{lastUserMessage}\" [EffectValue: {effectValue}, EffectDesc: {effectDesc}] " +
                         $"for {finalDamage} damage → Target: {target.characterName} (HP: {target.currentHP})";

            battleLog.Add(log);

            Debug.Log(log);
        }

        StartCoroutine(EndPlayerTurn());
    }


    // Keep the old method for backward compatibility
    public void PlayerAttack()
    {
        PlayerAttack(1f, 1f, "DefaultEffect", "DefaultDesc");
    }

    private IEnumerator EndPlayerTurn()
    {
        yield return new WaitForSeconds(2.0f);

        if (CheckBattleEnd())
        {
            battleActive = false;
            Debug.Log("Battle Finished!");
        }

        turnCount++;
        isActionPhase = false;
        currentActingCharacter = null;

        chatAI.HideInputUI();
    }
}