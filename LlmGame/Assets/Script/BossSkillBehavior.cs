using System.Linq;
using UnityEngine;

public class BossSkillBehavior : MonoBehaviour
{
    private Enemy enemy;
    public BattleManager battleManager;

    [Header("Phase 1")]
    public CharacterActionData summonThugs;
    public CharacterActionData smokeVeil;
    public CharacterActionData rallyOrders;

    [Header("Phase 2")]
    public CharacterActionData executionBullet;
    public CharacterActionData kingsFury;

    [Header("Summon Prefabs")]
    public GameObject pipeManPrefab;
    public GameObject robotPrefab;

    private int summonUseCount = 0;
    private bool smokeVeilUsed = false;
    private bool kingsFuryUsed = false;

    // Tracks whether Smoke Veil is currently active.
    private bool smokeVeilActive = false;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        if (battleManager == null)
            battleManager = FindObjectOfType<BattleManager>();
    }

    public bool IsUntargetable()
    {
        return smokeVeilActive && battleManager != null && battleManager.enemies.Any(e => e != enemy && e.IsAlive());
    }

    public CharacterActionData DecideAction()
    {
        if (enemy == null) return null;

        if (enemy.pendingDelayedAction != null)
        {
            var pending = enemy.pendingDelayedAction;
            enemy.pendingDelayedAction = null;
            return pending;
        }

        enemy.possibilityPool.SetBaseChance(StatusChanceType.Bleed, 0f);
        enemy.possibilityPool.SetBaseChance(StatusChanceType.Critical, 0f);

        if (smokeVeilActive && battleManager != null && !battleManager.enemies.Any(e => e != enemy && e.IsAlive()))
            smokeVeilActive = false;

        float hpPercent = (float)enemy.currentHP / enemy.maxHP;

        if (hpPercent <= 0.5f)
        {
            if (!kingsFuryUsed && kingsFury != null)
            {
                kingsFuryUsed = true;
                return kingsFury;
            }
            return executionBullet;
        }

        if (summonUseCount < 2)
        {
            bool first = hpPercent <= 0.8f && summonUseCount == 0;
            bool second = hpPercent <= 0.6f && summonUseCount == 1;
            if (first || second)
                return summonThugs;
        }

        if (!smokeVeilUsed && battleManager != null && battleManager.enemies.Any(e => e != enemy && e.IsAlive()))
        {
            smokeVeilUsed = true;
            return smokeVeil;
        }

        if (battleManager != null && battleManager.enemies.Any(e => e != enemy && e.IsAlive()) && Random.value < 0.5f)
        {
            return rallyOrders;
        }

        return enemy.availableActions.FirstOrDefault(a =>
            a != null &&
            a != summonThugs &&
            a != smokeVeil &&
            a != rallyOrders &&
            a != executionBullet &&
            a != kingsFury) ?? enemy.availableActions.FirstOrDefault();
    }

    public bool HandleSpecialAction(CharacterActionData action)
    {
        if (action == summonThugs)
        {
            summonUseCount++;

            int[] spawnIndexes = new int[] { 0, 1 };
            int count = Mathf.Min(spawnIndexes.Length, Random.Range(1, 3));

            for (int i = 0; i < count; i++)
            {
                GameObject prefab = Random.value < 0.5f ? pipeManPrefab : robotPrefab;
                int index = spawnIndexes[i];
                battleManager?.SpawnExtraEnemy(prefab, index);
            }

            return true;
        }

        if (action == smokeVeil)
        {
            smokeVeilActive = true;
            return true;
        }

        if (action == rallyOrders)
        {
            StatusEffectType buffType = Random.value < 0.5f ? StatusEffectType.AttackUp : StatusEffectType.DefenseUp;
            foreach (var ally in battleManager.enemies.Where(e => e.IsAlive()))
            {
                TurnStatusEffect buff = new(buffType, 2, 1, enemy);
                ally.ApplyStatusEffect(buff);
            }
            return true;
        }

        if (action == executionBullet)
        {
            enemy.possibilityPool.SetBaseChance(StatusChanceType.Bleed, 0.25f);
            enemy.possibilityPool.SetBaseChance(StatusChanceType.Critical, 1f);
            return false;
        }

        if (action == kingsFury)
        {
            TurnStatusEffect atk = new(StatusEffectType.AttackUp, 2, 1, enemy);
            TurnStatusEffect spd = new(StatusEffectType.SpeedUp, 2, 1, enemy);
            enemy.ApplyStatusEffect(atk);
            enemy.ApplyStatusEffect(spd);
            return true;
        }

        return false;
    }
}
