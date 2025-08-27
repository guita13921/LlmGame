using UnityEngine;
using UnityEngine.EventSystems;

public class UIHorizontalDrag_LR_HardStop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public RectTransform target;

    // ขอบเขตซ้าย/ขวาในหน่วย anchoredPosition.x
    public float minX = -600f;
    public float maxX = 0f;

    Vector2 _startAnchoredPos;
    Vector2 _pointerStart;

    void Reset() { target = GetComponent<RectTransform>(); }

    void OnEnable()
    {
        if (!target) return;
        // บังคับ anchor ให้อยู่ซ้าย (ไม่ Stretch) เพื่อให้แกน X แปลความหมายถูกต้อง
        target.anchorMin = new Vector2(0f, target.anchorMin.y);
        target.anchorMax = new Vector2(0f, target.anchorMax.y);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!target) return;

        _startAnchoredPos = target.anchoredPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            target.parent as RectTransform,
            eventData.position, eventData.pressEventCamera, out _pointerStart);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!target) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            target.parent as RectTransform,
            eventData.position, eventData.pressEventCamera, out var cur);

        float deltaX = cur.x - _pointerStart.x;             // ขวา=+, ซ้าย=-
        float proposedX = _startAnchoredPos.x + deltaX;
        float clampedX = Mathf.Clamp(proposedX, minX, maxX);

        // อัปเดตตำแหน่ง
        target.anchoredPosition = new Vector2(clampedX, _startAnchoredPos.y);

        // 🔒 Hard stop: ถ้าชนขอบ ให้รีเซ็ตจุดอ้างอิงการลากทันที
        if (!Mathf.Approximately(proposedX, clampedX))
        {
            _startAnchoredPos = target.anchoredPosition; // = จุดที่ขอบ
            _pointerStart = cur;                         // รีเซ็ตต้นทางของ pointer
        }
    }

    public void OnEndDrag(PointerEventData eventData) { }
}
