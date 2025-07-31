using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IStatusEffectListener
{
    bool ShouldSpreadBleed(Character character);
}
