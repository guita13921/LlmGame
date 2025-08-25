using TMPro;
using UnityEngine;

public class DamagePopupUI : MonoBehaviour
{
    public TMP_Text text;
    public float moveSpeed = 1f;
    public float lifetime = 1f;
    private float timer;

    public void Setup(int amount)
    {
        if (text != null)
            text.text = amount.ToString();
    }

    private void Update()
    {
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;
        timer += Time.deltaTime;
        if (timer >= lifetime)
            Destroy(gameObject);
    }
}
