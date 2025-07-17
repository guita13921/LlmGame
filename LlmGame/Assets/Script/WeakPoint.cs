using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWeakPoint", menuName = "Character/Weak Point")]
public class WeakPointData : ScriptableObject
{
    public string weakPointName;
    public string keyword;

    [TextArea]
    public string weakPointDescription;
}
