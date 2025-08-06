using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPossibilityModifier
{
    void ModifyChances(PossibilityPool pool);

    void ModifyCritical(Character character, PossibilityPool pool);
}