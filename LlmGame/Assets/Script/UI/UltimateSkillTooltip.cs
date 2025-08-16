using UnityEngine;
using TMPro;

public class UltimateSkillTooltip : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI mpCostText;

    [Header("Positioning")]
    public Canvas canvas;
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (canvas == null) canvas = GetComponentInParent<Canvas>();
        HideTooltip();
    }

    public void ShowTooltip(DamageModifierSkill skill, RectTransform target)
    {
        if (skill == null) return;

        // Basic info
        nameText.text = skill.skillName;
        descriptionText.text = skill.skillDescription;
        mpCostText.text = $"MP Cost: {skill.mpCost}";

        // Damage stats (only non-zero)
        string dmg = "";
        if (skill.damagePhysical > 0) dmg += $"Physical: {skill.damagePhysical}\n";
        if (skill.damageFire > 0) dmg += $"Fire: {skill.damageFire}\n";
        if (skill.damageElectric > 0) dmg += $"Electric: {skill.damageElectric}\n";
        if (skill.damageRadiation > 0) dmg += $"Radiation: {skill.damageRadiation}\n";
        if (skill.damageExplosive > 0) dmg += $"Explosive: {skill.damageExplosive}\n";
        if (skill.damageDigital > 0) dmg += $"Digital: {skill.damageDigital}\n";
        if (skill.damagePlasma > 0) dmg += $"Plasma: {skill.damagePlasma}\n";
        if (skill.damageLaser > 0) dmg += $"Laser: {skill.damageLaser}\n";
        if (skill.damageChemical > 0) dmg += $"Chemical: {skill.damageChemical}\n";
        if (skill.damageViral > 0) dmg += $"Viral: {skill.damageViral}\n";

        damageText.text = string.IsNullOrEmpty(dmg) ? "No direct damage" : dmg.TrimEnd();

        PositionTooltip(target);
        gameObject.SetActive(true);
    }

    public void HideTooltip() => gameObject.SetActive(false);

    private void PositionTooltip(RectTransform target)
    {
        if (canvas == null || rectTransform == null || target == null) return;

        // Get top-right of the button
        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);
        Vector3 topRightWorld = corners[2];

        // World -> Screen
        Camera uiCam = (canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : canvas.worldCamera;
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCam, topRightWorld);

        // Screen -> Local
        RectTransform parentRect = rectTransform.parent as RectTransform;
        if (parentRect == null) parentRect = canvas.transform as RectTransform;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPoint, uiCam, out localPoint);

        // Add offset
        rectTransform.anchoredPosition = localPoint + new Vector2(20f, 20f);
    }
}
