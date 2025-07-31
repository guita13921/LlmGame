using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Linq;
using TMPro;
using System.Text.RegularExpressions; // Make sure this is at the top

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

                // ⬇️ Process status effects BEFORE they take their action
                character.ProcessStatusEffects();

                // ⬇️ If stunned, skip action
                if (character.HasStatusEffect(StatusEffectType.Stun))
                {
                    Debug.Log($"{character.characterName} is stunned and skips their turn.");
                    StartCoroutine(SkipTurn(character));
                    break;
                }

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

        // Clear all item activation states
        foreach (var item in enemy.inventoryItems)
        {
            item.isActive = false;
        }

        enemy.activeItem.Clear();

        // Check left hand weapon
        ProcessWeaponForActivation(enemy.leftHandWeapon, lowerAction, enemy);

        // Check right hand weapon
        ProcessWeaponForActivation(enemy.rightHandWeapon, lowerAction, enemy);

        Debug.Log($"Total enemy active items: {enemy.activeItem.Count}");
    }

    private void ProcessWeaponForActivation(Weapon weapon, string lowerAction, Enemy enemy)
    {
        if (weapon == null || enemy == null)
            return;

        // Extract words from the action string
        var actionWords = Regex.Matches(lowerAction, @"\b\w+\b")
                               .Cast<Match>()
                               .Select(m => m.Value.ToLower())
                               .ToHashSet(); // For fast keyword lookup

        bool keywordFound = false;

        foreach (string keyword in weapon.keyWords)
        {
            if (!string.IsNullOrWhiteSpace(keyword) && actionWords.Contains(keyword.ToLower()))
            {
                if (!keywordFound)
                {
                    weapon.isActive = true;
                    enemy.activeItem.Add(weapon);
                    keywordFound = true;
                }

                Debug.Log($"Sub_Weapon '{weapon.itemName}' activated by keyword: '{keyword}' from action: '{lowerAction}'");
            }
        }

        if (!keywordFound)
        {
            Debug.Log($"Sub_Weapon '{weapon.itemName}' remains inactive - no keywords matched");
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

    private IEnumerator SkipTurn(Character character)
    {
        yield return new WaitForSeconds(1f); // Simulate a delay
        Debug.Log($"{character.characterName}'s turn was skipped due to stun.");
        isActionPhase = false;
    }

}
