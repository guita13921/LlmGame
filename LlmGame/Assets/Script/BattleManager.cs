using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Linq;
using TMPro;
using System.Text.RegularExpressions; // Make sure this is at the top
using Map;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public Player player;
    [SerializeField] public List<Enemy> enemies = new List<Enemy>();
    [SerializeField] public ChatAI chatAI;
    [SerializeField] public CharacterCombatHandler combatHandler;
    [SerializeField] public DamageCalculator damageCalculator;
    [SerializeField] public TMP_InputField playerInputField;

    [System.Serializable]
    public class EnemyGroup
    {
        public List<GameObject> enemies = new List<GameObject>();
    }

    [Header("Enemy Pools")]
    public List<EnemyGroup> minorEasyGroups = new List<EnemyGroup>();
    public List<EnemyGroup> minorNormalGroups = new List<EnemyGroup>();
    public List<EnemyGroup> minorHardGroups = new List<EnemyGroup>();
    public List<EnemyGroup> eliteEasyGroups = new List<EnemyGroup>();
    public List<EnemyGroup> eliteHardGroups = new List<EnemyGroup>();
    public List<EnemyGroup> bossEnemyGroups = new List<EnemyGroup>();

    [Header("Enemy Spawn Points")]
    [SerializeField] public Transform[] enemySpawnPoints = new Transform[3];

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
        allCharacters.Clear();
        allCharacters.Add(player);

        SpawnEnemyForCurrentNode();
    }

    private void SpawnEnemyForCurrentNode()
    {
        enemies.Clear();
        if (PlayerData.Instance == null) return;

        EnemyGroup group = GetRandomEnemyGroup(PlayerData.Instance.nextNodeType, PlayerData.Instance.nextEnemyDifficulty);
        if (group == null || group.enemies == null) return;

        int spawnCount = Mathf.Min(group.enemies.Count, enemySpawnPoints.Length);
        for (int i = 0; i < spawnCount; i++)
        {
            GameObject prefab = group.enemies[i];
            if (prefab == null) continue;

            Transform spawnPoint = enemySpawnPoints != null && i < enemySpawnPoints.Length ? enemySpawnPoints[i] : null;
            GameObject enemyObj = spawnPoint != null
                ? Instantiate(prefab, spawnPoint.position, spawnPoint.rotation)
                : Instantiate(prefab);

            Enemy enemy = enemyObj.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.turnGauge = 0f;
                enemies.Add(enemy);
                allCharacters.Add(enemy);
            }
        }
    }

    private EnemyGroup GetRandomEnemyGroup(NodeType type, EnemyDifficulty difficulty)
    {
        List<EnemyGroup> pool = null;
        switch (type)
        {
            case NodeType.MinorEnemy:
                pool = difficulty switch
                {
                    EnemyDifficulty.Easy => minorEasyGroups,
                    EnemyDifficulty.Normal => minorNormalGroups,
                    EnemyDifficulty.Hard => minorHardGroups,
                    _ => minorEasyGroups
                };
                break;
            case NodeType.EliteEnemy:
                pool = difficulty == EnemyDifficulty.Easy ? eliteEasyGroups : eliteHardGroups;
                break;
            case NodeType.Boss:
                pool = bossEnemyGroups;
                break;
        }

        if (pool == null || pool.Count == 0) return null;
        int index = Random.Range(0, pool.Count);
        return pool[index];
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

                foreach (var ticker in character.GetComponentsInChildren<ITurnListener>())
                {
                    ticker.OnTurnStart(character);
                }

                // ⬇️ Process status effects BEFORE they take their action
                character.ProcessStatusEffects();

                foreach (var ticker in character.GetComponentsInChildren<ITurnListener>())
                {
                    ticker.OnTurnEnd(character);
                }

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

        if (character is Enemy enemy)
        {
            if (enemy.availableActions == null || enemy.availableActions.Count == 0)
            {
                Debug.LogWarning($"{enemy.characterName} has no available actions.");
                yield break;
            }

            enemy.selectedAction = enemy.availableActions[0];
            if (enemy.selectedAction == null)
            {
                Debug.LogError($"{enemy.characterName} has a null selectedAction.");
                yield break;
            }

            // ✅ Check if enemy should use a consumable instead of attack
            if (CheckAndActivateEnemyItems(enemy, enemy.selectedAction.actionName, out IEnumerator consumeRoutine))
            {
                yield return StartCoroutine(consumeRoutine); // ✅ Use consumable
                yield break; // ✅ End turn, skip attack
            }

            // 🎯 Perform normal attack
            Character target = GetRandomOpponent(enemy);
            if (target != null)
            {
                Debug.Log($"Enemy {enemy.characterName} chosen action: {enemy.selectedAction.actionName}");
                combatHandler.EnemyAttack(enemy, target, enemy.selectedAction);
            }
        }
    }

    /*
    private Character GetLowestHPTargetInTeam(Enemy user)
    {
        List<Character> allies = GetAlliesOf(user);

        Character lowest = null;
        int lowestHP = int.MaxValue;

        foreach (var ally in allies)
        {
            if (ally.IsAlive() && ally.currentHP < ally.maxHP && ally.currentHP < lowestHP)
            {
                lowestHP = ally.currentHP;
                lowest = ally;
            }
        }

        return lowest;
    }

    // Replace with your actual method to get allies of a character
    private List<Character> GetAlliesOf(Character character)
    {
        return allCharacters.FindAll(c =>
            c.characterType == character.characterType &&
            c.IsAlive());
    }
    */


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

    private bool CheckAndActivateEnemyItems(Enemy enemy, string enemyAction, out IEnumerator consumeRoutine)
    {
        string lowerAction = enemyAction.ToLower();

        enemy.activeItem.Clear();
        consumeRoutine = null;

        // 🔁 Reset activation states
        foreach (var item in enemy.inventoryItems)
            item.isActive = false;

        // ✅ Weapon activation
        ProcessWeaponForActivation(enemy.leftHandWeapon, lowerAction, enemy);
        ProcessWeaponForActivation(enemy.rightHandWeapon, lowerAction, enemy);

        // ✅ Optional: Activate items based on keywords
        foreach (var item in enemy.activeItem)
        {
            if (item == null || item.keyWords == null) continue;

            if (item is ConsumeTurnItem consumeItem)
            {
                foreach (string keyword in item.keyWords)
                {
                    string lowerKeyword = keyword.ToLower();

                    Character healTarget = enemy;
                    item.isActive = true;
                    enemy.activeItem.Add(item);

                    enemy.isUsingConsumeTurnItem = true;

                    if (healTarget != null)
                    {
                        Debug.Log($"Enemy {enemy.characterName} will use {consumeItem.itemName} on {healTarget.characterName}");
                        consumeRoutine = consumeItem.UseOnTarget(enemy, healTarget, this);
                        return true;
                    }
                    else
                    {
                        Debug.LogWarning("No valid heal target found.");
                    }

                    return true;
                }
            }
        }


        return false; // No matching item to consume
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
            SceneManager.LoadScene("GameOver");
            return true;
        }

        bool anyEnemyAlive = enemies.Any(e => e.IsAlive());
        if (!anyEnemyAlive)
        {
            Debug.Log("All Enemies Defeated!");
            SceneManager.LoadScene("MapGenerate");
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

        float timeout = 3f;
        float timer = 0f;

        while (!character.animationFinished && timer < timeout)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (!character.animationFinished)
        {
            Debug.LogWarning($"Animation '{animationTriggerName}' for {character.characterName} timed out!");
        }
    }

    public void EndPlayerTurn()
    {
        isActionPhase = false;
        currentActingCharacter = null;
        selectedTarget = null;
        selectedParts.Clear();

        // Refresh inventory visuals in case items were consumed or equipment changed
        var inventoryUI = FindObjectOfType<BattleInventoryUI>();
        inventoryUI?.RefreshUI();

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
