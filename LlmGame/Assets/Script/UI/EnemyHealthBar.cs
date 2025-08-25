using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    public Enemy enemy;
    public Image hpFill;
    public TMP_Text hpText;

    private void Update()
    {
        if (enemy == null) return;
        float percent = (enemy.maxHP <= 0) ? 0f : (float)enemy.currentHP / enemy.maxHP;
        if (hpFill != null)
            hpFill.fillAmount = percent;
        if (hpText != null)
            hpText.text = $"{enemy.currentHP}/{enemy.maxHP}";
    }
}
