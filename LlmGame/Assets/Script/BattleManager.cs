using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Linq;
using TMPro;
using UnityEditor.Search;

public class BattleManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public Player player;
    [SerializeField] public List<Enemy> enemies = new List<Enemy>();
    [SerializeField] public ChatAI chatAI;
    [SerializeField] public CharacterCombatHandler combatHandler;
    [SerializeField] public DamageCalculator damageCalculator;
    [SerializeField] public TMP_InputField playerInputField;

    [Header("Show Debug")]
    [SerializeField] public bool showDebug;

    [Header("Character Lists")]
    [SerializeField] public List<Character> allCharacters = new List<Character>();
    [SerializeField] public List<string> battleLog = new List<string>();
    [SerializeField] public Character selectedTarget = null;
    [SerializeField] public List<BodyPartData> selectedParts = new List<BodyPartData>();


    [Header("Battle State")]
    [SerializeField] public int turnCount = 1;
    public float gaugeThreshold = 1000f;
    public bool battleActive = true;
    public bool isUsingConsumableMode = false;

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
        if (isActionPhase) return;

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

        isActionPhase = true;
        currentActingCharacter = character;

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
                enemy.selectedAction = enemy.availableActions[0];

                CheckAndActivateEnemyItems(enemy, enemy.selectedAction.actionName);
                CheckAndActivateDefensiveItems(enemy, target);

                Debug.Log($"Enemy {enemy.characterName} chosen action: {enemy.selectedAction.actionName}");

                combatHandler.EnemyAttack(enemy, target, enemy.selectedAction);
            }
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

    public void PlayerSelectedTarget(Character selectedCharacter)
    {
        if (player == null || !player.IsAlive()) return;
        if (selectedCharacter == null || !selectedCharacter.IsAlive()) return;

        Debug.Log($"Player selected {selectedCharacter.characterName} as target!");

        selectedTarget = selectedCharacter;

        if (isUsingConsumableMode && player.activeItem.Count > 0)
        {
            Item active = player.activeItem[0];

            if (active is ConsumeTurnItem consumeItem)
            {
                StartCoroutine(consumeItem.UseOnTarget(player, selectedCharacter, this));
            }
        }

        if (player.isUsingUltimateSkill == true)
        {
            StartCoroutine(player.currentSkill.UseOnTarget(player, selectedCharacter, this));
        }
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

    public void CheckAndActivateDefensiveItems(Character attacker, Character target)
    {
        HashSet<DamageType> incomingDamageTypes = new HashSet<DamageType>();

        // ✅ 1. Check if a skill is active
        if (attacker.currentSkill is DamageModifierSkill skill && skill.damageTypes != null && skill.damageTypes.Count > 0)
        {
            foreach (var dt in skill.damageTypes)
            {
                incomingDamageTypes.Add(dt);
            }

            Debug.Log($"[Defensive] Using skill '{skill.skillName}' with damage types: {string.Join(", ", skill.damageTypes)}");
        }
        else
        {
            // ✅ 2. Fallback to weapon items in activeItem
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

            if (incomingDamageTypes.Count == 0)
            {
                incomingDamageTypes.Add(DamageType.Physical);
                Debug.Log("No damage types detected. Defaulting to Physical damage.");
            }
        }

        Debug.Log($"Incoming damage types: {string.Join(", ", incomingDamageTypes.Select(t => t.ToString()))}");

        // ✅ 3. Clear previously active defensive items
        target.activeItem.RemoveAll(item => item is Defensive);

        // ✅ 4. Evaluate defensive items
        foreach (var item in target.inventoryItems)
        {
            if (item is Defensive defensive)
            {
                defensive.isActive = false; // Reset before checking

                bool hasMatchingType = defensive.damageTypeReduce.Any(dt => incomingDamageTypes.Contains(dt));

                if (hasMatchingType)
                {
                    defensive.isActive = true;

                    if (!target.activeItem.Contains(defensive))
                    {
                        target.activeItem.Add(defensive);
                    }

                    Debug.Log($"🛡️ Defensive item '{defensive.itemName}' activated! Matches: {string.Join(", ", defensive.damageTypeReduce)}");
                }
                else
                {
                    Debug.Log($"⚠️ Defensive item '{defensive.itemName}' did not match any damage types.");
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

    public IEnumerator WaitForAnimation(Character character, string animationTriggerName)
    {
        if (character.animator == null)
        {
            Debug.LogWarning($"{character.characterName} has no animator assigned!");
            yield break;
        }

        character.animationFinished = false;
        character.animator.SetTrigger(animationTriggerName);

        while (!character.animationFinished)
        {
            yield return null;
        }
    }

    public void EndPlayerTurn()
    {
        isActionPhase = false;
        currentActingCharacter = null;
        selectedTarget = null;
        selectedParts.Clear();

        chatAI.ShowInputUI(); // Reset UI
        Debug.Log("Player turn ended.");
    }


}
