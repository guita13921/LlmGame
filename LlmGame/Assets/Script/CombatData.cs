using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct DamageResult
{
    public float damage;
    public float feasibility;
    public float potential;

    public DamageResult(float damage, float feasibility, float potential)
    {
        this.damage = damage;
        this.feasibility = feasibility;
        this.potential = potential;
    }
}
