using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageReaction
{
    /// <summary>
    /// Allows the item to intercept and modify incoming damage.
    /// </summary>
    /// <param name="source">The attacking character.</param>
    /// <param name="target">The target character (being damaged).</param>
    /// <param name="damage">Incoming damage (modifiable by ref).</param>
    void OnBeforeDamage(Character source, Character target, ref int damage);

    /// <summary>
    /// Allows the item to react after damage is applied.
    /// </summary>
    /// <param name="source">The attacking character.</param>
    /// <param name="target">The target character.</param>
    /// <param name="finalDamage">The final damage applied.</param>
    void OnAfterDamage(Character source, Character target, int finalDamage);
}
