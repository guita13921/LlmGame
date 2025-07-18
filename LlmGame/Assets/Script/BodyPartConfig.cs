using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBodyPartConfig", menuName = "Character/Body Part Config")]
public class BodyPartConfig : ScriptableObject
{
    [Tooltip("List of body part overrides used to initialize runtime body parts.")]
    public List<BodyPartOverride> partOverrides = new List<BodyPartOverride>();

    // Auto-generate BodyPartData instances from the override data
    public List<BodyPartData> GenerateBodyParts()
    {
        List<BodyPartData> result = new List<BodyPartData>();

        foreach (var overrideData in partOverrides)
        {
            BodyPartData part = ScriptableObject.CreateInstance<BodyPartData>();
            part.type = overrideData.type;
            part.state = overrideData.defaultState;
            part.composition = overrideData.composition;
            part.keyword = new List<string>(overrideData.keywords);
            part.isVital = overrideData.isVital;
            part.health = overrideData.health;
            part.damageToPartRatio = overrideData.damageToPartRatio;
            part.equippedArmor = overrideData.defaultArmor;
            part.linkedWeakPoint = overrideData.weakPoint;
            part.hideFlags = HideFlags.DontSave; // Ensure they don't persist in memory

            result.Add(part);
        }

        return result;
    }
}
