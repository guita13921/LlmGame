using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Linq;

public class BattleManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public Player player;
    [SerializeField] public List<Enemy> enemies = new List<Enemy>();
    [SerializeField] public ChatAI chatAI;
    [SerializeField] public CharacterCombatHandler combatHandler;
    [SerializeField] public DamageCalculator damageCalculator;

    [Header("Character Lists")]
    [SerializeField] public List<Character> allCharacters = new List<Character>();
    [SerializeField] public List<string> battleLog = new List<string>();
    [HideInInspector] public Enemy selectedEnemy = null;

    [Header("Battle State")]
    [SerializeField] public int turnCount = 1;
    public float gaugeThreshold = 1000f;
    public bool battleActive = true;

    [SerializeField] public bool isActionPhase = false;
    [SerializeField] public Character currentActingCharacter = null;

    [HideInInspector] public string lastUserMessage = "";

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

        if (isActionPhase && currentActingCharacter is Player)
        {
            return; // Wait for player's input
        }

        if (isActionPhase) return;

        foreach (var character in allCharacters)
        {
            if (!character.IsAlive()) continue;

            character.turnGauge += character.speed * Time.deltaTime * 10;

            if (character.turnGauge >= gaugeThreshold)
            {
                currentActingCharacter = character;
                isActionPhase = true;
                character.turnGauge = 0f;

                if (character is Player)
                    chatAI.ShowInputUI();
                else
                    chatAI.HideInputUI();

                StartCoroutine(DoAction(character));
                break;
            }
        }
    }

    private IEnumerator DoAction(Character character)
    {
        Debug.Log($"=== {character.characterName}'s Turn ===");

        if (character is Player)
        {
            Debug.Log("Player's turn: waiting for player input.");
            chatAI.ShowInputUI();
            yield break;
        }
        else if (character is Enemy enemy)
        {
            Character target = GetRandomOpponent(enemy);
            if (target != null)
            {
                string enemyAction = GetRandomEnemyAction(enemy);
                CheckAndActivateEnemyItems(enemy, enemyAction);
                CheckAndActivateDefensiveItems(enemy, target);

                Debug.Log($"Enemy {enemy.characterName} chosen action: {enemyAction}");
                Debug.Log($"Enemy active items: {enemy.activeItem.Count}");

                combatHandler.EnemyAttack(enemy, target, enemyAction);
            }

            yield return new WaitForSeconds(2.0f);

            if (CheckBattleEnd())
            {
                battleActive = false;
                Debug.Log("Battle Finished!");
            }

            isActionPhase = false;
            currentActingCharacter = null;

            chatAI.HideInputUI();
        }
    }

    public Character GetRandomOpponent(Character self)
    {
        if (self is Player)
        {
            List<Enemy> aliveEnemies = enemies.FindAll(e => e.IsAlive());
            if (aliveEnemies.Count > 0)
                return aliveEnemies[Random.Range(0, aliveEnemies.Count)];
        }
        else if (self is Enemy && player.IsAlive())
        {
            return player;
        }

        return null;
    }

    public void PlayerSelectedTarget(Enemy selectedEnemy)
    {
        if (player == null || !player.IsAlive()) return;
        if (selectedEnemy == null || !selectedEnemy.IsAlive()) return;

        Debug.Log($"Player selected {selectedEnemy.characterName} as target!");

        // ✅ Just set it
        this.selectedEnemy = selectedEnemy;
    }

    private string GetRandomEnemyAction(Enemy enemy)
    {
        if (enemy.actions == null || enemy.actions.Count == 0)
        {
            Debug.LogWarning($"Enemy {enemy.characterName} has no actions defined, using default 'Punch'");
            return "Punch";
        }

        int randomIndex = Random.Range(0, enemy.actions.Count);
        return enemy.actions[randomIndex];
    }

    private void CheckAndActivateEnemyItems(Enemy enemy, string enemyAction)
    {
        string lowerAction = enemyAction.ToLower();

        foreach (var item in enemy.inventoryItems)
        {
            item.isActive = false;
        }

        enemy.activeItem.Clear();

        foreach (var item in enemy.inventoryItems)
        {
            bool keywordFound = false;
            foreach (string keyword in item.keyWords)
            {
                if (!string.IsNullOrEmpty(keyword) && lowerAction.Contains(keyword.ToLower()))
                {
                    item.isActive = true;
                    keywordFound = true;
                    enemy.activeItem.Add(item);

                    Debug.Log($"Enemy item '{item.itemName}' activated by keyword: '{keyword}' from action: '{enemyAction}'");
                    break;
                }
            }

            if (!keywordFound)
            {
                Debug.Log($"Enemy item '{item.itemName}' remains inactive - no keywords matched");
            }
        }

        Debug.Log($"Total enemy active items: {enemy.activeItem.Count}");
    }

    /// Checks target defensive items and activates them if they match any incoming damage type.
    /// <param name="attacker">The character dealing damage (used to read weapon types)</param>
    /// <param name="target">The character receiving damage (defensive items are checked)</param>
    public void CheckAndActivateDefensiveItems(Character attacker, Character target)
    {
        HashSet<DamageType> incomingDamageTypes = new HashSet<DamageType>();

        foreach (var weaponItem in attacker.activeItem)
        {
            if (weaponItem is Weapon weapon)
            {
                foreach (var dt in weapon.damageType)
                {
                    incomingDamageTypes.Add(dt);
                }
            }
        }

        // If no damage types found, default to Physical
        if (incomingDamageTypes.Count == 0)
        {
            incomingDamageTypes.Add(DamageType.Physical);
            Debug.Log("No damage types detected. Defaulting to Physical damage.");
        }

        Debug.Log($"Incoming damage types: {string.Join(", ", incomingDamageTypes.Select(t => t.ToString()))}");

        // Clear active defensive items first
        target.activeItem.RemoveAll(item => item is Defensive);

        // Check each defensive item
        foreach (var item in target.inventoryItems)
        {
            if (item is Defensive defensive)
            {
                defensive.isActive = false; // Reset before checking

                bool hasMatchingType = defensive.damageTypeReduce.Any(dt => incomingDamageTypes.Contains(dt));

                if (hasMatchingType)
                {
                    defensive.isActive = true;

                    // Only add if not already in activeItem
                    if (!target.activeItem.Contains(defensive))
                    {
                        target.activeItem.Add(defensive);
                    }

                    Debug.Log($"Defensive item '{defensive.itemName}' activated! Matches damage types: {string.Join(", ", defensive.damageTypeReduce.Select(t => t.ToString()))}");
                }
                else
                {
                    Debug.Log($"Defensive item '{defensive.itemName}' did not match any damage types and remains inactive.");
                }
            }
        }
    }

    public void SetUserMessage(string message)
    {
        lastUserMessage = message;
    }

    public bool CheckBattleEnd()
    {
        if (!player.IsAlive())
        {
            Debug.Log("Player Defeated!");
            return true;
        }

        bool anyEnemyAlive = enemies.Any(e => e.IsAlive());
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

    public List<string> GetPastMessagesFromActor(Character actor)
    {
        List<string> messages = new List<string>();

        foreach (string logEntry in battleLog)
        {
            if (logEntry.Contains(actor.characterName))
            {
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
}
