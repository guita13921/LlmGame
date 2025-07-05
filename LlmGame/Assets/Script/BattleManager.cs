using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class BattleManager : MonoBehaviour
{
    [SerializeField] public List<Character> allCharacters = new List<Character>();
    [SerializeField] private List<string> battleLog = new List<string>();
    [SerializeField] private int turnCount = 1;

    private float gaugeThreshold = 1000f;
    private bool battleActive = true;

    [SerializeField] private bool isActionPhase = false;
    [SerializeField] private Character currentActingCharacter = null;


    private void Start()
    {
        // ตัวอย่าง: เพิ่มตัวละคร
        var player = new Player("NetRunner", "Astra", "Cyber-hacker with high focus", 25, 25, 18, 70, 100, 60);
        var enemy = new Enemy("MechaWolf", "A brutal cybernetic wolf", 30, 20, 10, 80, 50, 40, EnemyArchetype.Attacker);

        allCharacters.Add(player);
        allCharacters.Add(enemy);

        // Init Turn Gauge ทุกตัว
        foreach (var c in allCharacters)
        {
            c.TurnGauge = 0f;
        }
    }

    private void Update()
    {
        if (!battleActive) return;

        // ถ้ายังมีตัวละครกำลังทำ Action → รอให้เสร็จก่อน
        if (isActionPhase)
            return;

        foreach (var character in allCharacters)
        {
            if (!character.IsAlive()) continue;

            character.TurnGauge += character.Speed * Time.deltaTime * 10; // ปรับ scale ได้

            if (character.TurnGauge >= gaugeThreshold)
            {
                currentActingCharacter = character;
                isActionPhase = true;
                character.TurnGauge = 0f;

                // เริ่ม Action (ถ้าอยากทำเป็น Coroutine รอ animation ก็ทำได้)
                StartCoroutine(DoAction(character));
                break; // หลังมีคนได้เทิร์น ให้หยุด loop ไปก่อน
            }
        }
    }

    private Character GetRandomOpponent(Character self)
    {
        List<Character> possibleTargets = new List<Character>();

        foreach (var c in allCharacters)
        {
            if (c != self && c.IsAlive())
            {
                possibleTargets.Add(c);
            }
        }

        if (possibleTargets.Count > 0)
        {
            return possibleTargets[Random.Range(0, possibleTargets.Count)];
        }

        return null;
    }

    private bool CheckBattleEnd()
    {
        // ตรวจถ้าเหลือผู้รอดชีวิตเพียงฝั่งเดียว
        int alivePlayers = 0;
        int aliveEnemies = 0;

        foreach (var c in allCharacters)
        {
            if (c.IsAlive())
            {
                if (c is Player) alivePlayers++;
                else if (c is Enemy) aliveEnemies++;
            }
        }

        return alivePlayers == 0 || aliveEnemies == 0;
    }

    // สำหรับดึง battle log ไปโชว์
    public string GetBattleLog()
    {
        StringBuilder sb = new StringBuilder();
        foreach (var entry in battleLog)
        {
            sb.AppendLine(entry);
        }
        return sb.ToString();
    }

    private System.Collections.IEnumerator DoAction(Character character)
    {
        Debug.Log($"=== {character.Name}'s Turn ===");
        //battleLog.Add($"{character.Name}'s Turn");

        // ทำ action เช่น เลือกเป้าหมาย
        Character target = GetRandomOpponent(character);
        if (target != null)
        {
            target.TakeDamage(character.Attack);
            battleLog.Add($"{character.Name}'s Turn {character} Attack {target} with Damage {character.Attack} make {target.CurrentHP}");
        }

        // ทำ delay เล็กน้อยจำลองแสดง animation
        yield return new WaitForSeconds(2.0f);

        // ตรวจจบการต่อสู้
        if (CheckBattleEnd())
        {
            battleActive = false;
            Debug.Log("Battle Finished!");
        }

        turnCount++;

        // จบ action phase
        isActionPhase = false;
        currentActingCharacter = null;
    }
}
