using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITurnListener
{
    void OnTurnStart(Character character);
    void OnTurnEnd(Character character);
}
