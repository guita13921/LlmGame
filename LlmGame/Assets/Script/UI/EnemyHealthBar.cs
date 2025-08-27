using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    public Enemy enemy;
    public Slider hpSlider;   // Use Slider instead of Image
    public TMP_Text hpText;

    private void Start()
    {
        if (enemy != null && hpSlider != null)
        {
            hpSlider.minValue = 0;
            hpSlider.maxValue = enemy.maxHP; // set max HP
            hpSlider.value = enemy.currentHP;
        }
    }

    private void Update()
    {
        if (enemy == null) return;

        if (hpSlider != null)
            hpSlider.value = enemy.currentHP;

        if (hpText != null)
            hpText.text = $"{enemy.currentHP}/{enemy.maxHP}";
    }
}
