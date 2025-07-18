using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWeakPoint", menuName = "Character/Weak Point")]
public class WeakPointData : ScriptableObject
{
    public string weakPointName;
    public bool isExposed = false;

    [TextArea]
    public string weakPointDescription;
}
