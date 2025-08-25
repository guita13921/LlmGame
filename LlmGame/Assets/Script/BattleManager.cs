using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Linq;
using TMPro;
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

    // ✅ NEW: Queue for safe character addition
    private List<Character> pendingCharacters = new List<Character>();
    private bool isResolvingAction = false;

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

            Transform spawnPoint = null;

            // If this is the boss, spawn at position 2 (index 2)
            if (group.enemies[i].GetComponent<Enemy>()?.archetype == EnemyArchetype.Boss && enemySpawnPoints.Length > 2)
            {
                spawnPoint = enemySpawnPoints[2]; // position 3 (index 2)
            }
            else
            {
                spawnPoint = enemySpawnPoints[i];
            }

            GameObject enemyObj = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);

            Enemy enemy = enemyObj.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.turnGauge = 0f;
                enemies.Add(enemy);
                allCharacters.Add(enemy);
            }
        }
    }

    public Enemy SpawnExtraEnemy(GameObject prefab, int spawnIndex = -1)
    {
        if (prefab == null || enemySpawnPoints == null || enemySpawnPoints.Length == 0)
            return null;

        Transform spawnPoint = null;

        if (spawnIndex >= 0 && spawnIndex < enemySpawnPoints.Length)
        {
            spawnPoint = enemySpawnPoints[spawnIndex];

            bool occupied = enemies.Any(e => Vector3.Distance(e.transform.position, spawnPoint.position) < 0.5f);
            if (occupied)
            {
                Debug.LogWarning($"Spawn position {spawnIndex} is occupied. Skipping spawn.");
                return null;
            }
        }
        else
        {
            // Fallback to random point
            spawnPoint = enemySpawnPoints[Random.Range(0, enemySpawnPoints.Length)];
        }

        GameObject enemyObj = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        Enemy enemy = enemyObj.GetComponent<Enemy>();

        if (enemy != null)
        {
            enemy.turnGauge = 0f;

            // ✅ Add immediately instead of queuing
            enemies.Add(enemy);
            allCharacters.Add(enemy);

            Debug.Log($"[Immediate Spawn] {enemy.characterName} added to battle.");
        }

        return enemy;
    }

    private void Update()
    {
        if (isActionPhase || !battleActive) return;

        foreach (var character in allCharacters)
        {
            if (!character.IsAlive()) continue;

            if (currentActingCharacter == null)
                character.turnGauge += character.speed * Time.deltaTime * 10;

            if (character.turnGauge >= gaugeThreshold)
            {
                currentActingCharacter = character;
                isActionPhase = true;
                character.turnGauge = 0f;

                foreach (var listener in character.GetComponentsInChildren<ITurnListener>())
                    listener.OnTurnStart(character);

                character.ProcessStatusEffects();

                foreach (var listener in character.GetComponentsInChildren<ITurnListener>())
                    listener.OnTurnEnd(character);

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
            chatAI.ShowInputUI();
            yield break; // Wait for player input
        }

        if (character is Enemy enemy)
        {
            if (enemy.availableActions == null || enemy.availableActions.Count == 0)
            {
                Debug.LogWarning($"{enemy.characterName} has no available actions.");
                yield break;
            }


            BossSkillBehavior bossAI = enemy.GetComponent<BossSkillBehavior>();

            if (enemy.archetype == EnemyArchetype.Boss && bossAI != null)
            {
                CharacterActionData action = bossAI.DecideAction();

                // Special case: handle summon or other unique behavior
                if (bossAI.HandleSpecialAction(action))
                {
                    //yield return StartCoroutine(combatHandler.EndEnemyTurn());
                    //yield break;
                }

                enemy.selectedAction = action;

                if (action != null && action.delayTurns > 0 && action.delayedAction != null)
                    enemy.pendingDelayedAction = action.delayedAction;
            }
            else
            {
                if (enemy.pendingDelayedAction != null)
                {
                    enemy.selectedAction = enemy.pendingDelayedAction;
                    enemy.pendingDelayedAction = null;
                }
                else
                {
                    int index = Random.Range(0, enemy.availableActions.Count);
                    enemy.selectedAction = enemy.availableActions[index];

                    if (enemy.selectedAction.delayTurns > 0 && enemy.selectedAction.delayedAction != null)
                        enemy.pendingDelayedAction = enemy.selectedAction.delayedAction;
                }
            }

            if (enemy.selectedAction == null)
            {
                Debug.LogError($"{enemy.characterName} has a null selectedAction.");
                yield break;
            }

            // Auto-activate enemy weapon
            enemy.activeItem.Clear();
            if (enemy.equippedWeapon != null)
            {
                enemy.equippedWeapon.isActive = true;
                enemy.activeItem.Add(enemy.equippedWeapon);
            }

            Character target = player;
            if (target != null)
            {
                Debug.Log($"Enemy {enemy.characterName} chosen action: {enemy.selectedAction.actionName}");
                combatHandler.EnemyAttack(enemy, target, enemy.selectedAction);
            }

            // Simulate time for enemy action resolution
            yield return new WaitForSeconds(1f);
        }
    }


    public void EndPlayerTurn()
    {
        isActionPhase = false;
        currentActingCharacter = null;
        selectedTarget = null;
        selectedParts.Clear();

        //AddPendingCharacters(); // ✅ Process newly spawned characters

        var inventoryUI = FindObjectOfType<BattleInventoryUI>();
        inventoryUI?.RefreshUI();

        chatAI.ShowInputUI();
        Debug.Log("Player turn ended.");
    }

    private IEnumerator SkipTurn(Character character)
    {
        yield return new WaitForSeconds(1f);
        Debug.Log($"{character.characterName}'s turn was skipped due to stun.");
        isActionPhase = false;
        currentActingCharacter = null;
        //AddPendingCharacters(); // ✅ Still add new characters even on skipped turn
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
        if (isResolvingAction) return;
        if (selectedTarget == selectedCharacter) return;

        Debug.Log($"Player selected {selectedCharacter.characterName} as target!");
        selectedTarget = selectedCharacter;
        StartCoroutine(ExecuteSelectionOnTarget(selectedCharacter));
    }

    private IEnumerator ExecuteSelectionOnTarget(Character target)
    {
        isResolvingAction = true;

        if (isUsingConsumableMode && player.activeItem.Count > 0)
        {
            Item active = player.activeItem[0];
            if (active is ConsumeTurnItem consumeItem)
            {
                yield return StartCoroutine(consumeItem.UseOnTarget(player, target, this));
            }
        }

        if (player.isUsingUltimateSkill && player.currentSkill != null)
        {
            yield return StartCoroutine(player.currentSkill.UseOnTarget(player, target, this));
        }

        isResolvingAction = false;
    }

    public string GetBattleLog()
    {
        StringBuilder sb = new StringBuilder();
        foreach (var entry in battleLog)
            sb.AppendLine(entry);

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

        var weaponAnim = character.GetComponent<PlayerWeaponAnimation>();
        if (weaponAnim != null && animationTriggerName == "Attack")
        {
            weaponAnim.PlayAttack();
        }
        else
        {
            character.animator.SetTrigger(animationTriggerName);
        }

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

    // ✅ Queuing methods
    public void QueueCharacterForSpawn(Character newCharacter)
    {
        if (newCharacter == null) return;
        if (pendingCharacters.Contains(newCharacter)) return;
        pendingCharacters.Add(newCharacter);
    }

    private void AddPendingCharacters()
    {
        if (pendingCharacters.Count == 0) return;

        foreach (var character in pendingCharacters)
        {
            if (!allCharacters.Contains(character))
                allCharacters.Add(character);

            if (character is Enemy enemy && !enemies.Contains(enemy))
                enemies.Add(enemy);

            character.turnGauge = 0f;
            character.gameObject.SetActive(true);

            Debug.Log($"[Spawned] {character.characterName} added to battle.");
        }

        pendingCharacters.Clear();
    }
}
