using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAttackListener
{
    /// Called when an attack hits.
    /// Returns any notable effects that should be included in battle narration.
    List<string> OnAttackHit(Character attacker, Character target);
}