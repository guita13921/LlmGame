using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryWeaponDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Weapon weaponData;

    private Transform originalParent;
    private CanvasGroup canvasGroup;
    private Canvas rootCanvas;
    private bool equipped = false;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        rootCanvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        equipped = false;
        originalParent = transform.parent;
        if (rootCanvas != null)
            transform.SetParent(rootCanvas.transform, true);
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        if (!equipped)
        {
            transform.SetParent(originalParent, true);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>Call this from the drop target when the weapon has been equipped.</summary>
    public void MarkEquipped()
    {
        equipped = true;
    }
}
