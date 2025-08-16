using UnityEngine;
using TMPro;

public class WeaponTooltip : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI effectsText;

    // ✅ NEW: keywords UI
    public TextMeshProUGUI keywordsText;

    [Header("Positioning")]
    public Canvas canvas;
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (canvas == null) canvas = GetComponentInParent<Canvas>();
        HideTooltip();
    }

    public void ShowTooltip(Weapon weapon, RectTransform target)
    {
        if (weapon == null) return;

        nameText.text = weapon.itemName;

        // Build damage info
        string dmg = "";
        if (weapon.damagePhysical > 0) dmg += $"Physical: {weapon.damagePhysical}\n";
        if (weapon.damageFire > 0) dmg += $"Fire: {weapon.damageFire}\n";
        if (weapon.damageElectric > 0) dmg += $"Electric: {weapon.damageElectric}\n";
        if (weapon.damageRadiation > 0) dmg += $"Radiation: {weapon.damageRadiation}\n";
        if (weapon.damageExplosive > 0) dmg += $"Explosive: {weapon.damageExplosive}\n";
        if (weapon.damagePlasma > 0) dmg += $"Plasma: {weapon.damagePlasma}\n";
        if (weapon.damageLaser > 0) dmg += $"Laser: {weapon.damageLaser}\n";
        if (weapon.damageChemical > 0) dmg += $"Chemical: {weapon.damageChemical}\n";
        if (weapon.damageViral > 0) dmg += $"Viral: {weapon.damageViral}\n";
        damageText.text = string.IsNullOrEmpty(dmg) ? "No direct damage" : dmg.TrimEnd();

        typeText.text = $"Type: {weapon.weaponType}";

        string effects = "";
        if (weapon.bleedChance > 0f) effects += $"Bleed: {(weapon.bleedChance * 100f):F0}%\n";
        if (weapon.poisonChance > 0f) effects += $"Poison: {(weapon.poisonChance * 100f):F0}%\n";
        if (weapon.stunChance > 0f) effects += $"Stun: {(weapon.stunChance * 100f):F0}%\n";
        if (weapon.criticalChance > 0f) effects += $"Critical: {(weapon.criticalChance * 100f):F0}%\n";
        effectsText.text = string.IsNullOrEmpty(effects) ? "No status effects" : effects.TrimEnd();

        // ✅ Keywords (from Item base class)
        if (keywordsText != null)
        {
            if (weapon.keyWords != null && weapon.keyWords.Count > 0)
            {
                // e.g., "Keywords: bleed, electric, heavy"
                keywordsText.text = $"Keywords: {string.Join(", ", weapon.keyWords)}";
                keywordsText.gameObject.SetActive(true);
            }
            else
            {
                keywordsText.gameObject.SetActive(false);
            }
        }

        PositionTooltip(target); // use your fixed positioning that converts World->Screen->Local
        gameObject.SetActive(true);
    }

    public void HideTooltip() => gameObject.SetActive(false);

    // Use the corrected positioning you’re using now. Example:
    private void PositionTooltip(RectTransform target)
    {
        if (canvas == null || rectTransform == null || target == null) return;

        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);
        Vector3 topRightWorld = corners[2];

        Camera uiCam = (canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : canvas.worldCamera;
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCam, topRightWorld);

        RectTransform parentRect = rectTransform.parent as RectTransform;
        if (parentRect == null) parentRect = canvas.transform as RectTransform;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPoint, uiCam, out localPoint);

        Vector2 desired = localPoint + new Vector2(20f, 20f);

        // Optional clamp to parent
        Vector2 parentSize = parentRect.rect.size;
        Vector2 tipSize = rectTransform.rect.size;
        Vector2 pvt = rectTransform.pivot;
        float minX = -parentSize.x * 0.5f + tipSize.x * pvt.x + 4f;
        float maxX = parentSize.x * 0.5f - tipSize.x * (1f - pvt.x) - 4f;
        float minY = -parentSize.y * 0.5f + tipSize.y * pvt.y + 4f;
        float maxY = parentSize.y * 0.5f - tipSize.y * (1f - pvt.y) - 4f;

        desired.x = Mathf.Clamp(desired.x, minX, maxX);
        desired.y = Mathf.Clamp(desired.y, minY, maxY);

        rectTransform.anchoredPosition = desired;
    }
}
