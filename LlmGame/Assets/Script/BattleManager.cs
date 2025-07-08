using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

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

    public void PlayerAttack()
    {
        if (!(currentActingCharacter is Player))
        {
            Debug.LogWarning("Not player's turn!");
            return;
        }

        Character target = GetRandomOpponent(currentActingCharacter);

        if (target != null)
        {
            target.TakeDamage(currentActingCharacter.attack);
            battleLog.Add($"{currentActingCharacter.characterName} attacked {target.characterName} for {currentActingCharacter.attack} damage. {target.characterName} HP: {target.currentHP}");
        }

        StartCoroutine(EndPlayerTurn());
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
