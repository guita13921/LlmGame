using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDamageModifierSkill", menuName = "Skills/Damage Modifier Skill")]
public class DamageModifierSkill : ScriptableObject
{
    public string skillName;

    [TextArea]
    public string skillDescription;

    [Header("StatToCharacter")]
    public int damagePhysical;   // Bullets, blades, blunt weapons
    public int damageFire;       // Flamethrowers, incendiary rounds
    public int damageElectric;   // Shock batons, EMP, etc.
    public int damageRadiation;  // Dirty energy, nuclear weapons
    public int damageExplosive;  // Grenades, rockets
    public int damageDigital;    // Hacking, cyberbrain disruption
    public int damagePlasma;     // High-energy plasma
    public int damageLaser;      // Laser weapons
    public int damageChemical;   // Gas, toxins
    public int damageViral;      // Biological or digital viruses

    [Header("StatToCharacter")]
    public bool isDamagePercentagePart;
    public int percentDamgePerPart;

    public int mpCost = 80;

    [Header("Animation Trigger")]
    public string aniamtionTrigger;

    [Tooltip("Which types of damage this skill affects.")]
    public List<DamageType> damageTypes = new List<DamageType>();

    [Tooltip("Which body parts this skill affects.")]
    public List<BodyPartType> bodyPartTypes = new List<BodyPartType>();

    //[Range(-1f, 1f), Tooltip("Modifier value: -1 = immune, 0 = neutral, +1 = double damage")]
    //public float modifier = 0f;
    public IEnumerator UseOnTarget(Character user, Character target, BattleManager battleManager)
    {
        // ✅ MP Check
        if (user.currentMP < mpCost)
        {
            Debug.LogWarning($"{user.characterName} does not have enough MP to use {skillName}!");
            yield break;
        }

        user.currentMP -= mpCost;
        Debug.Log($"{user.characterName} uses {skillName} on {target.characterName}");

        // Apply specific skill effects
        if (skillName == "Radiant Collapse")
        {
            // Permanent Poison and Radiation on target
            var poison = new TurnStatusEffect(StatusEffectType.Poison, int.MaxValue, 1, user, true);
            target.ApplyStatusEffect(poison);
            var radiation = new TurnStatusEffect(StatusEffectType.Radiation, int.MaxValue, 1, user, true);
            target.ApplyStatusEffect(radiation);
        }
        else if (skillName == "Multi-Target Scan")
        {
            Debug.Log("buff user: next 3 attacks have +100% crit chance and +100% crit damage");
            var critChance = new TurnStatusEffect(StatusEffectType.CritChanceUp, 3, 100, user);
            user.ApplyStatusEffect(critChance);
            var critDamage = new TurnStatusEffect(StatusEffectType.CritDamageUp, 3, 100, user);
            user.ApplyStatusEffect(critDamage);
        }

        /*
        // ✅ Optional damage logic (currently commented out)
        */

        // ✅ Build LLM-readable message with {Skill} prefix
        string skillMessage = $"{{Skill}} {skillName}: {skillDescription}";

        // ✅ Update chat input & user message
        battleManager.SetUserMessage(skillMessage);

        // ✅ Select body parts on the actual target
        battleManager.selectedParts.Clear();
        if (battleManager.selectedTarget != null && battleManager.selectedTarget.bodyParts != null)
        {
            foreach (var part in battleManager.selectedTarget.bodyParts)
            {
                if (part != null && bodyPartTypes.Contains(part.type))
                {
                    battleManager.selectedParts.Add(part);
                    Debug.Log($"✅ [Skill] Selected body part: {part.type} on {battleManager.selectedTarget.characterName}");
                }
            }

            if (battleManager.selectedParts.Count == 0)
            {
                Debug.LogWarning($"⚠️ [Skill] No matching body parts found on {battleManager.selectedTarget.characterName} for skill {skillName}");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ [Skill] No selected target or body parts missing!");
        }

        // ✅ Trigger skill-related item and defense checks
        PromptBuilder.CheckAndActivateItems(battleManager, skillMessage, target);
        // ✅ Apply status effects and build prompt
        battleManager.chatAI.turnEffectText = battleManager.combatHandler.TryApplyStatusEffects(battleManager.player, target);
        string finalPrompt = PromptBuilder.BuildPlayerPrompt(battleManager, target, skillMessage, battleManager.chatAI.turnEffectText);
        battleManager.StartCoroutine(battleManager.chatAI.SendMessageToAI(finalPrompt));

        // ⚠️ End turn and reset flags should be done by BattleManager after skill resolves
    }

}
