using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public ArmorData armorData;

    private Transform originalParent;
    private CanvasGroup canvasGroup;
    private Canvas rootCanvas;
    private bool equipped = false;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rootCanvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        equipped = false;
        originalParent = transform.parent;
        if (rootCanvas != null)
            transform.SetParent(rootCanvas.transform);
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
            transform.SetParent(originalParent);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void MarkEquipped()
    {
        equipped = true;
    }
}
