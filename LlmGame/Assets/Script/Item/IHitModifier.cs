using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHitModifier
{
    void OnHit(Character attacker, BodyPartData targetPart, ref int damage);
}
